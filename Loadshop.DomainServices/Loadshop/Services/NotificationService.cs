using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Loadshop.Data;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Utility;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Loadshop.DomainServices.Constants;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Utilities;
using Loadshop.DomainServices.Loadshop.Services.Repositories;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class NotificationService : INotificationService
    {
        private static readonly string LOADSHOP_URL_KEY = "LoadShopUrl";
        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;
        private readonly IConfigurationRoot _configuration;
        private readonly ISMSService _smsService;
        private readonly IEmailService _emailService;
        private readonly IRatingService _ratingService;
        private readonly ILoadQueryRepository _loadQueryRepository;
        private readonly ServiceUtilities _serviceUtilities;
        private readonly IDateTimeProvider _dateTime;
        private const int radius = 3956;
        private const decimal pidiv180 = 0.017453293m;
        private static bool _isNotificationsRunning;
        private static readonly string _emailRowStyle = "font-family: objektiv-mk1, sans-serif;font-size: 14px;line-height: 20px;";
        private static readonly string _aMessageFromLoadshop = "<span><i style='white-space: nowrap;'>A&nbsp;message&nbsp;from&nbsp;</i></span>";
        private string _logo => $"<img height=\"22\" width=\"114\" style=\"height: 22px; width: 114px;\" src=\"{_configuration["LoadShopLogoImageUrl"]}\"></img>";
        private static readonly string _anchorStyle = "color: #23aae6;";
        private static readonly string _colorBarStyle = "height: 8px; border: 0;";
        private string _emailBodyOpeningHtml =>
            $@"
<table width='100%' style='width: 100%; background-color: #232837;'>
    <tr style='padding: 0;'>
        <td width='100%' style='padding: 0.571em 1em 50px 1em; width: 100%;'>
            <table width='100%' style='width: 100%; table-layout: auto; color: #dedede;' cellspacing='0'>
                <tr style='{_emailRowStyle}'>
                    <td style='padding: 0;'>{_aMessageFromLoadshop}</td>
                    <td style='padding: 0;' width='100%'>{_logo}</td>
                </tr>
                <tr style='{_emailRowStyle}; padding: 0;'>
                    <td style='padding: 0;' colspan='2' width='100%;'>
                        <table width='100%' cellspacing='0' style='color: #dedede;'>
                            <tr style='padding: 0;'>
                                <td width='25%' style='padding: 1em 0;'><hr size='8' style='{_colorBarStyle} color: #23aae6; background-color: #23aae6'/></td>
                                <td width='25%' style='padding: 1em 0;'><hr size='8' style='{_colorBarStyle} color: #31c2ae; background-color: #31c2ae'/></td>
                                <td width='25%' style='padding: 1em 0;'><hr size='8' style='{_colorBarStyle} color: #ace171; background-color: #ace171'/></td>
                                <td width='25%' style='padding: 1em 0;'><hr size='8' style='{_colorBarStyle} color: #ffffff; background-color: #ffffff'/></td>
                            </tr>
                        </table>
                    </td>
                </tr>
                <tr style='{_emailRowStyle}; padding: 0;'>
                    <td style='padding: 0;' colspan='2' width='100%;'>
                        ";

        private static readonly string _emailBodyClosingHtml = $@"
                    </td>
                </tr>
            </table>
        </td>
    </tr>
</table>";

        public NotificationService(LoadshopDataContext context,
            IMapper mapper,
            IConfigurationRoot configuration,
            ISMSService smsService,
            IEmailService emailService,
            IRatingService ratingService,
            IDateTimeProvider dateTime,
            ServiceUtilities serviceUtilities,
            ILoadQueryRepository loadQueryRepository)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _smsService = smsService;
            _emailService = emailService;
            _ratingService = ratingService;
            _dateTime = dateTime;
            _serviceUtilities = serviceUtilities;
            _loadQueryRepository = loadQueryRepository;
        }

        public async Task CreateNotificationDetailsAsync()
        {
            if (_isNotificationsRunning) return;

            try
            {
                _isNotificationsRunning = true;
                var loads = await _loadQueryRepository.GetLoadDetailViewUnprocessedAsync();

                var loadIds = loads.Select(x => x.LoadId).Distinct().ToList();
                var equipmentIds = loads.Select(x => x.EquipmentId).Distinct().ToList();
                var loadStopGroups = _context.LoadStops.Where(x => loadIds.Contains(x.LoadId)).AsEnumerable().GroupBy(x => x.LoadId).ToList();
                var equipments = await _context.Equipment.Where(x => equipmentIds.Contains(x.EquipmentId)).ToListAsync();

                foreach (var load in loads)
                {
                    load.LoadStops = loadStopGroups.SingleOrDefault(x => x.Key == load.LoadId)?.OrderBy(x => x.StopNbr).ToList();
                    load.Equipment = equipments.SingleOrDefault(x => x.EquipmentId == load.EquipmentId);

                    var firstStop = load.LoadStops?.FirstOrDefault();
                    var lastStop = load.LoadStops?.LastOrDefault();
                    if (firstStop != null && lastStop != null)
                    {
                        // If new load then find all matching UserLanes and send out matching notifications
                        if (load.TransactionTypeId == TransactionTypes.New)
                        {
                            var scacs = await _context.LoadCarrierScacs.Where(x => x.LoadId == load.LoadId).ToDictionaryAsync(x => x.Scac);

                            if (scacs.Count == 1)
                            {
                                var users = await _context.Users.AsNoTracking()
                                                    .Where(x => x.PrimaryScac == scacs.Keys.Single())
                                                    .ToListAsync();

                                foreach (var user in users)
                                {
                                    var scac = scacs.Values.FirstOrDefault();
                                    if (scac?.ContractRate > load.LineHaulRate)
                                    {
                                        continue;
                                    }

                                    NotificationDataEntity notificationData = CreateNotificationData(load, firstStop, lastStop, user.UserId, MessageTypeConstants.Email_SingleCarrierScac);
                                    _context.NotificationDatas.Add(notificationData);
                                }
                            }
                            else if (scacs.Count > 0)
                            {
                                var userLanes = await _context.UserLanes.AsNoTracking()
                                    .Include(x => x.UserLaneMessageTypes)
                                    .Include(x => x.UserLaneEquipments)
                                    .Include(x => x.User)
                                    .Where(x => x.User.IsNotificationsEnabled)
                                    .ToListAsync();
                                foreach (var userLane in userLanes)
                                {
                                    if (await _context.NotificationDatas.AsNoTracking()
                                                    .AnyAsync(x => x.LoadId == load.LoadId && x.UserId == userLane.UserId))
                                    {
                                        continue;
                                    }

                                    var userScac = userLane.User.PrimaryScac;
                                    if (scacs.Any() && (string.IsNullOrWhiteSpace(userScac) || !scacs.ContainsKey(userScac) || scacs[userScac]?.ContractRate > load.LineHaulRate))
                                    {
                                        continue;
                                    }

                                    if (!IsUserLaneMatchLoad(userLane, load, firstStop, lastStop))
                                    {
                                        continue;
                                    }

                                    foreach (var messageType in userLane.UserLaneMessageTypes)
                                    {
                                        NotificationDataEntity notificationData = CreateNotificationData(load, firstStop, lastStop, userLane.UserId, messageType.MessageTypeId);
                                        _context.NotificationDatas.Add(notificationData);
                                    }
                                }
                            }
                        }
                        // If Accepted or Pending accepted then send out the accepted notification to the user on the Claim
                        else if (load.TransactionTypeId == TransactionTypes.Accepted || load.TransactionTypeId == TransactionTypes.Pending)
                        {
                            var claimUserId = _context.LoadClaims.AsNoTracking()
                                .Where(x => x.LoadTransactionId == load.LoadTransactionId)
                                .Select(x => x.UserId)
                                .FirstOrDefault();

                            if (claimUserId != default(Guid))
                            {
                                var notificationApplicableUserNotifications = await GetApplicableUserNotifications(claimUserId);

                                foreach (var messageType in notificationApplicableUserNotifications)
                                {
                                    NotificationDataEntity notificationData = CreateNotificationData(load, firstStop, lastStop, claimUserId, messageType.MessageTypeId);
                                    _context.NotificationDatas.Add(notificationData);
                                }
                            }
                        }
                        // If Removed or Updated then send out a notification to the user if someone has Pending accepted it
                        else if (load.TransactionTypeId == TransactionTypes.Removed || load.TransactionTypeId == TransactionTypes.Updated)
                        {
                            var transaction = await _context.LoadTransactions.AsNoTracking()
                                .Include(x => x.Claim)
                                .Where(x => x.LoadId == load.LoadId && x.TransactionTypeId == TransactionTypes.Pending)
                                .OrderByDescending(x => x.CreateDtTm)
                                .FirstOrDefaultAsync();

                            if (transaction != null && transaction.Claim != null)
                            {
                                var notificationApplicableUserNotifications = await GetApplicableUserNotifications(transaction.Claim.UserId);

                                foreach (var messageType in notificationApplicableUserNotifications)
                                {
                                    NotificationDataEntity notificationData = CreateNotificationData(load, firstStop, lastStop, transaction.Claim.UserId, messageType.MessageTypeId);
                                    _context.NotificationDatas.Add(notificationData);
                                }
                            }
                        }

                        var t = _context.LoadTransactions.SingleOrDefault(x => x.LoadTransactionId == load.LoadTransactionId);
                        if (t == null)
                        {
                            throw new Exception($"Unable to find LoadTransaction {load.LoadTransactionId}");
                        }
                        t.ProcessedDtTm = _dateTime.Now;
                        await _context.SaveChangesAsync("system");
                    }
                }
            }
            finally
            {
                _isNotificationsRunning = false;
            }
        }

        /// <summary>
        /// Only cell phone and email are applicable for sending notifications, only cells phones can have their notifications disabled
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task<List<UserNotificationEntity>> GetApplicableUserNotifications(Guid userId)
        {
            var notificationApplicableUserNotifications = await _context.UserNotifications.AsNoTracking()
                .Where(x => x.UserId == userId)
                .Where(x => (x.MessageTypeId == MessageTypeConstants.CellPhone && x.NotificationEnabled) ||
                            x.MessageTypeId == MessageTypeConstants.Email)
                .ToListAsync();

            return notificationApplicableUserNotifications;

        }

        private NotificationDataEntity CreateNotificationData(LoadDetailViewEntity load, LoadStopEntity firstStop, LoadStopEntity lastStop, Guid userId, string messageTypeId)
        {
            var user = _context.Users.Where(x => x.UserId == userId).SingleOrDefault();

            return new NotificationDataEntity()
            {
                LoadId = load.LoadId,
                UserId = userId,
                TransactionTypeId = load.TransactionTypeId,
                Origin = $"{firstStop.City}, {firstStop.State}",
                Dest = $"{lastStop.City}, {lastStop.State}",
                LineHaulRate = user == null ? load.LineHaulRate : _serviceUtilities.GetContractRate(_context, load.LoadId, user.IdentUserId) ?? load.LineHaulRate,
                FuelRate = load.FuelRate,
                EquipmentDesc = load.Equipment.EquipmentDesc,
                OriginDtTm = firstStop.LateDtTm,
                DestDtTm = lastStop.LateDtTm,
                Miles = load.Miles,
                Notification = new NotificationEntity()
                {
                    MessageTypeId = messageTypeId
                }
            };
        }

        private static bool IsUserLaneMatchLoad(UserLaneEntity userLane, LoadDetailViewEntity load, LoadStopEntity firstStop, LoadStopEntity lastStop)
        {
            if (userLane.UserLaneEquipments != null && userLane.UserLaneEquipments.Any() && !userLane.UserLaneEquipments.Any(x => x.EquipmentId == load.EquipmentId))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(userLane.OrigState) && !userLane.OrigLat.HasValue && !userLane.OrigLng.HasValue)
            {
                if (string.Compare(userLane.OrigState, firstStop.State, true) != 0)
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(userLane.DestState) && !userLane.DestLat.HasValue && !userLane.DestLng.HasValue)
            {
                if (string.Compare(userLane.DestState, lastStop.State, true) != 0)
                {
                    return false;
                }
            }

            if (userLane.OrigLat.HasValue && userLane.OrigLng.HasValue)
            {
                var d = CalculateDistance(firstStop.Latitude, firstStop.Longitude, userLane.OrigLat.Value, userLane.OrigLng.Value);
                if (d > (userLane.OrigDH ?? 50))
                {
                    return false;
                }
            }

            if (userLane.DestLat.HasValue && userLane.DestLng.HasValue)
            {
                var d = CalculateDistance(lastStop.Latitude, lastStop.Longitude, userLane.DestLat.Value, userLane.DestLng.Value);
                if (d > (userLane.DestDH ?? 50))
                {
                    return false;
                }
            }

            return true;
        }

        private static double CalculateDistance(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
        {
            var radlat1 = Convert.ToDouble(lat1 * pidiv180);
            var radlng1 = Convert.ToDouble(lng1 * pidiv180);
            var radlat2 = Convert.ToDouble(lat2 * pidiv180);
            var radlng2 = Convert.ToDouble(lng2 * pidiv180);
            var dLat = radlat2 - radlat1;
            var dLng = radlng2 - radlng1;
            var a = Math.Pow((Math.Sin(dLat / 2)), 2) + Math.Cos(radlat1) * Math.Cos(radlat2) * Math.Pow((Math.Sin(dLng / 2)), 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = radius * c;
            return d;
        }

        public async Task CreateNotificationsAsync()
        {
            DateTime now = _dateTime.Now;
            var notifications = _context.Notifications
                                .Include(x => x.NotificationData).ThenInclude(x => x.User).ThenInclude(x => x.UserNotifications)
                                .Include(x => x.NotificationData).ThenInclude(x => x.Load).ThenInclude(x => x.Contacts)
                                .Where(x => !x.ProcessedDtTm.HasValue)
                                .ToList();


            var loadMatchNotifications = new List<NotificationEntity>();

            foreach (var notification in notifications)
            {
                if (GuardNotification(notification))
                {
                    if (notification.MessageTypeId == MessageTypeConstants.Email && notification.NotificationData.TransactionTypeId == TransactionTypes.New)
                    {
                        loadMatchNotifications.Add(notification);
                    }
                    else
                    {
                        notification.ProcessedDtTm = now;
                        var notificationMessages = await CreateNotificationMessagesAsync(notification);

                        if (notificationMessages != null && notificationMessages.Any())
                        {
                            _context.NotificationMessages.AddRange(notificationMessages);
                        }

                        await _context.SaveChangesAsync("system");

                        if (notificationMessages != null)
                        {
                            foreach (var notificationMessage in notificationMessages)
                            {
                                if (notification.MessageTypeId == MessageTypeConstants.Email ||
                                    notification.MessageTypeId == MessageTypeConstants.Email_SingleCarrierScac)
                                {
                                    _emailService.SendMailMessage(notificationMessage, false);
                                }
                                else if (notification.MessageTypeId == MessageTypeConstants.CellPhone)
                                {
                                    SendSMSMessage(notificationMessage);
                                }
                            }
                        }
                    }
                }
                else
                {
                    notification.ProcessedDtTm = now;
                    await _context.SaveChangesAsync("system");
                }
            }


            if (loadMatchNotifications.Count > 0)
            {
                var sortedLoadMatchNotifications = loadMatchNotifications.OrderBy(x => x.NotificationData.UserId)
                                                                         .ThenBy(x => x.NotificationData.OriginDtTm)
                                                                         .ThenBy(x => x.NotificationData.DestDtTm)
                                                                         .ThenBy(x => x.NotificationData.Load.ReferenceLoadDisplay).ToList();
                await ProcessLoadMatchNotifications(sortedLoadMatchNotifications);
            }
        }

        private async Task ProcessLoadMatchNotifications(List<NotificationEntity> loadMatchNotifications)
        {
            Guid keyUserId = new Guid();
            var userLoadMatchNotifications = new List<NotificationEntity>();

            foreach (var loadMatchNotification in loadMatchNotifications)
            {
                if (userLoadMatchNotifications.Count == 0)
                {
                    keyUserId = loadMatchNotification.NotificationData.UserId;
                }
                else
                {
                    if (loadMatchNotification.NotificationData.UserId != keyUserId)
                    {
                        await ProcessUserLoadMatchNotificationsAsync(userLoadMatchNotifications);
                        userLoadMatchNotifications.Clear();
                        keyUserId = loadMatchNotification.NotificationData.UserId;
                    }
                }

                userLoadMatchNotifications.Add(loadMatchNotification);
            }

            if (userLoadMatchNotifications.Count > 0)
            {
                await ProcessUserLoadMatchNotificationsAsync(userLoadMatchNotifications);
            }
        }

        private async Task ProcessUserLoadMatchNotificationsAsync(List<NotificationEntity> userLoadMatchNotifications)
        {
            DateTime now = _dateTime.Now;
            var messages = new List<NotificationMessageEntity>();

            if (userLoadMatchNotifications.Count == 1)
            {
                var userLoadMatchNotification = userLoadMatchNotifications[0];

                userLoadMatchNotification.ProcessedDtTm = now;
                messages = await CreateNotificationMessagesAsync(userLoadMatchNotification);

                if (messages != null)
                {
                    _context.NotificationMessages.AddRange(messages);
                }

                await _context.SaveChangesAsync("system");
            }
            else
            {
                var loadNotifications = new List<NotificationMessageEntity>();
                var disinctLoadCount = userLoadMatchNotifications.Select(x => x.NotificationData.LoadId).Distinct().Count();

                if (disinctLoadCount == 1)
                {
                    messages = await CreateNotificationMessagesAsync(userLoadMatchNotifications.First());
                }
                else
                {
                    var email = CreateEmailNotificationMessage(userLoadMatchNotifications);
                    if (email != null)
                    {
                        messages.Add(email);
                    }
                }

                foreach (var userLoadMatchNotification in userLoadMatchNotifications)
                {
                    userLoadMatchNotification.ProcessedDtTm = now;

                    if (messages != null && messages.Any())
                    {
                        var userLoadMatchNotificationMessage = messages.Select(x => new NotificationMessageEntity
                        {
                            NotificationId = userLoadMatchNotification.NotificationId,
                            To = x.To,
                            Subject = x.Subject,
                            Message = x.Message,
                            MachineName = x.MachineName,
                        }).ToList();

                        loadNotifications.AddRange(userLoadMatchNotificationMessage);
                    }
                }

                _context.NotificationMessages.AddRange(loadNotifications);
                await _context.SaveChangesAsync("system");
            }

            if (messages != null && messages.Any())
            {
                messages.ForEach(x => _emailService.SendMailMessage(x, false));
            }
        }

        private async Task<List<NotificationMessageEntity>> CreateNotificationMessagesAsync(NotificationEntity notification)
        {
            if (notification.NotificationData == null)
            {
                return null;
            }

            var messageType = notification.MessageTypeId;
            var isEmailNotification = messageType == MessageTypeConstants.Email ||
                messageType == MessageTypeConstants.Email_SingleCarrierScac;
            var isSmsNotification = messageType == MessageTypeConstants.CellPhone;

            if (isEmailNotification)
            {
                return await CreateEmailNotificationMessageAsync(notification);
            }
            else if (isSmsNotification)
            {
                return CreateSMSNotificationMessage(notification);
            }
            return null;
        }

        private NotificationMessageEntity CreateEmailNotificationMessage(List<NotificationEntity> loadMatchNotifications)
        {
            var notificationMessage = new NotificationMessageEntity();
            notificationMessage.Subject = "Multiple Loadshop Favorite Matches";
            notificationMessage.To = GetTo(loadMatchNotifications[0].NotificationData.User, MessageTypeConstants.Email).FirstOrDefault();
            notificationMessage.MachineName = Environment.MachineName;
            var processedLoads = new List<Guid>();
            var liHtml = string.Empty;

            foreach (var notification in loadMatchNotifications)
            {
                if (!processedLoads.Contains(notification.NotificationData.LoadId))
                {
                    var n = notification.NotificationData;
                    processedLoads.Add(n.LoadId);

                    liHtml +=
                        $@"<li>
                        <a href='{_configuration[LOADSHOP_URL_KEY]}loads/detail/{n.LoadId}?at={AuditTypeData.FavoritesMatchEmailView.ToString("G")}' style='{_anchorStyle}'>
                        Load # {n.Load.ReferenceLoadDisplay} - {n.Origin} ({n.OriginDtTm.ToString("MM/dd/yyyy HH:mm")}) to {n.Dest} ({n.DestDtTm.ToString("MM/dd/yyyy HH:mm")}) - {n.EquipmentDesc} - {n.LineHaulRate} + {n.FuelRate} = {n.LineHaulRate + n.FuelRate}
                        </a>
                        <br />&nbsp;<br />
                        </li>";
                }
            }

            if (!string.IsNullOrWhiteSpace(liHtml))
            {
                notificationMessage.Message +=
                    $@"{_emailBodyOpeningHtml}
                        <p>The following loads match one of your Favorite Lanes created in loadshop. If interested, click on any of the links below to see more details of the load and have the opportunity to 'book it' instantly.</p>
                        <br/>
                        <ul>{liHtml}</ul>
                        {_emailBodyClosingHtml}";

                return notificationMessage;
            }
            else
            {
                return null;
            }
        }

        private List<NotificationMessageEntity> CreateSMSNotificationMessage(NotificationEntity notification)
        {
            var transactionType = notification.NotificationData.TransactionTypeId.ToLower();
            switch (transactionType)
            {
                case "new":
                    {
                        var phoneUserNotifications = GetTo(notification.NotificationData.User, MessageTypeConstants.CellPhone);

                        return phoneUserNotifications.Select(x => new NotificationMessageEntity()
                        {
                            Message = CreateSMSMessage(notification.NotificationData),
                            Subject = CreateSubject(notification.NotificationData, "Matched"),
                            To = x,
                            NotificationId = notification.NotificationId,
                            MachineName = Environment.MachineName
                        }).ToList();
                    }
                case "pending":
                case "accepted":
                case "removed":
                    return null;
                default:
                    return null;
            }
        }

        private async Task<List<NotificationMessageEntity>> CreateEmailNotificationMessageAsync(NotificationEntity notification)
        {
            var transactionType = notification.NotificationData.TransactionTypeId.ToLower();
            var notificationMessage = new NotificationMessageEntity();
            notificationMessage.NotificationId = notification.NotificationId;
            notificationMessage.MachineName = Environment.MachineName;

            switch (transactionType)
            {
                case "new":
                    {
                        notificationMessage.To = GetTo(notification.NotificationData.User, MessageTypeConstants.Email).FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(notificationMessage.To))
                        {
                            if (notification.MessageTypeId == MessageTypeConstants.Email)
                            {
                                notificationMessage.Message = CreateNotificationEmailMessage(notification.NotificationData);
                                notificationMessage.Subject = CreateNotificationSubject(notification.NotificationData);
                                return new List<NotificationMessageEntity>() { notificationMessage };
                            }
                            else if (notification.MessageTypeId == MessageTypeConstants.Email_SingleCarrierScac)
                            {
                                notificationMessage.Message = CreateSingleCarrierScacEmailMessage(notification.NotificationData);
                                notificationMessage.Subject = CreateSingleCarrierScacSubject(notification.NotificationData);
                                return new List<NotificationMessageEntity>() { notificationMessage };
                            }
                        }

                        return null;
                    }
                case "pending":
                    {
                        return null;
                    }
                case "accepted":
                    {
                        return null;
                    }
                case "removed":
                    {
                        notificationMessage.To = GetTo(notification.NotificationData.User, MessageTypeConstants.Email).FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(notificationMessage.To))
                        {
                            var reason = await _ratingService.GetRatingReason(notification.NotificationData.LoadId);

                            notificationMessage.Message = CreateRemovedEmailMessage(notification.NotificationData, reason);
                            notificationMessage.Subject = CreateRemovedSubject(notification.NotificationData);
                            notificationMessage.CC = GetRemovedCC(notification.NotificationData.Load.Contacts);

                            return new List<NotificationMessageEntity>() { notificationMessage };
                        }

                        return null;
                    }
                default:
                    return null;
            }
        }

        private void SendSMSMessage(NotificationMessageEntity notificationMessage)
        {
            var messages = notificationMessage.Message.Split('|');
            if (messages.Count() == 2)
            {
                _smsService.SendMessage(notificationMessage.To, messages[0]);
                _smsService.SendMessage(notificationMessage.To, messages[1]);
            }
        }

        public NotificationMessageEntity SendPendingEmail(LoadEntity load, UserEntity user, LoadContactEntity contact, string userContextEmail, LoadClaimEntity claim)
        {
            if (load != null && user != null && contact != null)
            {
                var message = CreatePendingEmailNotification(user, load, contact, userContextEmail, claim);
                _emailService.SendMailMessage(message, false);
                return message;
            }
            return null;
        }

        public NotificationMessageEntity SendCarrierRemovedEmail(LoadEntity load, UserEntity user, IEnumerable<LoadContactEntity> contacts, string userContextEmail, string reason)
        {
            if (load != null && user != null && contacts != null)
            {
                var message = CreateCarrierRemovedEmailNotification(user, load, contacts, userContextEmail, reason);
                _emailService.SendMailMessage(message, false);
                return message;
            }
            return null;
        }

        public NotificationMessageEntity SendShipperFeeChangeEmail(CustomerEntity original, CustomerProfileData updates, string changeUser)
        {
            if (updates == null)
                return null;

            var recipientEmail = _configuration.GetValue<string>("AccountsReceivableEmail");
            var message = CreateShipperFeeChangeEmailNotification(original, updates, recipientEmail, changeUser);
            _emailService.SendMailMessage(message, false);
            return message;
        }

        private NotificationMessageEntity CreatePendingEmailNotification(UserEntity user, LoadEntity load, LoadContactEntity contact, string userContextEmail, LoadClaimEntity claim)
        {
            var userEmail = GetTo(user, MessageTypeConstants.Email).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                userEmail = userContextEmail;
            }

            return new NotificationMessageEntity()
            {
                Message = CreatePendingEmailMessage(load, user, userEmail, contact, claim),
                Subject = CreatePendingSubject(load),
                To = userEmail,
                CC = GetPendingCC(user, contact)
            };
        }

        private NotificationMessageEntity CreateCarrierRemovedEmailNotification(UserEntity user, LoadEntity load, IEnumerable<LoadContactEntity> contacts, string userContextEmail, string reason)
        {
            var userEmail = GetTo(user, MessageTypeConstants.Email).FirstOrDefault();
            var shipperEmails = contacts.Select(x => x.Email).ToArray();
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                userEmail = userContextEmail;
            }

            return new NotificationMessageEntity()
            {
                Message = CreateCarrierRemovedEmailMessage(user, reason),
                Subject = CreateCarrierRemovedSubject(load),
                To = userEmail,
                CC = string.Join(";", shipperEmails)
            };
        }

        private NotificationMessageEntity CreateShipperFeeChangeEmailNotification(CustomerEntity original, CustomerProfileData updates, string emailRecipient, string changeUser)
        {
            return new NotificationMessageEntity()
            {
                Message = CreateShipperFeeChangeEmailMessage(original, updates, changeUser),
                Subject = "LoadShop Customer Fee Change",
                To = emailRecipient
            };
        }

        private string GetRemovedCC(List<LoadContactEntity> contacts)
        {
            var results = new List<string>();
            if (contacts != null)
            {
                var contact = contacts.FirstOrDefault();
                if (contact != null)
                {
                    results.Add(contact.Email);
                }
            }

            results.Add(_configuration["LoadshopEmail"]);
            return string.Join(";", results);
        }

        private string GetPendingCC(UserEntity user, LoadContactEntity contact)
        {
            var results = new List<string>();
            if (contact != null)
            {
                results.Add(contact.Email);
            }

            //check number of booked loads
            int numberOfBookedLoadsToCCLoadshopEmail = 3;
            int.TryParse(_configuration["LoadBoardNumberOfBookedLoadsToCCLoadshopEmail"], out numberOfBookedLoadsToCCLoadshopEmail);
            int totalBookedLoadsByCarrier = _loadQueryRepository.GetNumberOfBookedLoadsForCarrierByUserIdentId(user.IdentUserId);

            if (totalBookedLoadsByCarrier < numberOfBookedLoadsToCCLoadshopEmail)
            {
                results.Add(_configuration["LoadshopEmail"]);
            }

            return string.Join(";", results);
        }

        private string CreateSubject(NotificationDataEntity notificationDetail, string type)
        {
            return $"Load {type} - {notificationDetail.Origin} to {notificationDetail.Dest} - {notificationDetail.EquipmentDesc}";
        }

        private string CreateNotificationSubject(NotificationDataEntity notificationDetail)
        {
            return $"Loadshop Favorite Match - {notificationDetail.Origin} ({notificationDetail.OriginDtTm.ToString("MM/dd/yyyy")}) to {notificationDetail.Dest} ({notificationDetail.DestDtTm.ToString("MM/dd/yyyy HH:mm")}) - {notificationDetail.EquipmentDesc}";
        }

        private string CreateSingleCarrierScacSubject(NotificationDataEntity notificationDetail)
        {
            return $"Loadshop Ready To Book - {notificationDetail.Origin} ({notificationDetail.OriginDtTm.ToString("MM/dd/yyyy")}) to {notificationDetail.Dest} ({notificationDetail.DestDtTm.ToString("MM/dd/yyyy HH:mm")}) - {notificationDetail.EquipmentDesc}";
        }

        private string CreateRemovedSubject(NotificationDataEntity notificationDetail)
        {
            return $"Booking Cancelled – load # {notificationDetail.Load.ReferenceLoadDisplay} {notificationDetail.Origin} to {notificationDetail.Dest} - {notificationDetail.EquipmentDesc}";
        }

        private string CreatePendingSubject(LoadEntity load)
        {
            var origin = load.LoadStops.OrderBy(x => x.StopNbr).First();
            var dest = load.LoadStops.OrderByDescending(x => x.StopNbr).First();
            return $"Success! Loadshop Booking Confirmation for # {load.ReferenceLoadDisplay} {origin.City}, {origin.State} to {dest.City}, {dest.State} - {load.Equipment.EquipmentDesc}";
        }

        private string CreateCarrierRemovedSubject(LoadEntity load)
        {
            var origin = load.LoadStops.OrderBy(x => x.StopNbr).First();
            var dest = load.LoadStops.OrderByDescending(x => x.StopNbr).First();
            return $"Booking Cancelled – load # {load.ReferenceLoadDisplay} {origin.City}, {origin.State} to {dest.City}, {dest.State} - {load.Equipment.EquipmentDesc}";
        }

        private string CreateShipperFeeChangeSubject(CustomerProfileData customer)
        {
            return $"Shipper Fees Changed For {customer.Name}";
        }

        private IEnumerable<string> GetTo(UserEntity user, string messageType)
        {
            List<UserNotificationEntity> notifications = null;
            if (messageType.Equals(MessageTypeConstants.CellPhone, StringComparison.OrdinalIgnoreCase))
            {
                // ensure cell phone / sms messages have notifications enabled
                notifications = _context.UserNotifications.AsNoTracking()
                                    .Where(x => x.UserId == user.UserId)
                                    .Where(x => x.MessageTypeId.ToLower() == messageType.ToLower())
                                    .Where(x => x.NotificationEnabled)
                                    .ToList();
            }
            else
            {
                var notification = _context.UserNotifications.AsNoTracking()
                                    .Where(x => x.UserId == user.UserId && x.MessageTypeId.ToLower() == messageType.ToLower())
                                    .FirstOrDefault();
                if (notification != null)
                {
                    notifications = new List<UserNotificationEntity>() { notification };
                }
            }

            return notifications != null ? notifications.Select(x => x.NotificationValue).ToList() : new List<string>();
        }

        private string CreateSMSMessage(NotificationDataEntity notificationDetail)
        {
            var n = notificationDetail;
            return $@"Load matched {n.Origin} to {n.Dest} ({n.OriginDtTm.ToString("MM/dd/yyyy HH:mm")}) ${n.LineHaulRate + n.FuelRate}|{_configuration[LOADSHOP_URL_KEY]}loads/detail/{n.LoadId}?at={AuditTypeData.FavoritesMatchEmailView.ToString("G")}";
        }

        private string CreateNotificationEmailMessage(NotificationDataEntity notificationDetail)
        {
            var n = notificationDetail;
            return $@"{_emailBodyOpeningHtml}
                <p>
                    The following load matches one of your Favorite Lanes created in loadshop. If interested, click on the link below to see more details and have the opportunity to 'book it' instantly.
                </p>
                <br/>
                <ul>
                    <li>
                        <a href='{_configuration[LOADSHOP_URL_KEY]}loads/detail/{n.LoadId}?at={AuditTypeData.FavoritesMatchEmailView.ToString("G")}' style='{_anchorStyle}'>
                            Load # {n.Load.ReferenceLoadDisplay} - {n.Origin} ({n.OriginDtTm.ToString("MM/dd/yyyy HH:mm")}) to {n.Dest} ({n.DestDtTm.ToString("MM/dd/yyyy HH:mm")}) - {n.EquipmentDesc} - {n.LineHaulRate} + {n.FuelRate} = {n.LineHaulRate + n.FuelRate}
                        </a>
                    </li>
                </ul>
                {_emailBodyClosingHtml}";
        }

        private string CreateSingleCarrierScacEmailMessage(NotificationDataEntity notificationDetail)
        {
            var n = notificationDetail;
            return
            $@"
            {_emailBodyOpeningHtml}
            <p>
            The following load is ready to book. Please click on the link below to see the details and 'book it' instantly.
            </p>
            <br/>
            <ul>
            <li>
            <a href='{_configuration[LOADSHOP_URL_KEY]}loads/detail/{n.LoadId}?at={AuditTypeData.ReadyToBookEmailView.ToString("G")}' style='{_anchorStyle}'>
            Load # {n.Load.ReferenceLoadDisplay} - {n.Origin} ({n.OriginDtTm.ToString("MM/dd/yyyy HH:mm")}) to {n.Dest} ({n.DestDtTm.ToString("MM/dd/yyyy HH:mm")}) - {n.EquipmentDesc} - {n.LineHaulRate} + {n.FuelRate} = {n.LineHaulRate + n.FuelRate}
            </a>
            </li>
            </ul>
            {_emailBodyClosingHtml}";
        }

        private string CreateRemovedEmailMessage(NotificationDataEntity notificationDetail, string reason)
        {
            var firstNameDisplay = notificationDetail.User.FirstName ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(firstNameDisplay))
            {
                firstNameDisplay = $"<p>{GetTitleCase(firstNameDisplay)},</p>";
            }

            var cancelledText = string.IsNullOrWhiteSpace(reason) ? "This load has been cancelled." : $"This load has been cancelled due to a {reason} related issue.";
            return $@"{_emailBodyOpeningHtml}
                {firstNameDisplay}
                <p>{cancelledText} We've included the shipper and carrier representatives for the cancelled load on this email should you have any questions or follow ups.
                Please visit the Loadshop <a href='{_configuration[LOADSHOP_URL_KEY]}' style='{_anchorStyle}'>loadshop</a> website for additional opportunities.</p>
            {_emailBodyClosingHtml}";
        }

        private string CreateCarrierRemovedEmailMessage(UserEntity user, string reason)
        {
            var firstNameDisplay = user.FirstName ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(firstNameDisplay))
            {
                firstNameDisplay = $"<p>{GetTitleCase(firstNameDisplay)},</p>";
            }

            return $@"{_emailBodyOpeningHtml}
                {firstNameDisplay}
                <p>This load has been cancelled due to a {reason} related issue. We've included the shipper and carrier representatives for the cancelled load on this email should you have any questions or follow ups.
                Please visit the Loadshop <a href='{_configuration[LOADSHOP_URL_KEY]}' style='{_anchorStyle}'>loadshop</a> website for additional opportunities.</p>
            {_emailBodyClosingHtml}";
        }

        private string CreateShipperFeeChangeEmailMessage(CustomerEntity original, CustomerProfileData updates, string changeUser)
        {
            string feeData = $@"<br/>
                <h1>Current Fees</h1>
                <table>
                <tr><td style=""width: 200px"">In Network Flat Fee:</td><td>{updates.InNetworkFlatFee:C}</td></tr>
                <tr><td style=""width: 200px"">In Network Percent Fee:</td><td>{updates.InNetworkPercentFee:P}</td></tr>
                <tr><td style=""width: 200px"">In Network Fee Add:</td><td>{(updates.InNetworkFeeAdd ? "Yes" : "No")}</td></tr>
                <tr><td style=""width: 200px"">Out Network Flat Fee:</td><td>{updates.OutNetworkFlatFee:C}</td></tr>
                <tr><td style=""width: 200px"">Out Network Percent Fee:</td><td>{updates.OutNetworkPercentFee:P}</td></tr>
                <tr><td style=""width: 200px"">Out Network Fee Add:</td><td>{(updates.OutNetworkFeeAdd ? "Yes" : "No")}</td></tr>
                </table>";

            if (original != null)
            {
                feeData += $@"<br/>
                <h1>Previous Fees</h1>
                <table>
                <tr><td style=""width: 200px"">In Network Flat Fee:</td><td>{original.InNetworkFlatFee:C}</td></tr>
                <tr><td style=""width: 200px"">In Network Percent Fee:</td><td>{original.InNetworkPercentFee:P}</td></tr>
                <tr><td style=""width: 200px"">In Network Fee Add:</td><td>{(original.InNetworkFeeAdd ? "Yes" : "No")}</td></tr>
                <tr><td style=""width: 200px"">Out Network Flat Fee:</td><td>{original.OutNetworkFlatFee:C}</td></tr>
                <tr><td style=""width: 200px"">Out Network Percent Fee:</td><td>{original.OutNetworkPercentFee:P}</td></tr>
                <tr><td style=""width: 200px"">Out Network Fee Add:</td><td>{(original.OutNetworkFeeAdd ? "Yes" : "No")}</td></tr>
                </table>";
            }

            return $@"{_emailBodyOpeningHtml}
                <h3>Loadshop Shipper Fees have been changed for {updates.Name}</h3>
                <p>Changes made by: {changeUser}</p>
                {feeData}
            {_emailBodyClosingHtml}";
        }

        private string CreatePendingEmailMessage(LoadEntity load, UserEntity user, string userEmail, LoadContactEntity contact, LoadClaimEntity claim)
        {
            return $@"{_emailBodyOpeningHtml}
                {CreatePendingEmailBody(load, user, userEmail, contact, claim)}
                {_emailBodyClosingHtml}";
        }

        private string CreatePendingEmailBody(LoadEntity load, UserEntity user, string userEmail, LoadContactEntity contact, LoadClaimEntity claim, decimal? estimatedFuel = null)
        {
            var origin = load.LoadStops.OrderBy(x => x.StopNbr).First();
            var dest = load.LoadStops.OrderByDescending(x => x.StopNbr).First();

            var contactName = contact.Display;
            if (!string.IsNullOrWhiteSpace(contactName))
            {
                contactName = contactName.Split(' ').FirstOrDefault();
            }

            var userPhoneNumber = string.Empty;
            if (user.UserNotifications != null)
            {
                var phoneNotification = user.UserNotifications.Where(x => x.MessageTypeId == MessageTypeConstants.CellPhone).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(phoneNotification?.NotificationValue))
                {
                    userPhoneNumber = phoneNotification.NotificationValue;
                }
            }

            var originCity = GetTitleCase(origin.City);
            var destinationCity = GetTitleCase(dest.City);

            userEmail = GetLowerCase(userEmail);
            var contactEmail = GetLowerCase(contact.Email);

            var fuelRate = estimatedFuel ?? load.FuelRate;
            var totalRateTitle = "Total Rate";
            var fuelSurchargeTitle = "Fuel Surcharge";
            var estimatedRateDisclaimer = "";

            var isEstimatedFuel = estimatedFuel.HasValue;
            var fuelReratingNumberOfDays = FscUtilities.DEFAULT_FUEL_RERATING_NUMBER_OF_DAYS;
            var cust = _context.Customers.FirstOrDefault(x => x.CustomerId == load.CustomerId);
            if (cust != null && cust.UseFuelRerating)
            {
                isEstimatedFuel = FscUtilities.IsEstimatedFsc(cust, (origin.EarlyDtTm ?? origin.LateDtTm), _dateTime.Now) || estimatedFuel.HasValue;
                fuelReratingNumberOfDays = cust.FuelReratingNumberOfDays;
            }
            if (isEstimatedFuel)
            {
                totalRateTitle = "Est Total Rate*";
                fuelSurchargeTitle = "Est Fuel Surcharge*";
                estimatedRateDisclaimer = $"<p style='color: #23aae6'><strong><i>**Because pickup date is greater than {fuelReratingNumberOfDays} days out, fuel cost listed above is estimated.  The final fuel amount will be sent out 5 days prior to pickup.</i></strong></p>";
            }

            // Add service types display to email body
            var serviceTypeNames = (
                from st in _context.ServiceTypes
                join lst in _context.LoadServiceTypes on st.ServiceTypeId equals lst.ServiceTypeId
                where lst.LoadId == load.LoadId
                select st.Name
                ).ToList();
            var specialServiceTypesDisplay = "";
            if (serviceTypeNames != null && serviceTypeNames.Any())
            {
                specialServiceTypesDisplay = string.Join(", ", serviceTypeNames);
            }

            var billingLoadIdDisplay = RemoveOwnerId(load.ReferenceLoadId);
            var billingLoadIdHtml = !string.IsNullOrWhiteSpace(billingLoadIdDisplay) &&
                !billingLoadIdDisplay.Equals(load.ReferenceLoadDisplay, StringComparison.OrdinalIgnoreCase) ?
                    $"<li><strong>Billing Load ID:</strong> {billingLoadIdDisplay}</li>" :
                    "";

            //Subtract the fee if configured to do so
            var carrierLineHaul = load.LineHaulRate - (claim?.FeeAdd == false ? claim.LoadshopFee : 0);

            return $@"<p>{GetTitleCase(user.FirstName)} & {GetTitleCase(contactName)},</p>
                <p>Success! Load # {load.ReferenceLoadDisplay} was booked. Please work together to ensure the load picks up and delivers on time.</p>

                <div><strong>Carrier booking details and confirmation</strong></div>

                <table width='500px' style='width: 500px; table-layout: auto; color: #dedede; text-align: center;' cellspacing='0'>
                    <tr>
                        <td>{totalRateTitle}</td>
                        <td>Line Haul Rate</td>
                        <td>{fuelSurchargeTitle}</td>
                    </tr>
                    <tr>
                        <td>{fuelRate + carrierLineHaul:C}</td>
                        <td>{carrierLineHaul:C}</td>
                        <td>{fuelRate:C}</td>
                    </tr>
                </table>

                {estimatedRateDisclaimer}

                <ul>
                    <li><strong>Shipper's Order Number:</strong> {load.ReferenceLoadDisplay}</li>
                    {billingLoadIdHtml}
                    <li><strong>Equipment Type:</strong> {load.Equipment.EquipmentDesc}</li>
                    <li><strong>Pickup:</strong> {originCity}, {origin.State} ({origin.ApptType} {origin.LateDtTm.ToString("MM/dd/yyyy HH:mm")})</li>
                    <li><strong>Destination:</strong> {destinationCity}, {dest.State} ({dest.ApptType} {dest.LateDtTm.ToString("MM/dd/yyyy HH:mm")})</li>
                    <li><strong>Special Instructions:</strong> {load.Comments}</li>
                    <li><strong>Special Service Types:</strong> {specialServiceTypesDisplay}</li>
                </ul>

                <p>If issues occur during the trip, please connect directly with each other to work through any potential issues.</p>

                <table width='750px' style='width: 750px; table-layout: auto; color: #dedede; margin-left: 35px;' cellspacing='0'>
                    <tr>
                        <td>{GetCarrierName(user.PrimaryScac)}</td>
                        <td>{GetTitleCase(user.FirstName)} {GetTitleCase(user.LastName)}</td>
                        <td><a href='mailto:{userEmail}' style='{_anchorStyle}'>{userEmail}</a></td>
                        <td>{userPhoneNumber}</td>
                        <td>Operating Authority #: {GetCarrierOperatingAuthorityNumber(user.PrimaryScac)} </td>
                    </tr>
                    <tr>
                        <td>{GetShipperName(load.CustomerId)}</td>
                        <td>{GetTitleCase(contact.Display)}</td>
                        <td><a href='mailto:{contactEmail}' style='{_anchorStyle}'>{contactEmail}</a></td>
                        <td>{contact.Phone}</td>
                        <td>&nbsp;</td>
                    </tr>
                </table>

                <p style='color: #31c2ae;'><strong>ASSET ONLY - No Brokering or use of Partner Carriers will be allowed on this freight.  Unauthorized actions can lead to removal from platform.</strong></p>
                <p><strong>Accessorial per existing agreement with shipper of the load. If no existing agreement default to KBX accessorial agreement.</strong></p>
                <p>Lastly, please note depending on the carrier's motor carrier contract, the actual tender may be different than the load # shown in this email. It is the exact same load but different numbers. Please use the load # in the tender for billing purposes which can also be found under your My Loads tab.</p>";
        }

        /// <summary>
        /// Removes owner id from referenceLoadId
        /// </summary>
        /// <param name="referenceLoadId"></param>
        /// <returns></returns>
        public string RemoveOwnerId(string referenceLoadId)
        {
            var result = string.Empty;
            if (!string.IsNullOrWhiteSpace(referenceLoadId))
            {
                result = referenceLoadId;

                var index = referenceLoadId.IndexOf('-');
                if (index > 0)
                {
                    result = referenceLoadId.Substring(index + 1);
                }
            }

            return result;
        }

        private bool GuardNotification(NotificationEntity notification)
        {
            var userActions = _context.Users
                                        .Where(u => u.UserId == notification.NotificationData.UserId)
                                        .SelectMany(u => u.SecurityUserAccessRoles
                                                            .SelectMany(suar => suar.SecurityAccessRole.SecurityAccessRoleAppActions
                                                                                                            .Select(saraa => saraa.AppActionId))).ToList();

            var transactionType = notification.NotificationData.TransactionTypeId.ToLower();

            switch (transactionType)
            {
                case "new":
                    {
                        return userActions.Contains(SecurityActions.Loadshop_Notifications_Email_Favorites);
                    }
                case "pending":
                    {
                        return userActions.Contains(SecurityActions.Loadshop_Notification_Email_Booked_Loads);
                    }
                case "accepted":
                    {
                        return userActions.Contains(SecurityActions.Loadshop_Notification_Email_Booked_Loads);
                    }
                case "removed":
                    {
                        return userActions.Contains(SecurityActions.Loadshop_Notification_Email_Load_Cancelled);
                    }
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets a string in title case
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetTitleCase(string value)
        {
            string result = string.Empty;
            if (!string.IsNullOrWhiteSpace(value))
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                result = textInfo.ToTitleCase(value.ToLower());
            }

            return result;
        }

        /// <summary>
        /// Gets a string in lower case
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetLowerCase(string value)
        {
            string result = string.Empty;
            if (!string.IsNullOrWhiteSpace(value))
            {
                result = value.ToLower();
            }

            return result;
        }

        /// <summary>
        /// Gets shipper name for a customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public string GetShipperName(Guid customerId)
        {
            var customer = _context.Customers.SingleOrDefault(x => x.CustomerId == customerId);
            return customer?.Name ?? string.Empty;
        }

        /// <summary>
        /// Gets carrier name
        /// </summary>
        /// <param name="primaryScac"></param>
        /// <returns></returns>
        public string GetCarrierName(string primaryScac)
        {
            var carrier = _context.CarrierScacs
                .Include(x => x.Carrier)
                .SingleOrDefault(x => x.Scac == primaryScac);

            return carrier?.Carrier?.CarrierName ?? string.Empty;
        }

        private string GetCarrierOperatingAuthorityNumber(string scac)
        {
            var operatingAuthNbr = (
                from c in _context.Carriers
                join cs in _context.CarrierScacs on c.CarrierId equals cs.CarrierId
                where cs.Scac == scac
                select c.OperatingAuthNbr).SingleOrDefault();

            return operatingAuthNbr ?? string.Empty;
        }

        public NotificationMessageEntity SendFuelUpdateEmail(LoadEntity load, UserEntity user, LoadContactEntity contact, decimal oldFuelRate, LoadClaimEntity claim)
        {
            if (load != null && user != null && contact != null)
            {
                //Subtract the fee if configured to do so
                var carrierLineHaul = load.LineHaulRate - (claim?.FeeAdd == false ? claim.LoadshopFee : 0);

                var body = $@"{_emailBodyOpeningHtml}
                <h3>Finalized Tender Rate for Load # {load.ReferenceLoadDisplay}</h3>

                <table width='500px' style='width: 500px; table-layout: auto; color: #dedede; text-align: center;' cellspacing='0'>
                    <tr style='{_emailRowStyle}'>
                        <td>Final Rate</td>
                        <td>Line Haul Rate</td>
                        <td>Final Fuel Surcharge</td>
                    </tr>
                    <tr style='{_emailRowStyle}'>
                        <td>{carrierLineHaul + load.FuelRate:C}</td>
                        <td>{carrierLineHaul:C}</td>
                        <td>{load.FuelRate:C}</td>
                    </tr>
                </table>

                <br/>
                <br/>
                <h3 style='margin-bottom: 0px;'>Original Booking Confirmation</h3>
                <hr/>

                {CreatePendingEmailBody(load, user, GetTo(user, MessageTypeConstants.Email).FirstOrDefault(), contact, claim, oldFuelRate)}

                {_emailBodyClosingHtml}";

                var msg = new NotificationMessageEntity()
                {
                    Subject = "Loadshop Fuel Update",
                    Message = body,
                    To = GetTo(user, MessageTypeConstants.Email).FirstOrDefault(),
                    CC = GetPendingCC(user, contact)
                };
                _emailService.SendMailMessage(msg, false);

                return msg;
            }

            return null;
        }
    }
}
