using AutoMapper;
using Loadshop.Data;
using Loadshop.DomainServices.Common.Services;
using Loadshop.DomainServices.Common.Services.QueryWrappers;
using Loadshop.DomainServices.Constants;
using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Loadshop.Services.Utility;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class ShippingService : IShippingService
    {
        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly ISecurityService _securityService;
        private readonly ICommonService _commonService;
        private readonly INotificationService _notificationService;
        private readonly IRatingService _ratingService;
        private readonly ServiceUtilities _serviceUtilities;
        private readonly IDateTimeProvider _dateTime;

        private List<string> _initialTransStates = new List<string> {
                 TransactionTypes.PendingAdd
                ,TransactionTypes.PendingUpdate
            };

        private List<string> _transToProcess = new List<string> {
                 TransactionTypes.New
                ,TransactionTypes.Updated
                ,TransactionTypes.Posted
                ,TransactionTypes.PendingFuel
                ,TransactionTypes.PendingRates
                ,TransactionTypes.PendingAdd
                ,TransactionTypes.PendingUpdate
                ,TransactionTypes.PendingRemoveScac
            };

        public ShippingService(
            LoadshopDataContext context,
            IMapper mapper,
            IUserContext userContext,
            ISecurityService securityService,
            ICommonService commonService,
            INotificationService notificationService,
            IRatingService ratingService,
            IDateTimeProvider dateTime,
            ServiceUtilities serviceUtilities)
        {
            _context = context;
            _mapper = mapper;
            _serviceUtilities = serviceUtilities;
            _userContext = userContext;
            _securityService = securityService;
            _commonService = commonService;
            _notificationService = notificationService;
            _ratingService = ratingService;
            _dateTime = dateTime;
        }

        public ShippingLoadData GetLoad(Guid loadId)
        {
            var load = _context.Loads
                .Include(_ => _.LoadStops)
                .Include(_ => _.LoadTransactions)
                .Include(_ => _.CarrierScacs)
                .Include(_ => _.Customer)
                .Include(_ => _.PostedLoadCarrierGroups)
                .Where(_ => _.LoadId == loadId)
                .SingleOrDefault();

            if (load != null && !AuthorizedForCustomer(load.CustomerId))
            {
                throw new UnauthorizedAccessException($"User is not authorized for customer: {load.CustomerId}");
            }

            return _mapper.Map<ShippingLoadData>(load);
        }

        public OrderEntryLoadDetailData GetLoadDetailById(Guid loadId)
        {
            var load = _context.Loads
                .Include(_ => _.Equipment)
                .Include(_ => _.Contacts)
                .Include(_ => _.LoadTransactions)
                .Include(_ => _.LoadServiceTypes).ThenInclude(_ => _.ServiceType)
                .Include(_ => _.LoadStops).ThenInclude(_ => _.AppointmentSchedulerConfirmationType)
                .Include(_ => _.LoadStops).ThenInclude(_ => _.Contacts)
                .Include(_ => _.LoadStops).ThenInclude(_ => _.DeliveryLineItems)
                .SingleOrDefault(_ => _.LoadId == loadId);

            if (load == null)
            {
                throw new ValidationException("Load not found");
            }
            if (load.ManuallyCreated != true)
            {
                throw new ValidationException("Load was not manually created");
            }
            if (!AuthorizedForCustomer(load.CustomerId))
            {
                throw new ValidationException($"User is not authorized for customer: {load.CustomerId}");
            }

            var result = _mapper.Map<OrderEntryLoadDetailData>(load);
            if (result.LoadStops != null)
            {
                foreach (var stop in result.LoadStops)
                {
                    if (stop != null)
                    {
                        stop.StateName = _commonService.GetUSCANStateProvince(stop.State)?.Name ?? string.Empty;
                    }
                }

                foreach (var stop in load.LoadStops)
                {
                    if (stop?.DeliveryLineItems != null)
                    {
                        result.LineItems.AddRange(_mapper.Map<List<LoadLineItemData>>(stop.DeliveryLineItems));
                    }
                }

                if (result.LineItems != null)
                {
                    foreach (var lineItem in result.LineItems)
                    {
                        lineItem.PickupStopNumber = result.LoadStops.FirstOrDefault(_ => _.LoadStopId == lineItem.PickupStopId)?.StopNbr ?? 0;
                        lineItem.DeliveryStopNumber = result.LoadStops.FirstOrDefault(_ => _.LoadStopId == lineItem.DeliveryStopId)?.StopNbr ?? 0;
                    }
                }
            }

            return result;
        }

        public OrderEntryLoadDetailData GetDefaultLoadDetail(Guid identUserId)
        {
            var load = new OrderEntryLoadDetailData()
            {
                TransportationMode = "TRUCK",
                LoadStops = new List<OrderEntryLoadStopData>()
                {
                    new OrderEntryLoadStopData()
                    {
                        StopNbr = 1,
                        StopType = "Pickup",
                        Country = "USA",
                        IsLive = true
                    },
                    new OrderEntryLoadStopData()
                    {
                        StopNbr = 2,
                        StopType = "Delivery",
                        Country = "USA",
                        IsLive = true
                    }
                },
                LineItems = new List<LoadLineItemData>()
                {
                    new LoadLineItemData()
                    {
                        PickupStopNumber = 1,
                        DeliveryStopNumber = 2
                    }
                }
            };

            var user = _context.Users.FirstOrDefault(u => u.IdentUserId == identUserId);
            if (user != null)
            {
                /** 
                 * If the user's profile has a default commodity, then set the load's default commodity to that.
                 * Otherwise, attempt to set it from the default commodity on the user's primary scac's customer
                 * record.
                 */
                if (!string.IsNullOrWhiteSpace(user.DefaultCommodity))
                {
                    load.Commodity = user.DefaultCommodity;
                }
                else
                {
                    var userPrimaryCustomerId = user.PrimaryCustomerId;
                    if (userPrimaryCustomerId.HasValue)
                    {
                        var customer = _context.Customers.SingleOrDefault(x => x.CustomerId == userPrimaryCustomerId.Value);
                        if (customer != null)
                        {
                            load.Commodity = customer.DefaultCommodity;
                        }
                    }
                }

                var userNotifications = _context.UserNotifications.Where(x => x.UserId == user.UserId).ToList();

                load.Contacts = new List<LoadContactData>()
                {
                    new LoadContactData()
                    {
                        Display = user.FirstName + " " + user.LastName,
                        Email = userNotifications.FirstOrDefault(x => x.UserId == user.UserId && x.MessageTypeId == MessageTypeConstants.Email)?.NotificationValue,
                        Phone = userNotifications.Where(x => x.UserId == user.UserId)
                                        .Where(x=> x.MessageTypeId == MessageTypeConstants.CellPhone ||
                                                    x.MessageTypeId == MessageTypeConstants.Phone)
                                        .FirstOrDefault()?.NotificationValue
                    }
                };
            }

            return load;
        }

        public List<ShippingLoadData> GetLoadsForHomeTab(Guid identUserId)
        {
            List<string> loadshopLoadTypes = new List<string>
            {
                 TransactionTypes.New
                ,TransactionTypes.Updated
            };

            var userPrimaryCustomerId = _context.Users.SingleOrDefault(u => u.IdentUserId == identUserId)?.PrimaryCustomerId;
            if (!userPrimaryCustomerId.HasValue || userPrimaryCustomerId.Equals(Guid.Empty))
            {
                throw new Exception($"Unable to determine primary customer ID for IdentUserId {identUserId}");
            }

            var eligibleTransactionTypes = new List<string>
            {
                TransactionTypes.New,
                TransactionTypes.Updated,
                TransactionTypes.PreTender,
                TransactionTypes.Pending,
                TransactionTypes.PendingAdd,
                TransactionTypes.PendingUpdate,
                TransactionTypes.PendingFuel,
                TransactionTypes.PendingRates,
                TransactionTypes.PendingRemoveScac
            };

            var loads = _context.Loads
                .Include(x => x.LoadStops)
                .Include(x => x.Customer)
                .Include(x => x.Equipment)
                .Include(x => x.LatestLoadTransaction)
                .Include(x => x.LoadServiceTypes)
                .Where(x => x.CustomerId == userPrimaryCustomerId && eligibleTransactionTypes.Contains(x.LatestTransactionTypeId))
                .ToList();

            // order by loads that are in marketplace and then the pickup date relative to today
            var orderedLoadTransactions = loads
                .OrderBy(_ => loadshopLoadTypes.Contains(_.LatestTransactionTypeId) ? 1 : 0)
                .ThenBy(l => _serviceUtilities.DistanceFromToday(l.LoadStops.Where(ls => ls.StopNbr == 1).SingleOrDefault()?.LateDtTm ?? _dateTime.Now));

            return _mapper.Map<List<ShippingLoadData>>(orderedLoadTransactions);
        }

        public List<ShippingLoadViewData> GetLoadsBySearchType_Delete(ShipperSearchTypeData searchType)
        {
            var userPrimaryCustomerId = _context.Users.SingleOrDefault(u => u.IdentUserId == _userContext.UserId)?.PrimaryCustomerId;

            if (userPrimaryCustomerId.HasValue)
            {
                return GetLoadsBySearchType(searchType, max: int.MaxValue).ToList();
            }

            return new List<ShippingLoadViewData>();
        }

        public PageableQuery<ShippingLoadViewData> GetLoadsBySearchType(ShipperSearchTypeData searchType, DateTime? visibilityPickupWindowDate = null, bool topsToGoCarrier = false, bool p44Carrier = false)
        {
            return GetLoadsBySearchType(searchType, 100, visibilityPickupWindowDate, topsToGoCarrier, p44Carrier);
        }

        protected PageableQuery<ShippingLoadViewData> GetLoadsBySearchType(ShipperSearchTypeData searchType, int max, DateTime? visibilityPickupWindowDate = null, bool topsToGoCarrier = false, bool p44Carrier = false)
        {
            var userPrimaryCustomerId = _context.Users.SingleOrDefault(u => u.IdentUserId == _userContext.UserId)?.PrimaryCustomerId;
            var onLoadShopTransactionTypes = new[] { "New", "Updated" };
            var bookedUserTransActionsTypes = new[] { "Accepted", "Pending" };
            var currentDate = _dateTime.Now;
            var isMarketplace = ShipperSearchTypeData.Posted == searchType;
            visibilityPickupWindowDate = visibilityPickupWindowDate ?? currentDate;

            if (userPrimaryCustomerId.HasValue)
            {
                //var transactionTypeIds = MapSearchTypeToTransactionList(c);
                //var shippingLoads = _context.ShippingLoadViews.FromSql($"EXECUTE spGetLoadsByShipperAndTranType @CustomerId = {userPrimaryCustomerId}, @TransactionTypes = {transactionTypeIds}");

                var transTypes = _serviceUtilities.MapShipperSearchTypeToTransactionList(searchType);

                if (isMarketplace)
                {
                    return (from l in _context.Loads
                            join c in _context.Customers on l.CustomerId equals c.CustomerId
                            join OriginLoadStop in _context.LoadStops.Where(ls => ls.StopNbr == 1) on l.LoadId equals OriginLoadStop.LoadId
                            join DestinationLoadStop in _context.LoadStops on new { l.LoadId, StopNbr = (int)l.Stops } equals new { DestinationLoadStop.LoadId, DestinationLoadStop.StopNbr }
                            where
                              transTypes.Contains(l.LatestTransactionTypeId)
                              && l.CustomerId == userPrimaryCustomerId
                            select new ShippingLoadViewData()
                            {
                                LoadId = l.LoadId,
                                CustomerId = l.CustomerId,
                                ReferenceLoadId = l.ReferenceLoadId,
                                ReferenceLoadDisplay = l.ReferenceLoadDisplay,
                                Stops = l.Stops,
                                Miles = l.Miles,
                                LineHaulRate = l.LineHaulRate,
                                SmartSpotRate = l.SmartSpotRate,
                                FuelRate = l.FuelRate,
                                Commodity = l.Commodity,
                                Weight = l.Weight,
                                IsHazMat = l.IsHazMat,
                                TransactionTypeId = l.LatestTransactionTypeId,
                                DistanceFromOrig = 0,
                                DistanceFromDest = 0,
                                Onloadshop = onLoadShopTransactionTypes.Contains(l.LatestTransactionTypeId),
                                CustomerLoadTypeId = l.CustomerLoadTypeId,

                                //Equipment
                                EquipmentId = l.EquipmentId,
                                EquipmentType = l.Equipment.EquipmentDesc,
                                EquipmentCategoryId = l.Equipment.CategoryId ?? "Unknown",
                                EquipmentCategoryDesc = l.Equipment.CategoryEquipmentDesc,
                                EquipmentTypeDisplay = l.Equipment.CategoryId == null ? l.Equipment.EquipmentDesc : l.Equipment.CategoryEquipmentDesc,

                                OriginLat = (double)OriginLoadStop.Latitude,
                                OriginLng = (double)OriginLoadStop.Longitude,
                                OriginCity = OriginLoadStop.City,
                                OriginState = OriginLoadStop.State,
                                OriginEarlyDtTm = OriginLoadStop.EarlyDtTm,
                                OriginLateDtTm = OriginLoadStop.LateDtTm,
                                DestLat = (double)DestinationLoadStop.Latitude,
                                DestLng = (double)DestinationLoadStop.Longitude,
                                DestCity = DestinationLoadStop.City,
                                DestState = DestinationLoadStop.State,
                                DestEarlyDtTm = DestinationLoadStop.EarlyDtTm,
                                DestLateDtTm = DestinationLoadStop.LateDtTm,
                                IsEstimatedFSC = FscUtilities.IsEstimatedFsc(c, (OriginLoadStop.EarlyDtTm ?? OriginLoadStop.LateDtTm), _dateTime.Now),
                                //Needed so the quick filter works
                                Scac = null,
                                // Load entire entity to prevent EF from doing a subselect
                                LoadServiceTypes = l.LoadServiceTypes
                            })
                        .OrderByDescending(l => l.OriginLateDtTm)
                        .ToPageableQuery(shippingLoadList =>
                        {
                            foreach (var l in shippingLoadList)
                            {

                                l.ShowVisibilityWarning =
                                 l.OriginLateDtTm > currentDate
                                 && l.OriginLateDtTm <= visibilityPickupWindowDate
                                 && l.DestLateDtTm >= currentDate
                                 && ((topsToGoCarrier && p44Carrier && l.VisibilityPhoneNumber == null && !l.MobileExternallyEntered && l.VisibilityTruckNumber == null)
                                 || (topsToGoCarrier && !p44Carrier && l.VisibilityPhoneNumber == null && !l.MobileExternallyEntered)
                                 || (!topsToGoCarrier && p44Carrier && l.VisibilityTruckNumber == null));
                            }
                            return Task.CompletedTask;
                        }, max);
                }
                else
                {
                    return (from l in _context.Loads
                            join c in _context.Customers on l.CustomerId equals c.CustomerId
                            join lt in _context.LoadTransactions on l.LoadId equals lt.LoadId
                            join lc in _context.LoadClaims on lt.LoadTransactionId equals lc.LoadTransactionId into lcJoin
                            from lc in lcJoin.DefaultIfEmpty()
                            join claimUser in _context.Users on lc.UserId equals claimUser.UserId into claimUserJoin
                            from claimUser in claimUserJoin.DefaultIfEmpty()
                            join claimScac in _context.CarrierScacs on lc.Scac equals claimScac.Scac into claimScacJoin
                            from claimScac in claimScacJoin.DefaultIfEmpty()
                            join claimCarrier in _context.Carriers on claimScac.CarrierId equals claimCarrier.CarrierId into claimCarrierJoin
                            from claimCarrier in claimCarrierJoin.DefaultIfEmpty()
                            join OriginLoadStop in _context.LoadStops.Where(ls => ls.StopNbr == 1) on l.LoadId equals OriginLoadStop.LoadId
                            join DestinationLoadStop in _context.LoadStops on new { l.LoadId, StopNbr = (int)l.Stops } equals new { DestinationLoadStop.LoadId, DestinationLoadStop.StopNbr }
                            where
                              transTypes.Contains(l.LatestTransactionTypeId)
                              && l.CustomerId == userPrimaryCustomerId
                              && (lc.LoadClaimId == (from lt in _context.LoadTransactions
                                                     join lc in _context.LoadClaims on lt.LoadTransactionId equals lc.LoadTransactionId
                                                     where lt.LoadId == l.LoadId
                                                     orderby lc.CreateDtTm descending
                                                     select lc).FirstOrDefault().LoadClaimId)
                            select new ShippingLoadViewData()
                            {
                                LoadId = l.LoadId,
                                CustomerId = l.CustomerId,
                                ReferenceLoadId = l.ReferenceLoadId,
                                ReferenceLoadDisplay = l.ReferenceLoadDisplay,
                                Stops = l.Stops,
                                Miles = l.Miles,
                                LineHaulRate = lc != null ? lc.LineHaulRate : l.LineHaulRate,
                                SmartSpotRate = l.SmartSpotRate,
                                FuelRate = l.FuelRate,
                                Commodity = l.Commodity,
                                Weight = l.Weight,
                                IsHazMat = l.IsHazMat,
                                TransactionTypeId = l.LatestTransactionTypeId,
                                DistanceFromOrig = 0,
                                DistanceFromDest = 0,
                                Onloadshop = onLoadShopTransactionTypes.Contains(l.LatestTransactionTypeId),
                                CustomerLoadTypeId = l.CustomerLoadTypeId,

                                //Equipment
                                EquipmentId = l.EquipmentId,
                                EquipmentType = l.Equipment.EquipmentDesc,
                                EquipmentCategoryId = l.Equipment.CategoryId ?? "Unknown",
                                EquipmentCategoryDesc = l.Equipment.CategoryEquipmentDesc,
                                EquipmentTypeDisplay = l.Equipment.CategoryId == null ? l.Equipment.EquipmentDesc : l.Equipment.CategoryEquipmentDesc,

                                OriginLat = (double)OriginLoadStop.Latitude,
                                OriginLng = (double)OriginLoadStop.Longitude,
                                OriginCity = OriginLoadStop.City,
                                OriginState = OriginLoadStop.State,
                                OriginEarlyDtTm = OriginLoadStop.EarlyDtTm,
                                OriginLateDtTm = OriginLoadStop.LateDtTm,
                                DestLat = (double)DestinationLoadStop.Latitude,
                                DestLng = (double)DestinationLoadStop.Longitude,
                                DestCity = DestinationLoadStop.City,
                                DestState = DestinationLoadStop.State,
                                DestEarlyDtTm = DestinationLoadStop.EarlyDtTm,
                                DestLateDtTm = DestinationLoadStop.LateDtTm,
                                IsEstimatedFSC = FscUtilities.IsEstimatedFsc(c, (OriginLoadStop.EarlyDtTm ?? OriginLoadStop.LateDtTm), _dateTime.Now),

                                //Load Claim
                                Scac = lc.Scac,
                                BookedUser = claimUser.Username,
                                BookedUserCarrierName = claimCarrier.CarrierName,
                                BillingLoadId = lc.BillingLoadId,
                                BillingLoadDisplay = lc.BillingLoadDisplay,
                                VisibilityPhoneNumber = lc.VisibilityPhoneNumber,
                                VisibilityTruckNumber = lc.VisibilityTruckNumber,
                                VisibilityChgDtTm = lc.VisibilityChgDtTm,
                                MobileExternallyEntered = lc.MobileExternallyEntered,
                                // Load entire entity to prevent EF from doing a subselect
                                LoadServiceTypes = l.LoadServiceTypes
                            })
                         .OrderByDescending(l => l.OriginLateDtTm)
                         .ToPageableQuery(shippingLoadList =>
                         {
                             foreach (var l in shippingLoadList)
                             {

                                 l.ShowVisibilityWarning =
                                  l.OriginLateDtTm > currentDate
                                  && l.OriginLateDtTm <= visibilityPickupWindowDate
                                  && l.DestLateDtTm >= currentDate
                                  && ((topsToGoCarrier && p44Carrier && l.VisibilityPhoneNumber == null && !l.MobileExternallyEntered && l.VisibilityTruckNumber == null)
                                  || (topsToGoCarrier && !p44Carrier && l.VisibilityPhoneNumber == null && !l.MobileExternallyEntered)
                                  || (!topsToGoCarrier && p44Carrier && l.VisibilityTruckNumber == null));
                             }
                             return Task.CompletedTask;
                         }, max);
                }
            }

            return PageableQuery<ShippingLoadViewData>.Empty();
        }

        public async Task<ShippingLoadData> RemoveLoad(Guid loadId, string username)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_Shopit_Load_Manual_Remove);

            var updateLoad = await _context.Loads
                .Include(x => x.LoadTransactions)
                .Include(x => x.CarrierScacs)
                .Include(x => x.PostedLoadCarrierGroups)
                .SingleOrDefaultAsync(x => x.LoadId == loadId);

            if (updateLoad == null)
            {
                throw new Exception("Load not found");
            }

            if (!AuthorizedForCustomer(updateLoad.CustomerId))
            {
                throw new UnauthorizedAccessException($"User is not authorized for customer: {updateLoad.CustomerId}");
            }

            if (updateLoad.LoadTransactions == null || updateLoad.LoadTransactions.Count == 0)
            {
                throw new Exception($"Load may not be removed - Load {loadId} has no transactions");
            }

            var loadTrans = updateLoad.LoadTransactions
                 .OrderByDescending(x => x.CreateDtTm)
                 .ToList();

            if (!_transToProcess.Contains(loadTrans[0].TransactionTypeId))
            {
                throw new Exception("Load may not be removed - not an active Load");
            }

            string initialState = GetLoadInitialState(loadTrans);

            if (string.IsNullOrWhiteSpace(initialState) || !_initialTransStates.Contains(loadTrans[0].TransactionTypeId))
            {
                var transaction = new LoadTransactionEntity()
                {
                    LoadId = updateLoad.LoadId,
                    TransactionTypeId = string.IsNullOrWhiteSpace(initialState) ? TransactionTypes.PendingAdd : initialState,
                    CreateDtTm = _dateTime.Now  // to support unit testing
                };

                updateLoad.LoadTransactions.Add(transaction);

                /**
                 * https://kbxltrans.visualstudio.com/Suite/_workitems/edit/38578
                 * Delete all LoadCarrierScacs associated with this Load
                 */
                _context.LoadCarrierScacs.RemoveRange(updateLoad.CarrierScacs);
                _context.PostedLoadCarrierGroups.RemoveRange(updateLoad.PostedLoadCarrierGroups);

                _context.Loads.Update(updateLoad);
                await _context.SaveChangesAsync(username);
            }

            return _mapper.Map<ShippingLoadData>(updateLoad);
        }

        /// <summary>
        /// Removes the load from loadshop, if removed from booked tab, a rating question is prompted and the answer will be recorded
        /// </summary>
        /// <param name="loadId"></param>
        /// <param name="username"></param>
        /// <param name="ratingQuestionAnswer"></param>
        /// <returns></returns>
        public async Task<ShippingLoadData> DeleteLoad(Guid loadId, string username, RatingQuestionAnswerData ratingQuestionAnswer = null)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_Shopit_Load_Manual_Delete);

            var updateLoad = _context.Loads
                .Include(x => x.LoadTransactions)
                .Include(x => x.CarrierScacs)
                .Include(x => x.PostedLoadCarrierGroups)
                .SingleOrDefault(x => x.LoadId == loadId);

            if (updateLoad == null)
            {
                throw new Exception("Load not found");
            }

            if (!AuthorizedForCustomer(updateLoad.CustomerId))
            {
                throw new UnauthorizedAccessException($"User is not authorized for customer: {updateLoad.CustomerId}");
            }

            AddLoadTransaction(updateLoad, TransactionTypes.PendingRemove);
            /**
             * https://kbxltrans.visualstudio.com/Suite/_workitems/edit/38578
             * Delete all LoadCarrierScacs associated with this Load
             */
            _context.LoadCarrierScacs.RemoveRange(updateLoad.CarrierScacs);
            _context.PostedLoadCarrierGroups.RemoveRange(updateLoad.PostedLoadCarrierGroups);

            if (ratingQuestionAnswer != null)
            {
                // the load was removed from a booked tab
                var loadClaim = GetLatestLoadClaim(updateLoad.LoadTransactions);

                ratingQuestionAnswer.LoadClaimId = loadClaim.LoadClaimId;
                ratingQuestionAnswer.LoadId = loadId;
                await _ratingService.AddRatingQuestionAnswer(ratingQuestionAnswer);
            }

            await _context.SaveChangesAsync(username);
            return _mapper.Map<ShippingLoadData>(updateLoad);
        }

        public List<LoadAuditLogData> GetLoadAuditLogs(Guid loadId)
        {

            var userPrimaryCustomerId = _context.Users.SingleOrDefault(u => u.IdentUserId == _userContext.UserId)?.PrimaryCustomerId;

            //verify the user has rights to the load 
            var load = _context.Loads.SingleOrDefault(x => x.LoadId == loadId && x.Customer.CustomerId == userPrimaryCustomerId);

            if (load == null)
            {
                throw new Exception("Load not found");
            }

            if (!AuthorizedForCustomer(load.CustomerId))
            {
                throw new UnauthorizedAccessException($"User is not authorized for customer: {load.CustomerId}");
            }

            var entities = _context.LoadAuditLogs
                .Where(_ => _.LoadId == loadId)
                .OrderByDescending(_ => _.CreateDtTm)
                .ToList();

            return _mapper.Map<List<LoadAuditLogData>>(entities);
        }

        public List<LoadCarrierScacData> GetLoadCarrierScacs(Guid loadId)
        {
            var userPrimaryCustomerId = _context.Users.SingleOrDefault(u => u.IdentUserId == _userContext.UserId)?.PrimaryCustomerId;

            //verify the user has rights to the load 
            var load = _context.Loads.SingleOrDefault(x => x.LoadId == loadId && x.Customer.CustomerId == userPrimaryCustomerId);

            if (load == null)
            {
                throw new Exception("Load not found");
            }

            if (!AuthorizedForCustomer(load.CustomerId))
            {
                throw new UnauthorizedAccessException($"User is not authorized for customer: {load.CustomerId}");
            }

            /*
             * For dedicated scacs, we do not want to show their contract rate to the shipper.
             * Instead, we want to show them as if they're contracted at the exact Line Haul Rate
             * of the load because for Dedicated Scac's KBXL will cover any difference between
             * the Line Haul Rate and the Dedicated Scac's actual contracted rate.
             * 
             * As such, we overwrite a dedicated scac's Contract Rate with the load's Line Haul Rate
             */
            var entities = (
                from lcs in _context.LoadCarrierScacs
                join cs in _context.CarrierScacs on lcs.Scac equals cs.Scac
                where lcs.LoadId == loadId
                select new LoadCarrierScacEntity
                {
                    Scac = lcs.Scac,
                    ContractRate = cs.IsDedicated ? load.LineHaulRate : lcs.ContractRate
                }).ToList();

            return _mapper.Map<List<LoadCarrierScacData>>(entities);
        }

        public List<LoadCarrierScacRestrictionData> GetLoadCarrierScacRestrictions(Guid loadId)
        {
            var userPrimaryCustomerId = _context.Users.SingleOrDefault(u => u.IdentUserId == _userContext.UserId)?.PrimaryCustomerId;

            //verify the user has rights to the load 
            var load = _context.Loads.SingleOrDefault(x => x.LoadId == loadId && x.Customer.CustomerId == userPrimaryCustomerId);

            if (load == null)
            {
                throw new Exception("Load not found");
            }

            if (!AuthorizedForCustomer(load.CustomerId))
            {
                throw new UnauthorizedAccessException($"User is not authorized for customer: {load.CustomerId}");
            }

            var entities = _context.LoadCarrierScacRestrictions
                .Where(_ => _.LoadId == loadId)
                .ToList();

            return _mapper.Map<List<LoadCarrierScacRestrictionData>>(entities);
        }

        public async Task<ShippingLoadData> RemoveCarrierFromLoad(Guid loadId, string username, RatingQuestionAnswerData ratingQuestionAnswer)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_Shop_It_Load_Edit);

            var updateLoad = await _context.Loads
                                        .Include(x => x.LoadTransactions)
                                        .Include(x => x.Contacts)
                                        .Include(x => x.LoadStops)
                                        .Include(x => x.Equipment)
                                        .SingleOrDefaultAsync(x => x.LoadId == loadId);

            if (updateLoad == null)
            {
                throw new Exception("Load not found");
            }

            if (!AuthorizedForCustomer(updateLoad.CustomerId))
            {
                throw new UnauthorizedAccessException($"User is not authorized for customer: {updateLoad.CustomerId}");
            }

            if (updateLoad.LoadTransactions == null || updateLoad.LoadTransactions.Count == 0)
            {
                throw new Exception($"Load Carrier may not be removed - Load {loadId} has no transactions");
            }

            var loadTransactions = updateLoad.LoadTransactions
                                                .OrderByDescending(loadTransaction => loadTransaction.CreateDtTm)
                                                .ToList();

            var bookedTransactionTypes = _serviceUtilities.MapShipperSearchTypeToTransactionList(ShipperSearchTypeData.Booked);

            if (!bookedTransactionTypes.Contains(updateLoad.LatestTransactionTypeId))
            {
                throw new Exception("Load Carrier may not be removed - not an active Load");
            }

            AddLoadTransaction(updateLoad, TransactionTypes.PendingRemoveScac);

            var originalLoadHistory = _mapper.Map<LoadHistoryEntity>(updateLoad);
            updateLoad.IsAccepted = false;

            var newLoadHistory = _mapper.Map<LoadHistoryEntity>(updateLoad);
            if (_serviceUtilities.HasLoadChanged(originalLoadHistory, newLoadHistory))
                _context.LoadHistories.Add(newLoadHistory);

            var loadClaim = GetLatestLoadClaim(loadTransactions);

            ratingQuestionAnswer.LoadClaimId = loadClaim.LoadClaimId;
            ratingQuestionAnswer.LoadId = loadId;
            await _ratingService.AddRatingQuestionAnswer(ratingQuestionAnswer);

            await _context.SaveChangesAsync(username);

            var ratingQuestion = await _ratingService.GetRatingQuestion(ratingQuestionAnswer.RatingQuestionId);

            //Todo validate update load contacts logic
            _notificationService.SendCarrierRemovedEmail(updateLoad, loadClaim?.User, updateLoad.Contacts, this._userContext.Email, ratingQuestion.DisplayText);

            return _mapper.Map<ShippingLoadData>(updateLoad);
        }

        private void AddLoadTransaction(LoadEntity updateLoad, string transactionType)
        {
            var transaction = new LoadTransactionEntity()
            {
                LoadId = updateLoad.LoadId,
                TransactionTypeId = transactionType,
                CreateDtTm = _dateTime.Now  // to support unit testing
            };

            updateLoad.LoadTransactions.Add(transaction);
            _context.Loads.Update(updateLoad);
        }

        private string GetLoadInitialState(List<LoadTransactionEntity> loadTrans)
        {
            return loadTrans.Where(x => _initialTransStates.Contains(x.TransactionTypeId)).OrderByDescending(x => x.CreateDtTm).Select(x => x.TransactionTypeId).FirstOrDefault();
        }

        private LoadClaimEntity GetLatestLoadClaim(List<LoadTransactionEntity> loadTrans)
        {
            var loadTransIds = loadTrans.Select(lt => lt.LoadTransactionId).ToArray();

            var latestClaim = _context.LoadClaims
                                        .Where(loadClaim => loadTransIds.Contains(loadClaim.LoadTransactionId))
                                        .OrderByDescending(lc => lc.CreateDtTm)
                                        .Include(lc => lc.User)
                                        .FirstOrDefault();

            return latestClaim;
        }

        private string MapSearchTypeToTransactionList(ShipperSearchTypeData searchType)
        {
            var transactionTypeIds = _serviceUtilities.MapShipperSearchTypeToTransactionList(searchType);

            return string.Join(",", transactionTypeIds);
        }

        public async Task<PostLoadsResponse> PostLoadsAsync(PostLoadsRequest request)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_Shopit_Load_Post);
            var response = new PostLoadsResponse();
            var userPrimaryCustomerId = _context.Users.SingleOrDefault(u => u.IdentUserId == _userContext.UserId)?.PrimaryCustomerId;

            // Throw exceptions for non-user errors that the user cannot solve
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!userPrimaryCustomerId.HasValue || userPrimaryCustomerId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userPrimaryCustomerId));
            }

            if (!AuthorizedForCustomer(userPrimaryCustomerId.Value))
            {
                throw new UnauthorizedAccessException($"User is not authorized for customer: {userPrimaryCustomerId}");
            }

            if (string.IsNullOrWhiteSpace(request.CurrentUsername))
            {
                throw new ArgumentNullException(nameof(request.CurrentUsername));
            }

            if (request.Loads == null || request.Loads.Count <= 0)
            {
                response.ModelState.AddModelError($"urn:root", "PostLoads operation requires loads to be posted");
                return response;
            }

            var loadIds = request.Loads.Select(x => x.LoadId).ToList();
            var dbLoads = await _context.Loads
                .Include(x => x.LoadTransactions)
                .Include(x => x.LoadStops)
                .Include(x => x.CarrierScacs)
                .Include(x => x.Customer)
                .Include(x => x.Equipment)
                .Include(x => x.PostedLoadCarrierGroups)
                .Include(x => x.LoadServiceTypes)
                .Where(x => loadIds.Contains(x.LoadId))
                .ToListAsync();

            var serviceTypes = await _context.ServiceTypes.AsNoTracking().ToListAsync();

            //Verify submitting loads for only one customer at a time
            var groupedLoads = dbLoads.GroupBy(l => l.CustomerId);

            if (groupedLoads.Count() > 1)
                throw new Exception("Cannot post loads from multiple shippers");

            var contractedScacs = _securityService.GetCustomerContractedScacsByPrimaryCustomer();
            var bookedTransTypes = new[] { TransactionTypes.Accepted, TransactionTypes.Pending, TransactionTypes.SentToShipperTender, TransactionTypes.PreTender };

            foreach (var load in request.Loads)
            {
                var dbLoad = dbLoads.FirstOrDefault(x => x.LoadId == load.LoadId);

                response = ValidatePostedLoad(response, dbLoad, load, bookedTransTypes, serviceTypes);

                var originalLoadHistory = _mapper.Map<LoadHistoryEntity>(dbLoad);

                var eligibleScacs = new List<string>();
                if (load.CarrierIds.Count > 0)
                {
                    var carrierScacGroups = contractedScacs
                        .Where(x => load.CarrierIds.Contains(x.CarrierId))
                        .Where(Utilities.QueryFilters.GetActiveCarrierScacDataFilter(request.RequestTime.Date))
                        .GroupBy(x => x.CarrierId)
                        .ToList();

                    var dbCarriers = carrierScacGroups.Select(x => x.Key).ToList();
                    var missingCarriers = load.CarrierIds.Except(dbCarriers);

                    if (missingCarriers.Any())
                    {
                        response.ModelState.AddModelError($"urn:load:{load.LoadId}", $"The following carriers have no eligible SCACs defined: {string.Join(", ", missingCarriers)}");
                    }

                    eligibleScacs = carrierScacGroups.SelectMany(x => x.Select(y => y.Scac)).ToList();

                }
                else
                {
                    // When no Carriers are provided, we assume that we're posting to ALL eligible carriers for the customer
                    // Right now, TOPS is the only customer, so all Carriers and Scacs are eligible for TOPS, but in the 
                    // future we'll require some lookup here to determine which Carriers are associated with the customer
                    eligibleScacs = contractedScacs
                        .Where(x =>
                            x.EffectiveDate <= request.RequestTime
                            && (x.ExpirationDate == null || x.ExpirationDate > request.RequestTime))
                        .Select(x => x.Scac)
                        .ToList();

                    if (!eligibleScacs.Any())
                    {
                        response.ModelState.AddModelError($"urn:load:{load.LoadId}", $"No eligible SCACs found");
                    }
                }

                /**
                 * The model state applies to the entire request, so we have to check that errors related to
                 * this load are helping to cause the invalid model state before we skip processing this load.
                 * If this load has no errors, then continue processing it successfully.
                 */
                if (!response.ModelState.IsValid && response.ModelState.Keys.Any(x => x.Contains(load.LoadId.ToString())))
                {
                    continue;
                }

                var postedCarrierGroupEntities = load.CarrierGroupIds.Select(groupId =>
                    new PostedLoadCarrierGroupEntity()
                    {
                        LoadId = dbLoad.LoadId,
                        LoadCarrierGroupId = groupId,
                        PostedLoadCarrierGroupId = Guid.NewGuid()
                    });

                dbLoad.PostedLoadCarrierGroups.Clear();
                dbLoad.PostedLoadCarrierGroups.AddRange(postedCarrierGroupEntities);

                dbLoad.Comments = load.Comments;
                dbLoad.Commodity = load.Commodity;
                dbLoad.LineHaulRate = load.LineHaulRate;
                dbLoad.FuelRate = load.ShippersFSC;
                dbLoad.SmartSpotRate = load.SmartSpotRate ?? dbLoad.SmartSpotRate;
                dbLoad.DATGuardRate = load.DATGuardRate ?? dbLoad.DATGuardRate;
                dbLoad.MachineLearningRate = load.MachineLearningRate ?? dbLoad.MachineLearningRate;
                dbLoad.AllCarriersPosted = load.AllCarriersPosted;

                // remove any existing
                dbLoad.LoadServiceTypes?.Clear();

                if (load.ServiceTypeIds != null && load.ServiceTypeIds.Any())
                {
                    if (dbLoad.LoadServiceTypes == null)
                    {
                        dbLoad.LoadServiceTypes = new List<LoadServiceTypeEntity>();
                    }

                    var loadServiceTypes = load.ServiceTypeIds.Select(x => new LoadServiceTypeEntity()
                    {
                        LoadId = load.LoadId,
                        ServiceTypeId = x
                    }).ToList();
                    dbLoad.LoadServiceTypes.AddRange(loadServiceTypes);
                }

                // Delete all existing LoadCarrierScacs for this load, before inserting new ones
                var existingCarrierScacs = _context.LoadCarrierScacs.Where(x => x.LoadId == dbLoad.LoadId).ToList();
                if (existingCarrierScacs.Count > 0)
                {
                    _context.LoadCarrierScacs.RemoveRange(existingCarrierScacs);
                }

                var carrierScacs = new List<LoadCarrierScacEntity>();
                foreach (var scac in eligibleScacs)
                {
                    var carrierScac = new LoadCarrierScacEntity
                    {
                        LoadId = dbLoad.LoadId,
                        Scac = scac,
                        CreateBy = request.CurrentUsername,
                        CreateDtTm = request.RequestTime,
                        LastChgBy = request.CurrentUsername,
                        LastChgDtTm = request.RequestTime
                    };
                    carrierScacs.Add(carrierScac);
                }
                _context.LoadCarrierScacs.AddRange(carrierScacs);
                response.LoadCarrierScacs.AddRange(carrierScacs);

                var postedTx = new LoadTransactionEntity
                {
                    LoadId = dbLoad.LoadId,
                    TransactionTypeId = TransactionTypes.Posted,
                    CreateBy = request.CurrentUsername,
                    CreateDtTm = request.RequestTime,
                    LastChgBy = request.CurrentUsername,
                    LastChgDtTm = request.RequestTime
                };
                _context.LoadTransactions.Add(postedTx);

                var newLoadHistory = _mapper.Map<LoadHistoryEntity>(dbLoad);
                if (_serviceUtilities.HasLoadChanged(originalLoadHistory, newLoadHistory))
                    _context.LoadHistories.Add(newLoadHistory);

                await _context.SaveChangesAsync(request.CurrentUsername);

                response.PostedLoads.Add(_mapper.Map<ShippingLoadData>(dbLoad));
            }

            return response;
        }

        /// <summary>
        /// Checks if a customer has a fuel update API defined
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public bool HasFuelUpdateApi(Guid customerId)
        {
            return _context.CustomerApis.Any(x => x.CustomerId == customerId && x.CustomerApiTypeId == "FuelUpdate");
        }

        private bool AuthorizedForCustomer(Guid customerId)
        {
            return _securityService.IsAuthorizedForCustomer(customerId);
        }

        private PostLoadsResponse ValidatePostedLoad(PostLoadsResponse response,
            LoadEntity dbLoad,
            PostingLoad load,
            string[] bookedTransTypes,
            List<ServiceTypeEntity> serviceTypes)
        {
            if (dbLoad == null)
            {
                response.ModelState.AddModelError($"urn:load:{load.LoadId}", "Load not found");
            }
            else
            {
                if (load.ShippersFSC < 0)
                {
                    response.ModelState.AddModelError($"urn:load:{load.LoadId}:{nameof(load.ShippersFSC)}", $"Shippers FSC value must be positive");
                }
                else if (load.ShippersFSC == 0 && !dbLoad.Customer.AllowZeroFuel && !HasFuelUpdateApi(dbLoad.CustomerId))
                {
                    response.ModelState.AddModelError($"urn:load:{load.LoadId}:{nameof(load.ShippersFSC)}", $"Shippers FSC value must be provided");
                }
                if (dbLoad.FuelRate != load.ShippersFSC && !dbLoad.Customer.AllowEditingFuel)
                {
                    response.ModelState.AddModelError($"urn:load:{load.LoadId}:{nameof(load.ShippersFSC)}", $"User is not allowed to edit Shippers FSC");
                }
                var loadAlreadyBooked = bookedTransTypes.Contains(dbLoad.LatestTransactionTypeId);
                if (loadAlreadyBooked)
                {
                    response.ModelState.AddModelError($"urn:load:{load.LoadId}", $"Load cannot be posted because it has already been booked. Current status: {dbLoad.LatestTransactionTypeId}");
                }
            }

            if (load.LineHaulRate <= 0)
            {
                response.ModelState.AddModelError($"urn:load:{load.LoadId}:{nameof(load.LineHaulRate)}", $"Line Haul rate value must be provided");
            }

            if (string.IsNullOrWhiteSpace(load.Commodity))
            {
                response.ModelState.AddModelError($"urn:load:{load.LoadId}:{nameof(load.Commodity)}", $"Commodity is required");
            }

            if (load.ServiceTypeIds != null && load.ServiceTypeIds.Count > 0 && !serviceTypes.Any(x => load.ServiceTypeIds.Contains(x.ServiceTypeId)))
            {
                response.ModelState.AddModelError($"urn:load:{load.LoadId}:{nameof(load.ServiceTypeIds)}", $"Invalid service type, not found.");
            }

            return response;
        }
    }
}
