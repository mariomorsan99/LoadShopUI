using AutoMapper;
using Loadshop.DomainServices.Common.Services;
using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Security;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Utilities;
using System.Threading.Tasks;
using Loadshop.DomainServices.Constants;
using System.Text;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class UserProfileService : IUserProfileService
    {
        private const string ErrorPrefix = "urn:UserProfile";
        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;
        private readonly ICarrierService _carrierService;
        private readonly ICommonService _commonService;
        private readonly ISecurityService _securityService;
        private readonly IUserContext _userContext;
        private readonly ISMSService _smsService;
        private readonly IAgreementDocumentService _agreementDocumentService;

        public UserProfileService(LoadshopDataContext context,
            IMapper mapper,
            ICarrierService carrierService,
            ICommonService commonService,
            ISecurityService securityService,
            IUserContext userContext,
            ISMSService smsService,
            IAgreementDocumentService agreementDocumentService)
        {
            _context = context;
            _mapper = mapper;
            _carrierService = carrierService;
            _commonService = commonService;
            _securityService = securityService;
            _userContext = userContext;
            _smsService = smsService;
            _agreementDocumentService = agreementDocumentService;
        }

        public async Task<UserProfileData> GetUserProfileAsync(Guid identUserId)
        {
            var user = await _context.Users
                .Include(u => u.PrimaryScacEntity.Carrier)
                .Include(x => x.UserNotifications)
                .ThenInclude(x => x.MessageType)
                .Include(u => u.UserShippers)
                .ThenInclude(userShipper => userShipper.Customer)
                .Include(u => u.PrimaryCustomer)
                .SingleOrDefaultAsync(x => x.IdentUserId == identUserId);

            if (user == null)
            {
                // Throw exception so we can catch it and add the user
                throw new EntityNotFoundException($"UserProfile not found for id {identUserId}");
            }

            var userProfile = _mapper.Map<UserProfileData>(user);

            // Add missing notification types the user does not have set yet
            var messageTypes = await _context.MessageTypes.Where(x => x.MessageTypeId != MessageTypeConstants.Email_SingleCarrierScac).ToListAsync();
            foreach (var messageType in messageTypes)
            {
                var userMessageyTypeExists = userProfile.UserNotifications.Any(x => x.MessageTypeId.Equals(messageType.MessageTypeId, StringComparison.OrdinalIgnoreCase));
                if (userMessageyTypeExists) continue;

                if (messageType.MessageTypeId.Equals(MessageTypeConstants.Email))
                {
                    // add a record using the user's identity email as a default
                    var entity = new UserNotificationEntity()
                    {
                        MessageTypeId = MessageTypeConstants.Email,
                        UserId = user.UserId,
                        IsDefault = true,
                        NotificationValue = _userContext.Email,
                        CreateBy = user.Username,
                        CreateDtTm = DateTime.Now,
                        LastChgBy = user.Username,
                        LastChgDtTm = DateTime.Now,
                        NotificationEnabled = true
                    };
                    _context.UserNotifications.Add(entity);
                    await _context.SaveChangesAsync();

                    userProfile.UserNotifications.Add(_mapper.Map<UserNotificationData>(entity));
                }
                else
                {
                    userProfile.UserNotifications.Add(_mapper.Map<UserNotificationData>(messageType));
                }
            }

            // check if the user has agreed to the latest terms and privacy
            userProfile.HasAgreedToTerms = await _agreementDocumentService.HasUserAgreedToLatestTermsAndPrivacy(user.UserId);

            SetSecurityProperties(user, userProfile);
            userProfile.CarrierVisibilityTypes = _commonService.GetCarrierVisibilityTypes(user.Username, user.PrimaryScacEntity?.CarrierId);

            return userProfile;
        }

        private void SetSecurityProperties(UserEntity user, UserProfileData userProfile)
        {
            //Find the scac incase it was not loaded as part of the User Entity
            var primaryScac = _context.CarrierScacs.Find(user.PrimaryScac);
            if (primaryScac != null)
            {
                userProfile.AvailableCarrierScacs = GetAuthorizedScas();
                userProfile.AuthorizedShippersForMyPrimaryScac = _securityService.GetAuthorizedCustomersByScac().ToList();
            }

            if (user.PrimaryCustomerId.HasValue)
            {
                userProfile.MyCustomerContractedScacs = _securityService
                                                                .GetCustomerContractedScacsByPrimaryCustomer()
                                                                .Select(carrierScac => carrierScac.Scac)
                                                                .ToList();
            }

            userProfile.SecurityAccessRoles = _securityService.GetUserRoles().ToList();
            userProfile.IsShipper = SecurityRoles.ShipperRoles.Any(role => userProfile.SecurityAccessRoles.Any(securityAccessRole => securityAccessRole.AccessRoleName == role)) && user.PrimaryCustomerId != null;
            userProfile.IsCarrier = SecurityRoles.CarrierRoles.Any(role => userProfile.SecurityAccessRoles.Any(securityAccessRole => securityAccessRole.AccessRoleName == role)) && user.PrimaryScac != null;
            userProfile.IsAdmin = SecurityRoles.AdminRoles.Any(role => userProfile.SecurityAccessRoles.Any(securityAccessRole => securityAccessRole.AccessRoleName == role));

            var defaultCommodityRoles = new List<string> { SecurityRoles.ShipperAdmin, SecurityRoles.ShipperUser, SecurityRoles.ShipperUserViewOnly };
            userProfile.CanSetDefaultCommodity = defaultCommodityRoles.Any(role => userProfile.SecurityAccessRoles.Any(securityAccessRole => securityAccessRole.AccessRoleName == role));

            if (userProfile.IsShipper)
            {
                userProfile.FocusEntity = _mapper.Map<UserFocusEntityData>(user.PrimaryCustomer);
            }
            else if (userProfile.IsCarrier)
            {
                userProfile.FocusEntity = _mapper.Map<UserFocusEntityData>(user.PrimaryScacEntity);
            }
        }

        private string GetScacToAutomaticallySave(List<string> availableScacs)
        {
            string scacToSave = null;
            if (availableScacs?.Count == 1)
            {
                scacToSave = availableScacs.FirstOrDefault();
            }

            return scacToSave;
        }

        public async Task<SaveUserProfileResponse> SaveUserProfileAsync(UserProfileData userProfile, string username)
        {
            var response = new SaveUserProfileResponse();

            var user = await _context.Users
                .Include(x => x.UserNotifications)
                .SingleOrDefaultAsync(x => x.UserId == userProfile.UserId);

            if (user == null)
            {
                throw new EntityNotFoundException($"UserProfile not found for id {userProfile.UserId}");
            }

            var validationErrorMessage = ValidateUserProfile(userProfile);
            if (!string.IsNullOrWhiteSpace(validationErrorMessage))
            {
                response.ModelState.AddModelError($"{ErrorPrefix}", validationErrorMessage);
                return response;
            }

            //This should be removed as we are moving away from the Scac Field
            if (user.PrimaryScac == null)
            {
                user.PrimaryScac = userProfile.PrimaryScac;
            }

            // allow the commodity to be a nullable
            user.DefaultCommodity = userProfile.DefaultCommodity;

            user.IsNotificationsEnabled = userProfile.IsNotificationsEnabled;

            // remove notification user deleted
            user.UserNotifications = user.UserNotifications.Where(x => userProfile.UserNotifications.Any(y => y.UserNotificationId == x.UserNotificationId)).ToList();

            // add / update existing notifications
            foreach (var notification in userProfile.UserNotifications)
            {
                var dbNotification = user.UserNotifications.SingleOrDefault(x => x.UserNotificationId == notification.UserNotificationId);
                if (dbNotification == null)
                {
                    dbNotification = new UserNotificationEntity()
                    {
                        MessageTypeId = notification.MessageTypeId,
                        UserId = user.UserId
                    };
                    _context.UserNotifications.Add(dbNotification);
                }
                if (notification.MessageTypeId == MessageTypeConstants.CellPhone)
                {
                    dbNotification.MessageTypeId = MessageTypeConstants.CellPhone;
                }
                else if (notification.MessageTypeId == MessageTypeConstants.Phone)
                {
                    dbNotification.MessageTypeId = MessageTypeConstants.Phone;
                }
                else if (notification.MessageTypeId == MessageTypeConstants.Email)
                {
                    dbNotification.MessageTypeId = MessageTypeConstants.Email;
                }
                dbNotification.NotificationEnabled = notification.NotificationEnabled;
                dbNotification.IsDefault = notification.IsDefault;
                dbNotification.NotificationValue = notification.NotificationValue ?? string.Empty;
            }
            await _context.SaveChangesAsync(username);

            if (user.PrimaryScac != null)
            {
                var primaryCarrierScac = _context.CarrierScacs.Find(user.PrimaryScac);
                userProfile.AvailableCarrierScacs = GetAuthorizedScas(primaryCarrierScac?.CarrierId);
            }

            SetSecurityProperties(user, userProfile);

            response.UserProfile = await GetUserProfileAsync(userProfile.IdentUserId);
            return response;
        }

        public void UpdateUserData(Guid identUserId, string username, string firstName, string lastName)
        {
            var user = _context.Users.SingleOrDefault(x => x.IdentUserId == identUserId);
            if (user != null)
            {
                user.Username = username;
                user.FirstName = firstName;
                user.LastName = lastName;

                _context.SaveChanges(username);
            }
        }

        public async Task<UserProfileData> CreateUserProfileAsync(Guid identUserId, string carrierId, string email, string username, string firstName = null, string lastName = null)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.IdentUserId == identUserId);

            if (user != null)
            {
                throw new Exception($"User already exists");
            }

            var carrierScacs = GetAuthorizedScas(); //_carrierService.GetEligibleScacs(carrierId);
            var primaryScac = GetScacToAutomaticallySave(carrierScacs);

            user = new UserEntity()
            {
                UserId = Guid.NewGuid(),
                PrimaryScac = primaryScac,
                IsNotificationsEnabled = true,
                UserNotifications = new List<UserNotificationEntity>()
                {
                    CreateUserNotification(MessageTypeConstants.Email, email),
                    CreateUserNotification(MessageTypeConstants.CellPhone, "")
                },
                IdentUserId = identUserId,
                Username = username,
                FirstName = firstName,
                LastName = lastName
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(username);
            return await GetUserProfileAsync(identUserId);
        }

        private static UserNotificationEntity CreateUserNotification(string messageType, string defaultValue = null)
        {
            return new UserNotificationEntity()
            {
                MessageTypeId = messageType,
                UserNotificationId = Guid.NewGuid(),
                NotificationValue = defaultValue
            };
        }

        public async Task<UserProfileData> UpdateFocusEntityAsync(Guid identUserId, UserFocusEntityData userFocusEntityData, string userName)
        {
            identUserId.NullArgumentCheck(nameof(identUserId));
            userFocusEntityData.NullArgumentCheck(nameof(userFocusEntityData));
            userName.NullArgumentCheck(nameof(userName));

            var userEntity = await _context.Users.SingleOrDefaultAsync(user => user.IdentUserId == identUserId);

            userEntity.NullEntityCheck(identUserId);

            userEntity.PrimaryScac = null;
            userEntity.PrimaryCustomerId = null;


            switch (userFocusEntityData.Type)
            {
                case UserFocusEntityType.CarrierScac:
                    userEntity.PrimaryScac = userFocusEntityData.Id;

                    //If admin add a record for the user so we don't have to write special code for admin security checkes thorughout the app
                    if (await _securityService.IsAdminAsync() || await _securityService.IsDedicatedAsync())
                        await UpsertCarrier(userFocusEntityData.Id, userEntity);
                    break;
                case UserFocusEntityType.Shipper:

                    Guid customerId;

                    if (Guid.TryParse(userFocusEntityData.Id, out customerId))
                    {
                        userEntity.PrimaryCustomerId = customerId;

                        //If admin add a record for the user so we don't have to write special code for admin security checkes thorughout the app
                        if (await _securityService.IsAdminAsync())
                            await UpsertCustomer(customerId, userEntity);
                    }
                    else
                        throw new Exception($"Invalid customer id: {userFocusEntityData.Id}");
                    break;
            }

            await _context.SaveChangesAsync(userName);

            return await GetUserProfileAsync(identUserId);
        }

        private async Task UpsertCustomer(Guid customerId, UserEntity userEntity)
        {
            var hasShipper = await _context.UserShippers
                                                .AnyAsync(userShipper =>
                                                            userShipper.UserId == userEntity.UserId
                                                            && userShipper.CustomerId == customerId);
            if (!hasShipper)
                _context.UserShippers.Add(new UserShipperEntity() { UserId = userEntity.UserId, CustomerId = customerId });

        }

        private async Task UpsertCarrier(string carrierScac, UserEntity userEntity)
        {
            var carrierId = await _context.Carriers
                .Where(carrier => carrier.CarrierScacs.Any(cs => cs.Scac == carrierScac))
                .Select(carrier => carrier.CarrierId)
                .SingleOrDefaultAsync();

            var hasCarrier = await _context.UserCarrierScacs
                .AnyAsync(ucs =>
                        ucs.UserId == userEntity.UserId
                        && ucs.CarrierId == carrierId
                        && (ucs.Scac == null || ucs.Scac == carrierScac));

            if (!hasCarrier)
            {
                if (carrierId != null)
                    _context.UserCarrierScacs.Add(new UserCarrierScacEntity() { UserCarrierScacId = Guid.NewGuid(), UserId = userEntity.UserId, CarrierId = carrierId, Scac = carrierScac });
                else
                    throw new Exception($"Cannot find Carrier for SCAC: {carrierScac}");
            }

        }

        public async Task<bool> IsViewOnlyUserAsync(Guid identUserId)
        {
            var user = await _context.Users
                .Include(u => u.PrimaryScacEntity)
                .SingleOrDefaultAsync(x => x.IdentUserId == identUserId);
            if (user == null)
            {
                throw new EntityNotFoundException($"UserProfile not found for id {identUserId}");
            }

            bool isActiveCarrier = this._carrierService.IsActiveCarrier(user?.PrimaryScacEntity?.CarrierId);
            bool isPlanningEligible = this._carrierService.IsPlanningEligible(user?.PrimaryScac);

            bool isViewOnlyRole = false;
            var viewOnlyRoles = new List<string> { Security.SecurityRoles.CarrierUserViewOnly, Security.SecurityRoles.ShipperUserViewOnly };

            //Roles are already sorted by their Access Level
            var highestLevelRole = (await _securityService.GetUserRolesAsync()).FirstOrDefault();
            if (highestLevelRole != null)
            {
                isViewOnlyRole = viewOnlyRoles.Contains(highestLevelRole.AccessRoleName);
            }


            return !isActiveCarrier || !isPlanningEligible || isViewOnlyRole;
        }

        public async Task<string> GetPrimaryCustomerOwner(Guid identUserId)
        {
            var ownerId = (await _context.Users
                .Include(_ => _.PrimaryCustomer)
                .FirstOrDefaultAsync(_ => _.IdentUserId == identUserId))
                ?.PrimaryCustomer.TopsOwnerId;

            return ownerId;
        }

        public async Task<Guid?> GetPrimaryCustomerId(Guid identUserId)
        {
            return (await _context.Users.FirstOrDefaultAsync(_ => _.IdentUserId == identUserId))?.PrimaryCustomerId;
        }

        public async Task<Guid?> GetPrimaryCustomerIdentUserId(Guid identUserId)
        {
            var primaryCustomer = await _context.Users
                .Include(_ => _.PrimaryCustomer)
                .Where(_ => _.IdentUserId == identUserId)
                .Select(_ => _.PrimaryCustomer)
                .FirstOrDefaultAsync();

            if (primaryCustomer != null)
            {
                return primaryCustomer.IdentUserId;
            }

            return null;
        }

        private List<string> GetAuthorizedScas()
        {
            return _securityService
                        .GetAuthorizedScasForCarrierByPrimaryScac()
                        .Select(carrierScac => carrierScac.Scac)
                        .ToList();
        }

        private List<string> GetAuthorizedScas(string carrierId)
        {
            return _securityService
                        .GetAuthorizedScacsForCarrier(carrierId)
                        .Select(carrierScac => carrierScac.Scac)
                        .ToList();
        }

        private string ValidateUserProfile(UserProfileData userProfile)
        {
            var errors = new StringBuilder();

            if (userProfile.UserNotifications == null || !userProfile.UserNotifications.Any())
            {
                errors.AppendLine("Email Address must be provided");
                errors.AppendLine("Contact Phone Number must be provided");
                return errors.ToString();
            }

            if (!userProfile.UserNotifications.Any(x => x.MessageTypeId == MessageTypeConstants.Email))
            {
                errors.AppendLine("Email Address must be provided");
            }

            if (!userProfile.UserNotifications.Any(x => x.MessageTypeId == MessageTypeConstants.CellPhone || x.MessageTypeId == MessageTypeConstants.Phone))
            {
                errors.AppendLine("Contact Phone Number must be provided");
            }

            var cellPhoneNotifications = userProfile.UserNotifications.Where(x => x.MessageTypeId == MessageTypeConstants.CellPhone);

            foreach (var item in cellPhoneNotifications)
            {
                if (!_smsService.ValidateNumber(item.NotificationValue).Result)
                {
                    errors.AppendLine($"Contact Number: {item.NotificationValue} is not a valid cell phone number to receive SMS notifications.");
                }
            }

            if (errors.Length > 0)
            {
                return errors.ToString();
            }
            return string.Empty;
        }
    }
}
