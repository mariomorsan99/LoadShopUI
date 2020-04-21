using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Utility;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Exceptions;
using Microsoft.EntityFrameworkCore;
using Loadshop.DomainServices.Common.Services;
using Microsoft.Extensions.Configuration;
using Loadshop.DomainServices.Validation.Services;
using Loadshop.Data;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Dto;
using Loadshop.DomainServices.Loadshop.Services.Enum;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Common.Services.QueryWrappers;
using Microsoft.Extensions.Logging;
using Loadshop.DomainServices.Utilities;
using Loadshop.DomainServices.Constants;
using Loadshop.DomainServices.Loadshop.Services.Repositories;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class LoadService : ILoadService
    {
        private readonly ILogger<LoadService> logger;
        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IUserContext _userContext;
        private readonly ServiceUtilities _serviceUtilities;
        private readonly ICommonService _commonService;
        private readonly IConfigurationRoot _configuration;
        private readonly ILoadValidationService _loadValidationService;
        private readonly string[] bookedTransactionTypes = new[] { TransactionTypes.Accepted, TransactionTypes.PreTender, TransactionTypes.Pending, TransactionTypes.SentToShipperTender };

        private readonly ISecurityService _securityService;
        private readonly ISpecialInstructionsService _specialInstructionsService;
        private readonly IMileageService _mileageService;
        private readonly ISmartSpotPriceService _smartSpotPriceService;
        private readonly ILoadshopDocumentService _loadshopDocumentService;
        private readonly IDateTimeProvider _dateTime;
        private readonly ILoadshopFeeService _loadshopFeeService;
        private readonly ILoadQueryRepository _loadQueryReposiory;

        public LoadService(ILogger<LoadService> logger,
            LoadshopDataContext context,
            IMapper mapper,
            INotificationService notificationService,
            IUserContext userContext,
            ICommonService commonService,
            IConfigurationRoot configuration,
            ILoadValidationService loadValidationService,
            ISecurityService securityService,
            ISpecialInstructionsService specialInstructionsService,
            IMileageService mileageService,
            ISmartSpotPriceService smartSpotPriceService,
            ILoadshopDocumentService loadshopDocumentService,
            IDateTimeProvider dateTime,
            ServiceUtilities serviceUtilities,
            ILoadshopFeeService loadshopFeeService,
            ILoadQueryRepository loadQueryReposiory)
        {
            this.logger = logger;
            _context = context;
            _mapper = mapper;
            _notificationService = notificationService;
            _userContext = userContext;
            _serviceUtilities = serviceUtilities;
            _commonService = commonService;
            _configuration = configuration;
            _loadValidationService = loadValidationService;
            _securityService = securityService;
            _specialInstructionsService = specialInstructionsService;
            _mileageService = mileageService;
            _smartSpotPriceService = smartSpotPriceService;
            _loadshopDocumentService = loadshopDocumentService;
            _dateTime = dateTime;
            _loadshopFeeService = loadshopFeeService;
            _loadQueryReposiory = loadQueryReposiory;
        }

        #region Client UI Methods

        /// <summary>
        /// Insert a new LoadAuditLog record that takes a snapshot of the given load ID and
        /// associates it with the current user's action of viewing the load detail from the
        /// marketplace tab (meaning this load has not yet been booked.)
        /// </summary>
        /// <param name="id">Load ID</param>
        /// <returns>Number of records inserted</returns>
        public async Task<int> CreateLoadAuditLogEntryAsync(Guid id, AuditTypeData auditType)
        {
            return await _context.CreateLoadAuditLogEntryAsync(id, auditType, _userContext.UserId.Value, _userContext.UserName, _userContext.FirstName, _userContext.LastName);

        }

        public PageableQuery<LoadViewData> GetAllOpenLoads(Guid userIdentId)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_Marketplace_Loads_View);
            var openLoadTransTypes = new[] { TransactionTypes.New, TransactionTypes.Updated };

            return GetLoadsByTransactionTypes(openLoadTransTypes, userIdentId, isMarketplace: true);
        }

        public PageableQuery<LoadViewData> GetBookedLoads(Guid userIdentId)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_My_Loads_View_Booked);

            DateTime? pickupWindowDate = null;
            bool P44Carrier = false;
            bool TopsToGoCarrier = false;

            SetVisibilityParams(userIdentId, ref pickupWindowDate, ref P44Carrier, ref TopsToGoCarrier);

            return GetLoadsByTransactionTypes(bookedTransactionTypes, userIdentId, pickupWindowDate, TopsToGoCarrier, P44Carrier);
        }

        public PageableQuery<LoadViewData> GetDeliveredLoads(Guid userIdentId)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_Carrier_My_Loads_View_Delivered);

            DateTime? pickupWindowDate = null;
            bool P44Carrier = false;
            bool TopsToGoCarrier = false;
            var transTypes = new[] { TransactionTypes.Delivered };

            SetVisibilityParams(userIdentId, ref pickupWindowDate, ref P44Carrier, ref TopsToGoCarrier);

            return GetLoadsByTransactionTypes(transTypes, userIdentId, pickupWindowDate, TopsToGoCarrier, P44Carrier);
        }

        public async Task<LoadData> GetLoadByIdAsync(Guid id, Guid userId)
        {
            _securityService.GuardAction(
                SecurityActions.Loadshop_Ui_My_Loads_View_Booked_Detail,
                SecurityActions.Loadshop_Ui_Marketplace_Loads_View_Detail,
                SecurityActions.Loadshop_Ui_Shopit_Load_View_Booked_Detail,
                SecurityActions.Loadshop_Ui_Shopit_Load_View_Posted_Detail,
                SecurityActions.Loadshop_Ui_Shopit_Load_View_Delivered_Detail,
                SecurityActions.Loadshop_Ui_Carrier_My_Loads_View_Delivered_Detail);

            //Changing back to First or default b/c you could get back multiple result per load
            //TODO evaluate creation of "GetLatesLoadDetail" query in query repo
            var loadDetailOpts = new GetLoadDetailOptions()
            {
                LoadId = id,
                IncludeContacts = true,
                IncludeStops = true,
                IncludeEquipment = true,
                IncludeDocuments = true,
                IncludeCurrentStatuses = true,
                IncludeServiceTypes = true
            };
            var load = _loadQueryReposiory.GetLoadDetailViews(loadDetailOpts).FirstOrDefault();

            if (load == null) throw new Exception($"Load {id} not found");

            var isBooked = bookedTransactionTypes.Contains(load.TransactionTypeId);
            if (isBooked)
            {
                _securityService.GuardAction(SecurityActions.Loadshop_Ui_My_Loads_View_Booked_Detail, SecurityActions.Loadshop_Ui_Shopit_Load_View_Booked_Detail);
            }
            else if (load.TransactionTypeId == TransactionTypes.Delivered)
            {
                _securityService.GuardAction(SecurityActions.Loadshop_Ui_Carrier_My_Loads_View_Delivered_Detail, SecurityActions.Loadshop_Ui_Shopit_Load_View_Delivered_Detail);
            }
            else
            {
                _securityService.GuardAction(SecurityActions.Loadshop_Ui_Marketplace_Loads_View_Detail, SecurityActions.Loadshop_Ui_Shopit_Load_View_Posted_Detail);
            }

            AssertUserHasAccessToLoad(id);

            var result = _mapper.Map<LoadData>(load);

            if (result != null)
            {
                result.LineHaulRate = _serviceUtilities.GetContractRate(_context, id, userId) ?? result.LineHaulRate;

                //only show bookedUser if logged in user is the same carrier as booking carrier 
                var user = await _context.Users.Include(u => u.PrimaryScacEntity).FirstOrDefaultAsync(x => x.IdentUserId == userId);

                if (result.BookedUser == null && result.LoadTransaction?.TransactionType == TransactionTypeData.Delivered)
                {
                    // load claim records do not exist on Delivered loads, so go fetch the last load claim record
                    var loadTransaction = await _context.LoadTransactions.Include(x => x.Claim)
                                                    .Where(x => x.LoadId == id)
                                                    .Where(x => x.Claim != null)
                                                    .OrderByDescending(x => x.ProcessedDtTm)
                                                    .FirstOrDefaultAsync();
                    result.BookedUser = new UserContactData()
                    {
                        UserId = loadTransaction.Claim.UserId
                    };
                }

                if (result.BookedUser != null && result.BookedUser.UserId != Guid.Empty)
                {
                    // populate the user contact details
                    var bookedUser = await _context.Users.Include(x => x.UserNotifications)
                                    .FirstOrDefaultAsync(x => x.UserId == result.BookedUser.UserId);

                    result.BookedUser.FirstName = bookedUser.FirstName;
                    result.BookedUser.Email = bookedUser.UserNotifications.FirstOrDefault(x => x.MessageTypeId == MessageTypeConstants.Email)?.NotificationValue;
                    result.BookedUser.PhoneNumbers = bookedUser.UserNotifications.Where(x => x.MessageTypeId == MessageTypeConstants.Phone)
                                                        .Select(x => x.NotificationValue)
                                                        .ToList();
                    result.BookedUser.CellPhoneNumbers = bookedUser.UserNotifications.Where(x => x.MessageTypeId == MessageTypeConstants.CellPhone)
                                                        .Select(x => x.NotificationValue)
                                                        .ToList();
                }

                if (!string.IsNullOrEmpty(user?.PrimaryScac))
                {
                    //carrier user, apply the loadshop fee
                    if (isBooked || load.TransactionTypeId == TransactionTypes.Delivered)
                    {
                        _loadshopFeeService.ReapplyLoadshopFeeToLineHaul(result);
                    }
                    else
                    {
                        var customer = await _context.Customers
                            .Include(_ => _.CustomerCarrierScacContracts)
                            .FirstOrDefaultAsync(_ => _.CustomerId == load.CustomerId);
                        _loadshopFeeService.ApplyLoadshopFee(user.PrimaryScac, result, new List<CustomerEntity> { customer });
                    }
                }
            }

            // update document types
            if (load.LoadDocuments != null && load.LoadDocuments.Any())
            {
                var docTypes = _loadshopDocumentService.GetDocumentTypes();
                result.LoadDocuments.ForEach(x => x.LoadDocumentType = docTypes.FirstOrDefault(y => y.Id == x.LoadDocumentType.Id));
            }

            var latestCurrentStatus = load.LoadCurrentStatuses?.OrderByDescending(_ => _.StatusLastChgDtTm).FirstOrDefault();
            if (load.TransactionTypeId == TransactionTypes.Delivered &&
                string.Compare(latestCurrentStatus?.DescriptionShort, "DELIVERED", true) == 0)
            {
                result.DeliveredDate = latestCurrentStatus.StatusLastChgDtTm.Date;
            }

            return result;
        }

        public VisibilityBadge GetNumLoadsRequiringVisibilityInfo(Guid userIdentId)
        {
            DateTime? pickupWindowDate = null;
            bool P44Carrier = false;
            bool TopsToGoCarrier = false;

            SetVisibilityParams(userIdentId, ref pickupWindowDate, ref P44Carrier, ref TopsToGoCarrier);

            if (!P44Carrier && !TopsToGoCarrier)
            {
                // The user has no visibility types, so they cannot enter any visibility info in Loadshop.
                // Thus, they have no booked loads that require visibility info for which they should be notified
                return null;
            }

            var loads = GetLoadsByTransactionTypes(bookedTransactionTypes, userIdentId, pickupWindowDate, TopsToGoCarrier, P44Carrier, false, int.MaxValue);

            if (loads == null || loads.Count() <= 0)
            {
                // No booked loads means no loads that require visibility info
                return null;
            }

            var badge = new VisibilityBadge
            {
                ApplicableDate = pickupWindowDate.Value,
                NumRequiringInfo = loads.Where(x => x.ShowVisibilityWarning == true).Count()
            };

            return badge;
        }

        public LoadData PendingAcceptLoad(LoadData loadData, Guid userId)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_Marketplace_Loads_Book);

            var update = _context.Loads
                .Include(x => x.Contacts)
                .Include(x => x.LoadStops)
                .Include(x => x.Equipment)
                .Include(x => x.LatestLoadTransaction)
                .SingleOrDefault(x => x.LoadId == loadData.LoadId);
            if (update == null)
            {
                throw new ValidationException("Load not found");
            }
            var originalLoadHistory = _mapper.Map<LoadHistoryEntity>(update);


            var user = _context.Users
                .Include(x => x.UserNotifications)
                .SingleOrDefault(x => x.IdentUserId == userId);
            if (user == null)
            {
                throw new ValidationException("Invalid User");
            }

            var scac = user.PrimaryScac;
            if (!IsScacAllowedToBookLoad(update.LoadId, update.LineHaulRate, scac))
            {
                throw new ValidationException($"{scac} is not allowed to book this load");
            }

            if (!IsValidLoadToBook(update.LatestLoadTransaction))
            {
                throw new ValidationException("This load is not eligible for booking at this time");
            }

            var customer = _context.Customers
                .Include(_ => _.CustomerCarrierScacContracts)
                .FirstOrDefault(_ => _.CustomerId == update.CustomerId);

            update.LineHaulRate = _serviceUtilities.GetContractRate(_context, update.LoadId, userId) ?? update.LineHaulRate;

            //use the latest line haul to determine the fees without changing the LineHaul on the load itself
            loadData.LineHaulRate = update.LineHaulRate;
            _loadshopFeeService.ApplyLoadshopFee(scac, loadData, new List<CustomerEntity> { customer });

            var newTransactionType = TransactionTypes.Pending;
            if (update.ManuallyCreated == true)
            {
                var isCustomersCarrier = _context.CustomerCarrierScacContracts?.Any(_ => _.CustomerId == update.CustomerId && _.Scac == scac) ?? false;
                if (!isCustomersCarrier)
                    newTransactionType = TransactionTypes.PreTender;
            }

            var claim = new LoadClaimEntity()
            {
                LineHaulRate = update.LineHaulRate,
                FuelRate = update.FuelRate,
                Scac = user.PrimaryScac,
                UserId = user.UserId,
                FlatFee = loadData.FeeData?.LoadshopFlatFee ?? 0m,
                PercentFee = loadData.FeeData?.LoadshopPercentFee ?? 0m,
                FeeAdd = loadData.FeeData?.LoadshopFeeAdd ?? false,
                LoadshopFee = loadData.FeeData?.LoadshopFee ?? 0m
            };

            var transaction = new LoadTransactionEntity()
            {
                LoadId = update.LoadId,
                TransactionTypeId = newTransactionType,
                Claim = claim
            };

            var contact = update.Contacts.FirstOrDefault() ?? new LoadContactEntity();
            _notificationService.SendPendingEmail(update, user, contact, _userContext.Email, claim);

            _context.LoadTransactions.Add(transaction);

            var newLoadHistory = _mapper.Map<LoadHistoryEntity>(update);
            if (_serviceUtilities.HasLoadChanged(originalLoadHistory, newLoadHistory))
                _context.LoadHistories.Add(newLoadHistory);

            _context.SaveChanges(_userContext.UserName);
            return loadData;
        }

        #endregion

        #region Customer Methods

        public LoadDetailData AcceptLoad(LoadDetailData loadData, Guid customerId, string username)
        {
            var update = _context.Loads
                .Include(x => x.LatestLoadTransaction).ThenInclude(x => x.Claim)
                .SingleOrDefault(x => x.ReferenceLoadId == loadData.ReferenceLoadId && x.Customer.IdentUserId == customerId);

            if (update == null)
            {
                // Try to find the load by PlatformPlusLoadId, because some loads come from TOPS with LS Numbers
                // instead of the normal tops LoadId
                update = _context.Loads
                    .Include(x => x.LatestLoadTransaction).ThenInclude(x => x.Claim)
                    .SingleOrDefault(x => x.PlatformPlusLoadId == loadData.ReferenceLoadId && x.Customer.IdentUserId == customerId);

                if (update == null)
                {
                    throw new ValidationException("Load not found");
                }
            }
            var originalLoadHistory = _mapper.Map<LoadHistoryEntity>(update);

            /**
             * When a load is booked, a Pending Transaction is entered.  When a load is Sent to the Shipper's Tender API via 
             * TRS_LoadBoard_003, then a SentToShipperTender Transaction is entered.  Both of these are valid previous 
             * transactions when it's time to Accept a load.
             */
            var isValidTransactionType = update.LatestTransactionTypeId == TransactionTypes.Pending || update.LatestTransactionTypeId == TransactionTypes.SentToShipperTender;
            if (update.LatestLoadTransaction == null || update.LatestLoadTransaction.Claim == null || !isValidTransactionType)
            {
                string errorMessage = $"Load cannot be accepted because it hasn't been booked. Current load status: {update.LatestTransactionTypeId}";
                throw new ValidationException(errorMessage);
            }

            var claim = update.LatestLoadTransaction.Claim;
            update.IsAccepted = true;
            var transaction = new LoadTransactionEntity()
            {
                LoadId = update.LoadId,
                LoadBoardId = loadData.LoadBoardId,
                TransactionTypeId = TransactionTypes.Accepted,
                Claim = new LoadClaimEntity()
                {
                    LineHaulRate = update.LineHaulRate,
                    FuelRate = update.FuelRate,
                    Scac = update.LatestLoadTransaction.Claim.Scac,
                    UserId = update.LatestLoadTransaction.Claim.UserId,
                    BillingLoadId = loadData.BillingLoadId,
                    BillingLoadDisplay = loadData.BillingLoadDisplay,
                    FlatFee = claim.FlatFee,
                    PercentFee = claim.PercentFee,
                    FeeAdd = claim.FeeAdd,
                    LoadshopFee = claim.LoadshopFee
                }
            };
            _context.LoadTransactions.Add(transaction);

            //set pending transaction's LoadBoardId
            var pendingTransaction = _context.LoadTransactions
                .Where(x => x.LoadId == update.LoadId && x.TransactionTypeId == "Pending" && x.LoadBoardId == null)
                .OrderByDescending(x => x.CreateDtTm)
                .FirstOrDefault();
            if (pendingTransaction != null)
            {
                pendingTransaction.LoadBoardId = loadData.LoadBoardId;
                _context.LoadTransactions.Update(pendingTransaction);
            }

            var newLoadHistory = _mapper.Map<LoadHistoryEntity>(update);
            if (_serviceUtilities.HasLoadChanged(originalLoadHistory, newLoadHistory))
                _context.LoadHistories.Add(newLoadHistory);

            _context.SaveChanges(username);
            return loadData;
        }

        public async Task<GenericResponse<LoadDetailData>> CreateLoad(LoadDetailData loadData, Guid customerId,
            string username,
            CreateLoadOptionsDto options,
            string urnPrefix = "urn:root")
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_Shopit_Load_Manual_Create);
            var customer = await _context.Customers.SingleOrDefaultAsync(x => x.IdentUserId == customerId);
            if (customer == null)
            {
                throw new ValidationException("Customer not found");
            }

            if (options.ManuallyCreated && !customer.AllowManualLoadCreation)
            {
                throw new ValidationException("Customer not configured for Order Entry");
            }

            var response = new GenericResponse<LoadDetailData>();
            _loadValidationService.ValidateLoad(loadData, customerId, options.ValidateAddress, options.ManuallyCreated, urnPrefix, response);

            if (!response.IsSuccess)
                return response;

            var load = await _context.Loads
                .SingleOrDefaultAsync(x => x.ReferenceLoadId == loadData.ReferenceLoadId && x.CustomerId == customer.CustomerId);

            if (load != null)
            {
                if (customer.ValidateUniqueReferenceLoadIds)
                {
                    throw new ValidationException($"Load {load.ReferenceLoadId} already exists.  Please use the UpdateLoad end-point or edit the load in Loadshop");
                }
                else
                {
                    var transactionType = GetUpdateTransactionType(loadData.LoadTransaction);
                    loadData.LoadTransaction = new LoadTransactionData()
                    {
                        LoadBoardId = loadData.LoadBoardId,
                        TransactionType = transactionType
                    };
                    var updateOptions = new UpdateLoadOptions()
                    {
                        MapObject = true,
                        AppendComments = true,
                        AddSmartSpot = options.AddSmartSpot,
                        OverrideLineHaulWithSmartSpot = options.OverrideLineHaulWithSmartSpot
                    };

                    return await UpdateLoad(loadData, customerId, username, updateOptions);
                }
            }

            load = _mapper.Map<LoadEntity>(loadData);
            load.LoadId = Guid.NewGuid();
            loadData.LoadId = load.LoadId;
            load.EquipmentId = loadData.EquipmentType;
            load.CustomerId = customer.CustomerId;
            load.LineHaulRate = Math.Round(loadData.LineHaulRate, 2, MidpointRounding.AwayFromZero);
            load.FuelRate = Math.Round(loadData.FuelRate, 2, MidpointRounding.AwayFromZero);
            load.ManuallyCreated = options.ManuallyCreated;
            SetCustomerLoadType(load, customer);

            if (string.IsNullOrWhiteSpace(load.Commodity))
            {
                var customerEntity = _context.Customers.FirstOrDefault(x => x.CustomerId == customer.CustomerId);
                if (string.IsNullOrWhiteSpace(customerEntity?.DefaultCommodity))
                {
                    throw new ValidationException($"Load {loadData.ReferenceLoadId} does not have a commodity set and the shipper doesn't have a default commodity set.");
                }
                load.Commodity = customerEntity.DefaultCommodity;
            }

            load.LoadTransactions = new List<LoadTransactionEntity>()
            {
                new LoadTransactionEntity()
                {
                    LoadBoardId = loadData.LoadBoardId,
                    TransactionTypeId = GetCreateTransactionType(loadData.LoadTransaction)
                },
            };
            load.Stops = (short)loadData.LoadStops.Count;

            load.ScacsSentWithLoad = loadData.CarrierScacs?.Count > 0;
            if (loadData.CarrierScacs != null)
            {
                load.CarrierScacs = loadData.CarrierScacs.Select(x =>
                {
                    return new LoadCarrierScacEntity()
                    {
                        Scac = x.Scac,
                        ContractRate = x.ContractRate
                    };
                }).ToList();
            }
            //Ensure we only have distinct SCACs
            load.CarrierScacs = load.CarrierScacs.GroupBy(_ => _.Scac?.ToLower().Trim()).Select(_ => _.First()).ToList();

            if (loadData.CarrierScacRestrictions != null)
            {
                load.CarrierScacRestrictions = loadData.CarrierScacRestrictions.Select(x =>
                {
                    return new LoadCarrierScacRestrictionEntity()
                    {
                        Scac = x.Scac,
                        LoadCarrierScacRestrictionTypeId = x.LoadCarrierScacRestrictionTypeId
                    };
                }).ToList();
            }
            //Ensure we only have distinct SCAC restrictions
            load.CarrierScacRestrictions = load.CarrierScacRestrictions.GroupBy(x => new
            {
                Scac = x.Scac?.ToLower().Trim(),
                TypeId = x.LoadCarrierScacRestrictionTypeId
            }).Select(_ => _.First()).ToList();

            load.HasScacRestrictions = load.CarrierScacRestrictions?.Count > 0;

            List<LoadLineItemEntity> lineItems = null;
            if (loadData.LineItems != null)
            {
                lineItems = loadData.LineItems.Select(x =>
                    new LoadLineItemEntity()
                    {
                        LoadLineItemNumber = x.LoadLineItemNumber,
                        ProductDescription = x.ProductDescription ?? "",
                        Quantity = x.Quantity,
                        UnitOfMeasureId = x.UnitOfMeasureId.Value,
                        Weight = x.Weight,
                        Volume = x.Volume,
                        CustomerPurchaseOrder = x.CustomerPurchaseOrder,
                        PickupStop = load.LoadStops.FirstOrDefault(_ => _.StopNbr == x.PickupStopNumber),
                        DeliveryStop = load.LoadStops.FirstOrDefault(_ => _.StopNbr == x.DeliveryStopNumber),
                    }).ToList();
            }

            if (options.AddSpecialInstructions)
            {
                // append special instructions
                var specialInstructions = await _specialInstructionsService.GetSpecialInstructionsForLoadAsync(load);

                if (specialInstructions != null && specialInstructions.Any())
                {
                    load.Comments += $"<br />{string.Join("<br />", specialInstructions.Select(x => x.Comments))}";
                }
            }

            //set direct miles
            var loadStops = load.LoadStops?.OrderBy(x => x.StopNbr).ToList();
            var origin = loadStops?.FirstOrDefault();
            var destination = loadStops?.LastOrDefault();

            if (options.ManuallyCreated)
            {
                load.Miles = await _mileageService.GetRouteMiles(loadData.LoadStops.ToList());
            }
            load.DirectMiles = load.Miles;
            if (origin != null && destination != null)
            {
                load.DirectMiles = _mileageService.GetDirectMiles(new MileageRequestData()
                {
                    OriginCity = origin.City,
                    OriginState = origin.State,
                    OriginPostalCode = origin.PostalCode,
                    OriginCountry = origin.Country,
                    DestinationCity = destination.City,
                    DestinationState = destination.State,
                    DestinationPostalCode = destination.PostalCode,
                    DestinationCountry = destination.Country,
                    DefaultMiles = load.Miles
                });
            }

            //cap rates
            var capRates = _commonService.GetCapRates(loadData.ReferenceLoadId);
            load.HCapRate = capRates?.HCapRate;
            load.XCapRate = capRates?.XCapRate;

            var history = _mapper.Map<LoadHistoryEntity>(load);
            history.Load = load;
            _context.Loads.Add(load);
            _context.LoadLineItems.AddRange(lineItems);
            _context.LoadHistories.Add(history);
            await _context.SaveChangesAsync(username);

            if (options.AddSmartSpot)
            {
                // add load transactionTypeId since there is a db trigger to update it.  transactionTypeId is required by spot price service
                load.LatestTransactionTypeId = load.LoadTransactions.FirstOrDefault()?.TransactionTypeId;

                // spot pricing requires the load be saved to the db, so now add smart spot pricing
                await AddSpotPricing(load);

                if (options.OverrideLineHaulWithSmartSpot)
                {
                    if (load.SmartSpotRate.HasValue && load.SmartSpotRate.Value > 0)
                    {
                        load.LineHaulRate = Math.Round(load.SmartSpotRate.Value - load.FuelRate, 2);
                    }
                    else
                    {
                        logger.LogWarning($"CreateLoad::Not overriding line haul rate with smart spot rate because Smart Spot is null or 0");
                    }
                }
                await _context.SaveChangesAsync(username);
            }

            response.Data = loadData;
            return response;
        }

        public async Task<GenericResponse<LoadDetailData[]>> CreateLoadsWithContinueOnFailure(LoadDetailData[] loadDataList, Guid customerId, string username,
            CreateLoadOptionsDto options)
        {
            var response = new GenericResponse<LoadDetailData[]>();
            var data = new List<LoadDetailData>();
            for (var i = 0; i < loadDataList.Length; i++)
            {
                var loadDetailData = loadDataList[i];

                if (options.RemoveLineHaulRate)
                {
                    loadDetailData.LineHaulRate = 0;
                }
                var resp = await CreateLoad(loadDetailData, customerId, username, options, $"urn:root:{i}");
                if (resp.IsSuccess)
                {
                    data.Add(resp.Data);
                }
                else
                {
                    foreach (var modelStateValue in resp.ModelState)
                    {
                        foreach (var error in modelStateValue.Value.Errors)
                        {
                            response.AddError(modelStateValue.Key, error.ErrorMessage);
                        }
                    }
                }
            }

            response.Data = data.ToArray();
            return response;
        }

        public LoadDetailData DeleteLoad(LoadDetailData loadData, Guid customerId, string username)
        {
            var currentLoad = _context.Loads.SingleOrDefault(x => x.ReferenceLoadId == loadData.ReferenceLoadId && x.Customer.IdentUserId == customerId);
            if (currentLoad == null)
            {
                throw new ValidationException("Load not found");
            }

            var transaction = new LoadTransactionEntity()
            {
                LoadId = currentLoad.LoadId,
                LoadBoardId = loadData.LoadBoardId,
                TransactionTypeId = TransactionTypes.Removed
            };
            _context.LoadTransactions.Add(transaction);
            _context.SaveChanges(username);
            return loadData;
        }

        public string GenerateReturnURL(LoadDetailData[] loads)
        {
            var separator = "";
            var oldSeparator = "";
            var url = new StringBuilder();
            var sepIdx = 0;
            var separators = _configuration.GetValue<string>("LoadshopShopItUrlLoadIdSeparators").Split(new Char[] { ',' });
            var maxSepIdx = separators.Count() - 1;

            if (loads != null)
            {
                foreach (var load in loads)
                {
                    if (load != null && !string.IsNullOrWhiteSpace(load.ReferenceLoadId))
                    {
                        if (!load.ReferenceLoadId.Contains(separators[sepIdx]))
                        {
                            separator = separators[sepIdx];
                        }
                        else
                        {
                            oldSeparator = separator;
                            var startIdx = sepIdx + 1;
                            separator = "";

                            for (int i = startIdx; i <= maxSepIdx; i++)
                            {
                                if (!load.ReferenceLoadId.Contains(separators[i]))
                                {
                                    sepIdx = i;
                                    separator = separators[sepIdx];
                                    break;
                                }
                            }

                            if (string.IsNullOrEmpty(separator))
                            {
                                break;
                            }

                            if (url.Length > 0 && oldSeparator != separator)
                            {
                                url = new StringBuilder(url.ToString().Replace(oldSeparator, separator));
                            }
                        }

                        url.Append(load.ReferenceLoadId + separator);
                    }
                    else
                    {
                        throw new ValidationException("Reference Load Id is required.");
                    }
                }

                if (string.IsNullOrEmpty(separator))
                {
                    throw new ValidationException("The sent Reference LoadIds contain: \";\", \"|\", \"~\", and \":\". This is not allowed.");
                }
            }

            url.Insert(0, _configuration.GetValue<string>("LoadshopShopItHomeTabURL") + "?loadids=");
            url.Append("&sep=" + separators[sepIdx]);

            return System.Net.WebUtility.UrlEncode(url.ToString());
        }

        public List<LoadDetailData> GetAllOpenLoadsByCustomerId(Guid customerId)
        {
            var loadDetailOpts = new GetLoadDetailOptions()
            {
                CustomerIdentUserId = customerId,
                TransactionTypes = GetOpenLoadStatuses(),
                IncludeContacts = true,
                IncludeStops = true
            };

            var loads = _loadQueryReposiory.GetLoadDetailViews(loadDetailOpts);

            return _mapper.Map<List<LoadDetailData>>(loads);
        }

        public List<LoadDetailData> GetAllPendingAcceptedLoads(Guid customerId)
        {
            var loadDetailOpts = new GetLoadDetailOptions()
            {
                CustomerIdentUserId = customerId,
                TransactionTypes = new List<string> { TransactionTypes.PreTender, TransactionTypes.Pending },
                IncludeContacts = true,
                IncludeStops = true
            };
            var loads = _loadQueryReposiory.GetLoadDetailViews(loadDetailOpts);

            return _mapper.Map<List<LoadDetailData>>(loads);
        }

        public bool GetAutoPostToLoadshop(Guid customerIdentUserId)
        {
            return _context.Customers
                .Where(x => x.IdentUserId == customerIdentUserId)
                .Select(x => x.AutoPostLoad)
                .FirstOrDefault();
        }

        public LoadTransactionEntity GetLatestTransaction(Guid loadId)
        {
            return _context.LoadTransactions
                .Include(x => x.Claim)
                .OrderByDescending(x => x.CreateDtTm)
                .FirstOrDefault(x => x.LoadId == loadId);
        }

        public LoadDetailData GetLoadByCustomerReferenceId(string id, Guid customerId)
        {
            var loadDetailOpts = new GetLoadDetailOptions()
            {
                ReferenceLoadId = id,
                CustomerIdentUserId = customerId,
                IncludeContacts = true,
                IncludeStops = true
            };

            var load = _loadQueryReposiory.GetLoadDetailViews(loadDetailOpts).FirstOrDefault();
            if (load == null)
            {
                throw new ValidationException("Load not found");
            }

            return _mapper.Map<LoadDetailData>(load);
        }

        /// <summary>
        /// Loads are automatically set to Pending when booked via the Loadshop marketplace.  They are then
        /// picked up by the TRS_Loadboard_003 service, and sent to their Shipper's Tender API, after which
        /// a SentToShipperTender transaction is entered.  PreTender, Pending and SentToShipperTender should be 
        /// logically considered as "Pending" statuses.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<LoadDetailData> HasPendingLoadshopClaim(string id)
        {
            var loadDetailOpts = new GetLoadDetailOptions()
            {
                ReferenceLoadId = id,
                TransactionTypes = new List<string> { TransactionTypes.PreTender, TransactionTypes.Pending, TransactionTypes.SentToShipperTender }
            };
            var loads = _loadQueryReposiory.GetLoadDetailViews(loadDetailOpts);
            return _mapper.Map<List<LoadDetailData>>(loads);
        }

        /// <summary>
        /// Previews the update to see what changes will happen if applied. This is currently used to determine whether the changes impact the load to keep it or
        /// remove it from the marketplace
        /// </summary>
        /// <param name="loadData"></param>
        /// <param name="customerId"></param>
        /// <param name="username"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<UpdateLoadResult> PreviewUpdateLoad(LoadDetailData loadData, Guid customerId, string username, UpdateLoadOptions options)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_Shopit_Load_Manual_Edit);
            var validationResponse = new GenericResponse<LoadDetailData>();
            var addressValidation = options.ValidateAddress ? OrderAddressValidationEnum.Validate : OrderAddressValidationEnum.None;
            _loadValidationService.ValidateLoad(loadData, customerId, addressValidation, options.ManuallyCreated, $"urn:root", validationResponse);
            if (!validationResponse.IsSuccess)
                return new UpdateLoadResult()
                {
                    ErrorMessage = validationResponse.GetAllMessages()
                };

            // ensure the update is not saved
            options.SaveChanges = false;

            var checkUpdateResult = await UpdateLoadInternal(loadData, customerId, username, options);
            return checkUpdateResult;
        }

        public List<LoadStopData> SetApptTypeOnLoadStops(List<LoadStopData> stops)
        {
            foreach (var stop in stops)
            {
                if (stop.AppointmentSchedulingCode == null)
                {
                    continue;
                }
                var isApptNecessary = !stop.AppointmentSchedulingCode.Equals("NAN", StringComparison.OrdinalIgnoreCase);
                if (isApptNecessary)
                {
                    var isCarrierScheduledAppt = stop.AppointmentSchedulingCode.Equals("CAR", StringComparison.OrdinalIgnoreCase);
                    if (isCarrierScheduledAppt)
                    {
                        stop.ApptType = "CS";
                    }
                    else
                    {
                        stop.ApptType = "AT";
                    }
                }
                else
                {
                    var isDateRangeSpecified = (stop.EarlyDtTm.HasValue && !stop.EarlyDtTm.Equals(DateTime.MinValue)) && !stop.LateDtTm.Equals(DateTime.MinValue);
                    if (isDateRangeSpecified)
                    {
                        var areDatesTheSame = stop.EarlyDtTm.Value.Date == stop.LateDtTm.Date;
                        if (areDatesTheSame)
                        {
                            var areTimesTheSame = stop.EarlyDtTm.Value == stop.LateDtTm;
                            if (areTimesTheSame)
                            {
                                stop.ApptType = "ON";
                            }
                            else
                            {
                                stop.ApptType = "AT";
                            }
                        }
                        else
                        {
                            stop.ApptType = "BY";
                        }
                    }
                    else
                    {
                        stop.ApptType = "BY";
                    }
                }
            }
            return stops;
        }

        public bool UpdateFuel(LoadUpdateFuelData loadFuel, Guid customerId, string username)
        {
            var customer = _context.Customers.SingleOrDefault(x => x.IdentUserId == customerId);
            if (customer == null)
            {
                throw new ValidationException($"Customer not found");
            }

            if (loadFuel == null)
            {
                throw new ValidationException("Load not found");
            }

            var update = _context.Loads
                .Include(x => x.Contacts)
                .Include(x => x.LoadStops)
                .Include(x => x.Equipment)
                .Include(x => x.LatestLoadTransaction).ThenInclude(x => x.Claim)
                .SingleOrDefault(x => x.ReferenceLoadId == loadFuel.ReferenceLoadId && x.Customer.IdentUserId == customerId);
            if (update == null)
            {
                throw new ValidationException("Load not found");
            }
            else if (IsLoadInThePast(update))
            {
                throw new ValidationException("Fuel cannot be updated on loads from the past");
            }

            var shouldUpdateFuel = loadFuel.FuelRate != update.FuelRate;
            if (shouldUpdateFuel)
            {
                var oldFuelRate = update.FuelRate;
                update.FuelRate = loadFuel.FuelRate;

                var newLoadHistory = _mapper.Map<LoadHistoryEntity>(update);
                _context.LoadHistories.Add(newLoadHistory);
                _context.SaveChanges(username);

                if (HasLoadBeenBooked(update.LatestLoadTransaction))
                {
                    update.LatestLoadTransaction.Claim.FuelRate = loadFuel.FuelRate;
                    _context.SaveChanges(username);

                    var bookedUser = _context.Users.FirstOrDefault(x => x.UserId == update.LatestLoadTransaction.Claim.UserId);
                    var contact = update.Contacts.FirstOrDefault();
                    var claim = update.LatestLoadTransaction.Claim;
                    _notificationService.SendFuelUpdateEmail(update, bookedUser, contact, oldFuelRate, claim);
                }
                else if (IsLoadPendingFuel(update.LatestLoadTransaction))
                {
                    _context.LoadTransactions.Add(new LoadTransactionEntity
                    {
                        LoadId = update.LoadId,
                        TransactionTypeId = TransactionTypes.Posted
                    });

                    _context.SaveChanges(username);
                }
            }

            return shouldUpdateFuel;
        }

        public async Task<GenericResponse<LoadDetailData>> UpdateLoad(LoadDetailData loadData, Guid customerId, string username, UpdateLoadOptions options)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_Shopit_Load_Manual_Edit);
            var response = new GenericResponse<LoadDetailData>();
            var transactionType = loadData.LoadTransaction.TransactionType;
            if (transactionType != TransactionTypeData.Removed && transactionType != TransactionTypeData.PendingRemove)
            {
                var addressValidation = options.ValidateAddress ? OrderAddressValidationEnum.Validate : OrderAddressValidationEnum.None;
                _loadValidationService.ValidateLoad(loadData, customerId, addressValidation, options.ManuallyCreated, $"urn:root", response);
            }
            if (!response.IsSuccess)
                return response;

            await UpdateLoadInternal(loadData, customerId, username, options);
            response.Data = loadData;
            return response;
        }

        public LoadUpdateScacData UpdateScacs(LoadUpdateScacData loadScacs, Guid customerId, string username)
        {
            var customer = _context.Customers.SingleOrDefault(x => x.IdentUserId == customerId);
            if (customer == null)
            {
                throw new ValidationException($"Customer not found");
            }

            if (loadScacs == null)
            {
                throw new ValidationException("Load not found");
            }

            var update = _context.Loads
                .Include(x => x.CarrierScacs)
                .Include(x => x.LatestLoadTransaction)
                .SingleOrDefault(x => x.ReferenceLoadId == loadScacs.ReferenceLoadId && x.Customer.IdentUserId == customerId);
            if (update == null)
            {
                throw new ValidationException("Load not found");
            }

            if (update.LatestTransactionTypeId != TransactionTypes.PendingRates)
            {
                string errorMessage = $"Load SCACs cannot be updated because load has not been created";
                if (!string.IsNullOrEmpty(update.LatestTransactionTypeId))
                {
                    errorMessage = $"Load SCACs cannot be updated because its current status is: {update.LatestTransactionTypeId}";
                }

                throw new ValidationException(errorMessage);
            }

            if (ShouldUpdateCarrierScacs(update.CarrierScacs, loadScacs.CarrierScacs))
            {
                //remove existing scacs
                foreach (var dbCarrierScac in update.CarrierScacs)
                {
                    _context.LoadCarrierScacs.Remove(dbCarrierScac);
                }

                //add new scacs
                if (loadScacs.CarrierScacs != null)
                {
                    //Ensure we only have distinct SCACs
                    loadScacs.CarrierScacs = loadScacs.CarrierScacs.GroupBy(_ => _.Scac?.ToLower().Trim()).Select(_ => _.First()).ToList();

                    foreach (var carrierScac in loadScacs.CarrierScacs)
                    {
                        var dbCarrierScac = new LoadCarrierScacEntity
                        {
                            LoadId = update.LoadId,
                            Scac = carrierScac.Scac,
                            ContractRate = carrierScac.ContractRate
                        };
                        _context.LoadCarrierScacs.Add(dbCarrierScac);
                    }
                }
            }

            var transaction = new LoadTransactionEntity()
            {
                LoadId = update.LoadId,
                LoadBoardId = loadScacs.LoadBoardId,
                TransactionTypeId = GetUpdateScacsNextTransactionTypeId(update.LoadId)
            };
            _context.LoadTransactions.Add(transaction);
            _context.SaveChanges(username);

            return loadScacs;
        }

        #endregion

        #region Private Methods

        private async Task AddSpotPricing(LoadEntity load)
        {
            try
            {
                var spotPrice = await _smartSpotPriceService.GetSmartSpotPriceAsync(load);

                if (spotPrice != null)
                {
                    load.SmartSpotRate = spotPrice.Price;
                    load.MachineLearningRate = spotPrice.MachineLearningRate;
                    load.DATGuardRate = spotPrice.DATGuardRate;
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Error occurred while fetching Smart Spot Pricing, not setting values, error: {e.Message}", e);
            }
        }

        private void AssertUserHasAccessToLoad(Guid loadId)
        {
            var unauthorized = new UnauthorizedAccessException("User does not have access to view this load");
            var userEntities = _securityService.GetAllMyAuthorizedEntities();
            if (!userEntities.Any()) throw unauthorized;

            var shipperEntities = userEntities.Where(x => x.Type == UserFocusEntityType.Shipper).ToList();
            var userScacs = userEntities.Where(x => x.Type == UserFocusEntityType.CarrierScac).ToList();

            if (shipperEntities.Any())
            {
                var shipperCustomerIds = shipperEntities.Select(x => Guid.Parse(x.Id)).ToList();
                if (!_context.Loads.Where(x => x.LoadId == loadId && shipperCustomerIds.Contains(x.CustomerId)).Any())
                {
                    throw unauthorized;
                }
            }
            else if (userScacs.Any())
            {
                var loadScacs = _context.LoadCarrierScacs.Where(x => x.LoadId == loadId).Select(x => x.Scac).ToList();
                if (!loadScacs.Any() || !userScacs.Any(x => loadScacs.Contains(x.Id)))
                {
                    throw unauthorized;
                }
            }
            else
            {
                throw unauthorized;
            }

            // Fall through to here means the user is authorized to view the details of this load
        }

        /// <summary>
        /// Gets create transaction type
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private string GetCreateTransactionType(LoadTransactionData transaction)
        {
            var transactionType = TransactionTypes.New;
            if (transaction?.TransactionType == TransactionTypeData.PendingAdd)
            {
                transactionType = TransactionTypes.PendingAdd;
            }

            return transactionType;
        }

        private PageableQuery<LoadViewData> GetLoadsByTransactionTypes(string[] transTypes, Guid userIdentId, DateTime? visibilityPickupWindowDate = null, bool topsToGoCarrier = false, bool p44Carrier = false, bool isMarketplace = false, int maxResults = 100)
        {
            var userPrimaryScac = _context.Users.SingleOrDefault(u => u.IdentUserId == userIdentId)?.PrimaryScac;
            var userAuthorizedScacs = _securityService.GetAuthorizedScasForCarrierByPrimaryScac().Select(cs => cs.Scac).ToArray();
            var onLoadShopTransactionTypes = new[] { TransactionTypes.New, TransactionTypes.Updated };
            var bookedUserTransActionsTypes = new[] { TransactionTypes.Accepted, TransactionTypes.Pending };
            var currentDate = _dateTime.Now;
            visibilityPickupWindowDate = visibilityPickupWindowDate ?? currentDate;
            var isAdmin = _securityService.IsAdmin();

            if (!string.IsNullOrEmpty(userPrimaryScac))
            {
                if (isMarketplace)
                {
                    IQueryable<LoadViewData> query;

                    if (isAdmin)
                    {
                        query = _loadQueryReposiory.GetLoadsForCarrierMarketplaceAsAdmin(transTypes);
                    }
                    else
                    {
                        query = _loadQueryReposiory.GetLoadsForCarrierMarketplace(transTypes, userPrimaryScac);
                    }

                    return query.ToPageableQuery(async (shippingLoadList) =>
                    {
                        var customerIds = shippingLoadList.Select(_ => _.CustomerId).ToList();
                        var customers = await _context.Customers
                            .Include(_ => _.CustomerCarrierScacContracts)
                            .Where(_ => customerIds.Contains(_.CustomerId))
                            .ToListAsync();

                        foreach (var l in shippingLoadList)
                        {
                            l.ShowVisibilityWarning = _loadQueryReposiory.ShouldShowVisibility(visibilityPickupWindowDate, topsToGoCarrier, p44Carrier, currentDate, l);
                            _loadshopFeeService.ApplyLoadshopFee(userPrimaryScac, l, customers);
                        }
                    }, int.MaxValue);

                }
                else
                {
                    return _loadQueryReposiory.GetLoadsForCarrierWithLoadClaim(transTypes, userAuthorizedScacs)
                            .ToPageableQuery(shippingLoadList =>
                            {
                                foreach (var l in shippingLoadList)
                                {
                                    l.ShowVisibilityWarning = _loadQueryReposiory.ShouldShowVisibility(visibilityPickupWindowDate, topsToGoCarrier, p44Carrier, currentDate, l);
                                    l.IsPlatformPlus = l.Scac != null && !l.HasContractedCarrierScac;
                                    _loadshopFeeService.ReapplyLoadshopFeeToLineHaul(l);
                                }

                                return Task.CompletedTask;
                            }, maxResults);
                }
            }

            return PageableQuery<LoadViewData>.Empty();
        }

        /// <summary>
        /// Gets open load statuses
        /// </summary>
        /// <returns></returns>
        private List<string> GetOpenLoadStatuses()
        {
            return new List<string> { TransactionTypes.PendingAdd, TransactionTypes.PendingUpdate, TransactionTypes.New, TransactionTypes.Updated };
        }

        /// <summary>
        /// Gets next transaction type when updating a load's scacs
        /// </summary>
        /// <param name="loadId"></param>
        /// <returns></returns>
        private string GetUpdateScacsNextTransactionTypeId(Guid loadId)
        {
            var transactions = _context.LoadTransactions
                .Where(x => x.LoadId == loadId)
                .OrderByDescending(x => x.CreateDtTm);

            return GetUpdateScacsNextTransactionTypeId(transactions.ToList());
        }

        /// <summary>
        /// Gets the transaction type
        /// </summary>
        /// <param name="transactions"></param>
        /// <returns></returns>
        private string GetUpdateScacsNextTransactionTypeId(List<LoadTransactionEntity> transactions)
        {
            var result = TransactionTypes.New;
            if (transactions != null)
            {
                transactions = transactions.OrderByDescending(x => x.CreateDtTm).ToList();
                var postedTransaction = transactions.Where(x => x.TransactionTypeId == TransactionTypes.Posted).FirstOrDefault();
                if (postedTransaction != null)
                {
                    var postedIndex = transactions.IndexOf(postedTransaction);
                    if (postedIndex > 0 && postedIndex < (transactions.Count - 1))
                    {
                        var previousTransaction = transactions[postedIndex + 1];
                        if (previousTransaction != null)
                        {
                            if (previousTransaction.TransactionTypeId == TransactionTypes.PendingUpdate)
                            {
                                result = TransactionTypes.Updated;
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets update transaction type
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private TransactionTypeData GetUpdateTransactionType(LoadTransactionData transaction)
        {
            var transactionType = TransactionTypeData.Updated;
            if (transaction?.TransactionType == TransactionTypeData.PendingAdd)
            {
                transactionType = TransactionTypeData.PendingUpdate;
            }

            return transactionType;
        }

        /// <summary>
        /// Checks if a load has been booked
        /// </summary>
        /// <param name="latestTransaction"></param>
        /// <returns></returns>
        private bool HasLoadBeenBooked(LoadTransactionEntity latestTransaction)
        {
            var bookedTransactionTypes = new List<string>() { TransactionTypes.PreTender, TransactionTypes.Pending, TransactionTypes.SentToShipperTender, TransactionTypes.Accepted };
            return bookedTransactionTypes.Contains(latestTransaction?.TransactionTypeId) && latestTransaction?.Claim != null;
        }

        /// <summary>
        /// Checks if a load is in the past
        /// </summary>
        /// <param name="load"></param>
        /// <returns></returns>
        private bool IsLoadInThePast(LoadEntity load)
        {
            var result = true;
            if (load?.LoadStops != null)
            {
                var firstStop = load.LoadStops.OrderBy(x => x.StopNbr).FirstOrDefault();
                if (firstStop != null)
                {
                    result = _dateTime.Now > (firstStop.EarlyDtTm ?? firstStop.LateDtTm);
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if a load is pending fuel
        /// </summary>
        /// <param name="latestTransaction"></param>
        /// <returns></returns>
        private bool IsLoadPendingFuel(LoadTransactionEntity latestTransaction)
        {
            return latestTransaction?.TransactionTypeId == TransactionTypes.PendingFuel;
        }

        /// <summary>
        /// Checks if a scac is allowed to book a load
        /// </summary>
        /// <param name="loadId"></param>
        /// <param name="linehaulRate"></param>
        /// <param name="userPrimaryScac"></param>
        /// <returns></returns>
        private bool IsScacAllowedToBookLoad(Guid loadId, decimal linehaulRate, string userPrimaryScac)
        {
            var scac = _context.LoadCarrierScacs
                .Include(lcs => lcs.CarrierScac)
                .SingleOrDefault(x => x.LoadId == loadId && x.Scac == userPrimaryScac);

            if (scac == null || (scac.ContractRate.HasValue && scac.ContractRate.Value > linehaulRate && !scac.CarrierScac.IsDedicated))
            {
                return false;
            }

            var scacRestrictions = _context.LoadCarrierScacRestrictions
                .Where(x => x.LoadId == loadId).ToList();
            if (scacRestrictions != null)
            {
                var useOnlyScacs = scacRestrictions.Where(x => x.LoadCarrierScacRestrictionTypeId == Convert.ToInt32(LoadCarrierScacRestrictionTypeEnum.UseOnly))
                    .Select(x => x.Scac).ToList();
                var doNotUseScacs = scacRestrictions.Where(x => x.LoadCarrierScacRestrictionTypeId == Convert.ToInt32(LoadCarrierScacRestrictionTypeEnum.DoNotUse))
                    .Select(x => x.Scac).ToList();
                if ((useOnlyScacs != null && useOnlyScacs.Count > 0 && !useOnlyScacs.Contains(userPrimaryScac)) ||
                    (doNotUseScacs != null && doNotUseScacs.Contains(userPrimaryScac)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if transaction is a valid load to book
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool IsValidLoadToBook(LoadTransactionEntity transaction)
        {
            return transaction != null && (transaction.TransactionTypeId == TransactionTypes.New || transaction.TransactionTypeId == TransactionTypes.Updated);
        }

        /// <summary>
        /// Populates load update from details
        /// </summary>
        /// <param name="update"></param>
        /// <param name="loadData"></param>
        private void PopulateLoadUpdate(LoadEntity update, LoadDetailData loadData, UpdateLoadOptions options)
        {
            if (update == null || loadData == null)
            {
                return;
            }
            loadData.LoadId = update.LoadId;
            if (options.AppendComments)
            {
                // if we are appending comments, ensure the comment doesn't exist already so we don't dup
                if (!string.IsNullOrEmpty(update.Comments) &&
                    !string.IsNullOrEmpty(loadData.Comments) &&
                    !update.Comments.Contains(loadData.Comments))
                {
                    update.Comments += loadData.Comments;
                }
            }
            else
            {
                update.Comments = loadData.Comments;
            }

            if (string.IsNullOrWhiteSpace(loadData.Commodity))
            {
                var customerEntity = _context.Customers.FirstOrDefault(x => x.CustomerId == update.CustomerId);
                if (string.IsNullOrWhiteSpace(customerEntity?.DefaultCommodity))
                {
                    throw new ValidationException($"Load {loadData.ReferenceLoadId} does not have a commodity set and the shipper doesn't have a default commodity set.");
                }
                loadData.Commodity = customerEntity.DefaultCommodity;
            }
            update.Commodity = loadData.Commodity;

            update.Cube = loadData.Cube;
            update.ReferenceLoadId = loadData.ReferenceLoadId;
            update.ReferenceLoadDisplay = loadData.ReferenceLoadDisplay;
            update.EquipmentId = loadData.EquipmentType;
            update.IsHazMat = loadData.IsHazMat;
            update.Miles = loadData.Miles;
            if (!options.IgnoreFuel)
            {
                update.FuelRate = Math.Round(loadData.FuelRate, 2, MidpointRounding.AwayFromZero);
            }

            if (!options.IgnoreLineHaulRate)
            {
                update.LineHaulRate = Math.Round(loadData.LineHaulRate, 2, MidpointRounding.AwayFromZero);
            }

            if (loadData.LoadStops != null)
            {
                update.Stops = (short)loadData.LoadStops.Count;
            }
            update.Weight = loadData.Weight;
            update.ScacsSentWithLoad = loadData.CarrierScacs?.Count > 0;

            update.TransportationModeId = loadData.TransportationModeId;
            update.ShipperPickupNumber = loadData.ShipperPickupNumber;

            SetCustomerLoadType(update, update.Customer);
        }

        /// <summary>
        /// Sets customer load type based on customer type
        /// </summary>
        /// <param name="update"></param>
        /// <param name="customer"></param>
        private void SetCustomerLoadType(LoadEntity update, CustomerEntity customer)
        {
            if (update != null && customer != null)
            {
                update.CustomerLoadTypeId = null;
                if (customer.CustomerLoadTypeId.HasValue &&
                    (!customer.CustomerLoadTypeExpirationDate.HasValue || customer.CustomerLoadTypeExpirationDate.Value > _dateTime.Today))
                {
                    update.CustomerLoadTypeId = customer.CustomerLoadTypeId.Value;
                }
            }
        }

        private void SetVisibilityParams(Guid userId, ref DateTime? pickupWindowDate, ref bool P44Carrier, ref bool TopsToGoCarrier)
        {
            var user = _context.Users
                                .Include(u => u.PrimaryScacEntity)
                                .SingleOrDefault(x => x.IdentUserId == userId);
            if (user == null)
            {
                throw new Exception("Invalid User");
            }

            List<string> visibilityTypes = null;
            if (user.PrimaryScacEntity != null)
                visibilityTypes = _commonService.GetCarrierVisibilityTypes(user.Username, user.PrimaryScacEntity?.CarrierId);

            if (visibilityTypes == null || visibilityTypes.Count <= 0)
            {
                pickupWindowDate = null;
                P44Carrier = false;
                TopsToGoCarrier = false;
            }
            else
            {
                var numHoursUntilPickup = 8;
                int.TryParse(_configuration["LoadBoardVisibilityHoursUntilPickup"], out numHoursUntilPickup);
                pickupWindowDate = _dateTime.Now + TimeSpan.FromHours(numHoursUntilPickup);

                if (visibilityTypes.Contains(CarrierVisibilityTypes.Project44))
                    P44Carrier = true;

                if (visibilityTypes.Contains(CarrierVisibilityTypes.TopsToGo))
                    TopsToGoCarrier = true;
            }
        }

        /// <summary>
        /// Checks if load carrier scacs should be updated
        /// </summary>
        /// <param name="existingScacs"></param>
        /// <param name="newScacs"></param>
        /// <returns></returns>
        private bool ShouldUpdateCarrierScacs(List<LoadCarrierScacEntity> existingScacs, List<LoadCarrierScacData> newScacs)
        {
            bool updateCarrierScacs = false;
            //don't remove existing scacs if no new scacs are sent
            if (existingScacs != null && newScacs != null && newScacs.Count > 0)
            {
                var existScacs = existingScacs.Select(x => new { x.Scac, x.ContractRate }).OrderBy(x => x.Scac).ToList();
                var inputScacs = newScacs.Select(x => new { x.Scac, x.ContractRate }).OrderBy(x => x.Scac).ToList();
                updateCarrierScacs = existScacs.Count != inputScacs.Count;

                if (!updateCarrierScacs)
                {
                    for (int i = 0; i < existScacs.Count; i++)
                    {
                        if (existScacs[i].Scac != inputScacs[i].Scac || existScacs[i].ContractRate != inputScacs[i].ContractRate)
                        {
                            updateCarrierScacs = true;
                            break;
                        }
                    }
                }
            }

            return updateCarrierScacs;
        }

        /// <summary>
        /// Checks if load carrier scac restrictions should be updated
        /// </summary>
        /// <param name="existingScacs"></param>
        /// <param name="newScacs"></param>
        /// <returns></returns>
        private bool ShouldUpdateCarrierScacRestrictions(List<LoadCarrierScacRestrictionEntity> existingScacs, List<LoadCarrierScacRestrictionData> newScacs)
        {
            bool updateCarrierScacRestrictions = false;
            //don't remove existing scacs if no new scacs are sent
            if (existingScacs != null && newScacs != null && newScacs.Count > 0)
            {
                var existScacs = existingScacs.Select(x => new { x.Scac, x.LoadCarrierScacRestrictionTypeId }).OrderBy(x => x.Scac).ThenBy(x => x.LoadCarrierScacRestrictionTypeId).ToList();
                var inputScacs = newScacs.Select(x => new { x.Scac, x.LoadCarrierScacRestrictionTypeId }).OrderBy(x => x.Scac).ThenBy(x => x.LoadCarrierScacRestrictionTypeId).ToList();
                updateCarrierScacRestrictions = existScacs.Count != inputScacs.Count;

                if (!updateCarrierScacRestrictions)
                {
                    for (int i = 0; i < existScacs.Count; i++)
                    {
                        if (existScacs[i].Scac != inputScacs[i].Scac || existScacs[i].LoadCarrierScacRestrictionTypeId != inputScacs[i].LoadCarrierScacRestrictionTypeId)
                        {
                            updateCarrierScacRestrictions = true;
                            break;
                        }
                    }
                }
            }

            return updateCarrierScacRestrictions;
        }

        private bool ShouldUpdateSpecialInstructions(LoadEntity loadEntity, LoadDetailData updatedData)
        {
            if (loadEntity == null || updatedData == null)
            {
                return false;
            }

            if (loadEntity.EquipmentId != updatedData.EquipmentType)
            {
                return true;
            }
            if (!updatedData.LoadStops.Any())
            {
                return false;
            }
            var origin = updatedData.LoadStops.OrderBy(x => x.StopNbr).First();
            var dbOrigin = loadEntity.LoadStops.OrderBy(x => x.StopNbr).First();

            var dest = updatedData.LoadStops.OrderBy(x => x.StopNbr).Last();
            var dbDest = loadEntity.LoadStops.OrderBy(x => x.StopNbr).Last();

            if ((dbOrigin.Address1 != origin.Address1) ||
                (dbOrigin.Address2 != origin.Address2) ||
                (dbOrigin.Address3 != origin.Address3) ||
                (dbOrigin.City != origin.City) ||
                (dbOrigin.State != origin.State) ||
                (dbOrigin.PostalCode != origin.PostalCode) ||
                (dbOrigin.Country != origin.Country) ||
                (dbDest.Address1 != dest.Address1) ||
                (dbDest.Address2 != dest.Address2) ||
                (dbDest.Address3 != dest.Address3) ||
                (dbDest.City != dest.City) ||
                (dbDest.State != dest.State) ||
                (dbDest.PostalCode != dest.PostalCode) ||
                (dbDest.Country != dest.Country))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// This method uses EF's change tracker to verify the LoadEntity and the LoadStop entities for changes.
        /// 
        /// Specific changes such as weight, cube and time on the load stop's shouldn't affect the load's status if in the marketplace
        /// </summary>
        /// <param name="loadEntity"></param>
        /// <returns></returns>
        private bool ShouldKeepLoadInMarketplace(LoadEntity loadEntity)
        {
            var marketplaceStatuses = _serviceUtilities.MapShipperSearchTypeToTransactionList(ShipperSearchTypeData.Posted);

            // check if the load is one of the applicable statuses for the marketplace
            if (!marketplaceStatuses.Any(x => x.Equals(loadEntity.LatestTransactionTypeId, StringComparison.InvariantCultureIgnoreCase)))
            {
                return false;
            }

            var changeCount = 0;

            // load is currrently on marketplace, check if load changed time
            var changes = _context.Entry(loadEntity).Properties.Where(x => x.IsModified)
                                .Where(x => !(string.IsNullOrEmpty(x.CurrentValue?.ToString()) && string.IsNullOrEmpty(x.OriginalValue?.ToString())))
                                .ToList();
            changeCount = changes.Count;
            // check if weight and cube are the only changes
            if (changes.Count > 0)
            {
                return changes.All(x => x.Metadata.Name == nameof(LoadEntity.Cube) || x.Metadata.Name == nameof(LoadEntity.Weight));
            }
            var nonLoadStopTimeChange = false;
            foreach (var item in loadEntity.LoadStops)
            {
                var loadStopChanges = _context.Entry(item).Properties.Where(x => x.IsModified)
                                        .Where(x => !(string.IsNullOrEmpty(x.CurrentValue?.ToString()) && string.IsNullOrEmpty(x.OriginalValue?.ToString())))
                                        .ToList();
                changeCount += loadStopChanges.Count;
                if (loadStopChanges.Count == 0)
                {
                    continue;
                }
                else if (loadStopChanges.All(x => x.Metadata.Name == nameof(LoadStopEntity.EarlyDtTm) ||
                      x.Metadata.Name == nameof(LoadStopEntity.LateDtTm)))
                {
                    // check if the time change only
                    var timePropertyChanges = loadStopChanges.Where(x => x.Metadata.Name == nameof(LoadStopEntity.EarlyDtTm) ||
                                                        x.Metadata.Name == nameof(LoadStopEntity.LateDtTm));

                    foreach (var timeChange in timePropertyChanges)
                    {
                        var dbValue = timeChange.OriginalValue as DateTime?;
                        var currentValue = timeChange.CurrentValue as DateTime?;

                        if (dbValue.HasValue && currentValue.HasValue &&
                            dbValue.Value.Date != currentValue.Value.Date)
                        {
                            nonLoadStopTimeChange = true;
                            break;
                        }
                    }
                }
                else
                {
                    // changes outside of pick / deliever times
                    nonLoadStopTimeChange = true;
                }

                if (nonLoadStopTimeChange) break;
            }
            // start and stop changes only had time changes
            if (changeCount == 0 || (loadEntity.LoadStops.Any() && !nonLoadStopTimeChange))
            {
                return true;
            }
            return false;
        }

        private async Task<UpdateLoadResult> UpdateLoadInternal(LoadDetailData loadData, Guid customerId, string username, UpdateLoadOptions options)
        {
            var result = new UpdateLoadResult();
            var update = await _context.Loads
                .Include(x => x.LoadStops).ThenInclude(x => x.Contacts)
                .Include(x => x.LoadStops).ThenInclude(x => x.DeliveryLineItems)
                .Include(x => x.Contacts)
                .Include(x => x.CarrierScacs)
                .Include(x => x.CarrierScacRestrictions)
                .Include(x => x.LoadServiceTypes)
                .Include(x => x.Customer)
                .SingleOrDefaultAsync(x => x.ReferenceLoadId == loadData.ReferenceLoadId && x.Customer.IdentUserId == customerId);
            if (update == null)
            {
                throw new ValidationException("Load not found");
            }
            var originalLoadHistory = _mapper.Map<LoadHistoryEntity>(update);
            var validPendingLoadTransactionTypes = new List<string>
            {
                TransactionTypes.PreTender,
                TransactionTypes.Pending,
                TransactionTypes.SentToShipperTender,
                TransactionTypes.Accepted
            };
            if (validPendingLoadTransactionTypes.Contains(update.LatestTransactionTypeId))
            {
                throw new ValidationException($"Load cannot be updated.  Invalid Status: {update.LatestTransactionTypeId}");
            }

            if (options.MapObject)
            {
                // check if the origin / dest / equipment got updated to see if we need to repull special instructions
                // need to update entity load stop before we get instructions
                var refetchSpecialInstructions = ShouldUpdateSpecialInstructions(update, loadData);


                PopulateLoadUpdate(update, loadData, options);

                foreach (var dbStop in update.LoadStops)
                {
                    // remove stops no longer needed in db
                    var stop = loadData.LoadStops.SingleOrDefault(x => x.StopNbr == dbStop.StopNbr);
                    if (stop == null)
                    {
                        _context.LoadStops.Remove(dbStop);
                    }
                }

                var dbStops = new List<LoadStopEntity>();
                foreach (var stop in loadData.LoadStops)
                {
                    // update/add stops
                    var dbStop = update.LoadStops.SingleOrDefault(x => x.StopNbr == stop.StopNbr);
                    if (dbStop == null)
                    {
                        dbStop = new LoadStopEntity();
                        _context.LoadStops.Add(dbStop);
                    }
                    dbStops.Add(dbStop);//track for relational use on line items
                                        //_mapper.Map(stop, dbStop);
                    dbStop.LoadId = update.LoadId;
                    dbStop.StopNbr = stop.StopNbr;
                    dbStop.StopTypeId = stop.StopTypeId;
                    dbStop.LocationName = stop.LocationName;
                    dbStop.Address1 = stop.Address1;
                    dbStop.Address2 = stop.Address2;
                    dbStop.Address3 = stop.Address3;
                    dbStop.City = stop.City;
                    dbStop.State = stop.State;
                    dbStop.Country = stop.Country;
                    dbStop.PostalCode = stop.PostalCode;
                    dbStop.Latitude = stop.Latitude;
                    dbStop.Longitude = stop.Longitude;
                    dbStop.EarlyDtTm = stop.EarlyDtTm;
                    dbStop.LateDtTm = stop.LateDtTm;
                    dbStop.ApptType = stop.ApptType;
                    dbStop.Instructions = stop.Instructions;
                    dbStop.AppointmentSchedulerConfirmationTypeId = stop.AppointmentSchedulerConfirmationTypeId;
                    dbStop.IsLive = stop.IsLive;

                    //replace any contacts on the stop
                    if (dbStop.Contacts != null)
                    {
                        foreach (var contact in dbStop.Contacts)
                            _context.LoadStopContacts.Remove(contact);
                    }
                    if (stop.Contacts != null)
                    {
                        foreach (var contact in stop.Contacts)
                        {
                            var dbContact = new LoadStopContactEntity();
                            dbContact.FirstName = contact.FirstName;
                            dbContact.LastName = contact.LastName;
                            dbContact.Email = contact.Email;
                            dbContact.PhoneNumber = contact.PhoneNumber;
                            dbContact.LoadStop = dbStop;
                            _context.LoadStopContacts.Add(dbContact);
                        }
                    }

                    //replace any line items on the stop
                    //line items are tied to a delivery stop but also reference a pickup
                    if (loadData.LineItems != null && dbStop.StopTypeId == (int?)StopTypeEnum.Delivery)
                    {
                        if (dbStop.DeliveryLineItems != null)
                        {
                            foreach (var dbLineItem in dbStop.DeliveryLineItems)
                            {
                                _context.LoadLineItems.Remove(dbLineItem);
                            }
                        }
                    }
                }
                if (refetchSpecialInstructions && options.UpdateSpecialInstructions)
                {
                    var specialInstructions = await _specialInstructionsService.GetSpecialInstructionsForLoadAsync(update);

                    if (specialInstructions != null && specialInstructions.Any())
                    {
                        // if any comments exists we wipe then and replace with the new special instructions
                        update.Comments = string.Join("<br />", specialInstructions.Select(x => x.Comments));
                    }
                }

                if (loadData.Contacts != null)
                {
                    // remove no longer needed contacts
                    foreach (var dbContact in update.Contacts)
                    {
                        _context.LoadContacts.Remove(dbContact);
                    }

                    // add contacts
                    foreach (var contact in loadData.Contacts)
                    {
                        var dbContact = new LoadContactEntity();
                        _context.LoadContacts.Add(dbContact);
                        //_mapper.Map(contact, dbContact);
                        dbContact.LoadId = update.LoadId;
                        dbContact.Display = contact.Display;
                        dbContact.Email = contact.Email;
                        dbContact.Phone = contact.Phone;
                    }
                }

                if (loadData.LineItems != null)
                {
                    // add/readd line items
                    foreach (var lineItem in loadData.LineItems)
                    {
                        var dbLineItem = new LoadLineItemEntity();
                        dbLineItem.LoadLineItemNumber = lineItem.LoadLineItemNumber;
                        dbLineItem.ProductDescription = lineItem.ProductDescription ?? "";
                        dbLineItem.Quantity = lineItem.Quantity;
                        dbLineItem.UnitOfMeasureId = lineItem.UnitOfMeasureId.Value;
                        dbLineItem.Weight = lineItem.Weight;
                        dbLineItem.Volume = lineItem.Volume;
                        dbLineItem.CustomerPurchaseOrder = lineItem.CustomerPurchaseOrder;
                        dbLineItem.PickupStop = dbStops.FirstOrDefault(_ => _.StopNbr == lineItem.PickupStopNumber);
                        dbLineItem.DeliveryStop = dbStops.FirstOrDefault(_ => _.StopNbr == lineItem.DeliveryStopNumber);
                        _context.LoadLineItems.Add(dbLineItem);
                    }
                }

                if (loadData.ServiceTypes != null)
                {
                    foreach (var dbLoadServiceType in update.LoadServiceTypes)
                    {
                        _context.LoadServiceTypes.Remove(dbLoadServiceType);
                    }

                    foreach (var serviceType in loadData.ServiceTypes)
                    {
                        var dbLoadServiceType = new LoadServiceTypeEntity();
                        dbLoadServiceType.LoadId = update.LoadId;
                        dbLoadServiceType.ServiceTypeId = serviceType.ServiceTypeId;
                        _context.LoadServiceTypes.Add(dbLoadServiceType);
                    }
                }

                if (ShouldUpdateCarrierScacs(update.CarrierScacs, loadData.CarrierScacs) || update.LineHaulRate != loadData.LineHaulRate)
                {
                    // remove no longer needed scacs
                    foreach (var dbCarrierScac in update.CarrierScacs)
                    {
                        _context.LoadCarrierScacs.Remove(dbCarrierScac);
                    }

                    // add scacs
                    if (loadData.CarrierScacs != null)
                    {
                        //Ensure we only have distinct SCACs
                        loadData.CarrierScacs = loadData.CarrierScacs.GroupBy(_ => _.Scac?.ToLower().Trim()).Select(_ => _.First()).ToList();

                        foreach (var carrierScac in loadData.CarrierScacs)
                        {
                            var dbCarrierScac = new LoadCarrierScacEntity
                            {
                                LoadId = update.LoadId,
                                Scac = carrierScac.Scac,
                                ContractRate = carrierScac.ContractRate
                            };
                            _context.LoadCarrierScacs.Add(dbCarrierScac);
                        }
                    }
                }

                if (ShouldUpdateCarrierScacRestrictions(update.CarrierScacRestrictions, loadData.CarrierScacRestrictions))
                {
                    // remove no longer needed scacs
                    foreach (var dbCarrierScacRestriction in update.CarrierScacRestrictions)
                    {
                        _context.LoadCarrierScacRestrictions.Remove(dbCarrierScacRestriction);
                    }

                    // add scac restrictions
                    if (loadData.CarrierScacRestrictions != null)
                    {
                        //Ensure we only have distinct SCACs
                        loadData.CarrierScacRestrictions = loadData.CarrierScacRestrictions.GroupBy(x => new
                        {
                            Scac = x.Scac?.ToLower().Trim(),
                            TypeId = x.LoadCarrierScacRestrictionTypeId
                        }).Select(_ => _.First()).ToList();

                        foreach (var carrierScacRestriction in loadData.CarrierScacRestrictions)
                        {
                            var dbCarrierScacRestriction = new LoadCarrierScacRestrictionEntity
                            {
                                LoadId = update.LoadId,
                                Scac = carrierScacRestriction.Scac,
                                LoadCarrierScacRestrictionTypeId = carrierScacRestriction.LoadCarrierScacRestrictionTypeId
                            };
                            _context.LoadCarrierScacRestrictions.Add(dbCarrierScacRestriction);
                        }
                    }
                }

                update.HasScacRestrictions = loadData.CarrierScacRestrictions?.Count > 0;

                //set direct miles
                var loadStops = loadData.LoadStops?.OrderBy(x => x.StopNbr);
                var origin = loadStops?.FirstOrDefault();
                var destination = loadStops?.LastOrDefault();

                if (update.ManuallyCreated == true)
                {
                    update.Miles = await _mileageService.GetRouteMiles(loadStops.ToList());
                }

                update.DirectMiles = update.Miles;
                if (origin != null && destination != null)
                {
                    update.DirectMiles = _mileageService.GetDirectMiles(new MileageRequestData()
                    {
                        OriginCity = origin.City,
                        OriginState = origin.State,
                        OriginPostalCode = origin.PostalCode,
                        OriginCountry = origin.Country,
                        DestinationCity = destination.City,
                        DestinationState = destination.State,
                        DestinationPostalCode = destination.PostalCode,
                        DestinationCountry = destination.Country,
                        DefaultMiles = update.Miles
                    });
                }

                //cap rates
                var capRates = _commonService.GetCapRates(update.ReferenceLoadId);
                update.HCapRate = capRates?.HCapRate;
                update.XCapRate = capRates?.XCapRate;
            }

            if (options.AddSmartSpot)
            {
                // spot pricing requires the load be saved to the db, so now add smart spot pricing
                await AddSpotPricing(update);

                if (options.OverrideLineHaulWithSmartSpot)
                {
                    if (update.SmartSpotRate.HasValue && update.SmartSpotRate.Value > 0)
                    {
                        update.LineHaulRate = Math.Round(update.SmartSpotRate.Value - update.FuelRate, 2);
                    }
                    else
                    {
                        logger.LogWarning($"UpdateLoadInternal::Not overriding line haul rate with smart spot rate because Smart Spot is null or 0");
                    }
                }
            }

            var transaction = new LoadTransactionEntity()
            {
                LoadId = update.LoadId,
                LoadBoardId = loadData.LoadBoardId,
                TransactionTypeId = loadData.LoadTransaction.TransactionType.ToString()
            };
            _context.LoadTransactions.Add(transaction);

            var keepLoadInMarketPlace = ShouldKeepLoadInMarketplace(update);
            var transactionType = loadData.LoadTransaction.TransactionType;
            // if the load was posted and an applicable change happened, keep it on the marketplace 
            // this additional record should cause the DB trigger to use this as the most recent transaction type
            if (keepLoadInMarketPlace && transactionType != TransactionTypeData.Removed && transactionType != TransactionTypeData.PendingRemove)
            {
                var marketPlaceTransaction = new LoadTransactionEntity()
                {
                    LoadId = update.LoadId,
                    LoadBoardId = loadData.LoadBoardId,
                    TransactionTypeId = update.LatestTransactionTypeId
                };
                _context.LoadTransactions.Add(marketPlaceTransaction);
                result.LoadKeptInMarketplace = true;
            }

            var newLoadHistory = _mapper.Map<LoadHistoryEntity>(update);
            if (_serviceUtilities.HasLoadChanged(originalLoadHistory, newLoadHistory))
                _context.LoadHistories.Add(newLoadHistory);

            if (options.SaveChanges)
            {
                await _context.SaveChangesAsync(username);
                result.SavedSuccessfully = true;
            }
            else
            {
                _context.Dispose();
            }

            return result;
        }

        #endregion
    }
}
