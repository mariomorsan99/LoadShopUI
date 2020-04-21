using Loadshop.Data;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Dto;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loadshop.DomainServices.Loadshop.Services.Repositories
{
    public class LoadQueryRepository : ILoadQueryRepository
    {
        private readonly ISecurityService _securityService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly LoadshopDataContext _context;
        private readonly string[] bookedTransactionTypes = new[] { TransactionTypes.Accepted, TransactionTypes.PreTender, TransactionTypes.Pending, TransactionTypes.SentToShipperTender };

        public LoadQueryRepository(LoadshopDataContext loadshopDataContext, ISecurityService securityService, IDateTimeProvider dateTimeProvider)
        {
            _securityService = securityService;
            _dateTimeProvider = dateTimeProvider;
            _context = loadshopDataContext;
        }

        public int GetNumberOfBookedLoadsForCarrierByUserIdentId(Guid identUserId)
        {
            _securityService.OverrideUserIdentId = identUserId;
            _securityService.ResetInit();
            var userAuthorizedScacs = _securityService.GetAuthorizedScasForCarrierByPrimaryScac().Select(cs => cs.Scac).ToArray();

            var bookedLoadsCount = (
                        from l in _context.Loads
                        join lt in _context.LoadTransactions on l.LoadId equals lt.LoadId
                        join lc in _context.LoadClaims on lt.LoadTransactionId equals lc.LoadTransactionId
                        where
                            bookedTransactionTypes.Contains(l.LatestTransactionTypeId)
                            && userAuthorizedScacs.Contains(lc.Scac)
                            && lc.LoadClaimId == (
                                from lt in _context.LoadTransactions
                                join lc in _context.LoadClaims on lt.LoadTransactionId equals lc.LoadTransactionId
                                where lt.LoadId == l.LoadId
                                orderby lc.CreateDtTm descending
                                select lc).FirstOrDefault().LoadClaimId
                        select l.LoadId
                    )
                   .Count();

            //Make sure we reset the security service incase anything needs to execute as the current user
            _securityService.OverrideUserIdentId = null;
            _securityService.ResetInit();

            return bookedLoadsCount;
        }

        public IQueryable<LoadViewData> GetLoadsForCarrierMarketplace(string[] transTypes, string userPrimaryScac)
        {
            return (from l in _context.Loads
                    join cus in _context.Customers on l.CustomerId equals cus.CustomerId
                    join OriginLoadStop in _context.LoadStops.Where(ls => ls.StopNbr == 1) on l.LoadId equals OriginLoadStop.LoadId
                    join DestinationLoadStop in _context.LoadStops on new { l.LoadId, StopNbr = (int)l.Stops } equals new { DestinationLoadStop.LoadId, DestinationLoadStop.StopNbr }
                    join lcs in _context.LoadCarrierScacs on new { l.LoadId, Scac = userPrimaryScac } equals new { lcs.LoadId, lcs.Scac }
                    join cs in _context.CarrierScacs on lcs.Scac equals cs.Scac
                    where transTypes.Contains(l.LatestTransactionTypeId)
                            /*
                            * If the User's PrimaryScac is a Dedicated Scac, then include the load
                            * If the User's PrimaryScac is not dedicated and has no contract rate, then include the load
                            * If the User's PriamryScac is not dedicated and has a contract rate, only include the load 
                            * if the contract rate is less-than-or-equal-to the load's LineHaulRate
                            */
                            && (lcs.ContractRate == null || lcs.ContractRate <= l.LineHaulRate || cs.IsDedicated)
                    select new LoadViewData()
                    {
                        LoadId = l.LoadId,
                        CustomerId = l.CustomerId,
                        ReferenceLoadId = l.ReferenceLoadId,
                        ReferenceLoadDisplay = l.ReferenceLoadDisplay,
                        Stops = l.Stops,
                        Miles = l.Miles,
                        LineHaulRate = cs.IsDedicated ? l.LineHaulRate : lcs.ContractRate ?? l.LineHaulRate,
                        FuelRate = l.FuelRate,
                        Commodity = l.Commodity,
                        Weight = l.Weight,
                        IsHazMat = l.IsHazMat,
                        TransactionTypeId = l.LatestTransactionTypeId,
                        DistanceFromOrig = 0,
                        DistanceFromDest = 0,
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
                        IsEstimatedFSC = FscUtilities.IsEstimatedFsc(cus, (OriginLoadStop.EarlyDtTm ?? OriginLoadStop.LateDtTm), _dateTimeProvider.Now),
                        //This is needed to make sure the quick filters work w / o loading to memory
                        Scac = null,
                        // Load entire entity to prevent EF from doing a subselect
                        LoadServiceTypes = l.LoadServiceTypes
                    })
                .OrderBy(l => l.OriginLateDtTm);
        }

        public IQueryable<LoadViewData> GetLoadsForCarrierMarketplaceAsAdmin(string[] transTypes)
        {
            return (from l in _context.Loads
                    join c in _context.Customers on l.CustomerId equals c.CustomerId
                    join OriginLoadStop in _context.LoadStops.Where(ls => ls.StopNbr == 1) on l.LoadId equals OriginLoadStop.LoadId
                    join DestinationLoadStop in _context.LoadStops on new { l.LoadId, StopNbr = (int)l.Stops } equals new { DestinationLoadStop.LoadId, DestinationLoadStop.StopNbr }
                    where transTypes.Contains(l.LatestTransactionTypeId)
                    select new LoadViewData()
                    {
                        LoadId = l.LoadId,
                        CustomerId = l.CustomerId,
                        ReferenceLoadId = l.ReferenceLoadId,
                        ReferenceLoadDisplay = l.ReferenceLoadDisplay,
                        Stops = l.Stops,
                        Miles = l.Miles,
                        LineHaulRate = l.LineHaulRate,
                        FuelRate = l.FuelRate,
                        Commodity = l.Commodity,
                        Weight = l.Weight,
                        IsHazMat = l.IsHazMat,
                        TransactionTypeId = l.LatestTransactionTypeId,
                        DistanceFromOrig = 0,
                        DistanceFromDest = 0,
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
                        IsEstimatedFSC = FscUtilities.IsEstimatedFsc(c, (OriginLoadStop.EarlyDtTm ?? OriginLoadStop.LateDtTm), _dateTimeProvider.Now),
                        // This is needed to make sure the quick filters work w/o loading to memory
                        Scac = null,
                        // Load entire entity to prevent EF from doing a subselect
                        LoadServiceTypes = l.LoadServiceTypes
                    })
                    .OrderBy(l => l.OriginLateDtTm);
        }

        public IQueryable<LoadViewData> GetLoadsForCarrierWithLoadClaim(string[] transTypes, string[] userAuthorizedScacs)
        {
            return (from l in _context.Loads
                    join c in _context.Customers on l.CustomerId equals c.CustomerId
                    join lt in _context.LoadTransactions on l.LoadId equals lt.LoadId
                    join lc in _context.LoadClaims on lt.LoadTransactionId equals lc.LoadTransactionId
                    join claimUser in _context.Users on lc.UserId equals claimUser.UserId into claimUserJoin
                    from claimUser in claimUserJoin.DefaultIfEmpty()
                    join claimScac in _context.CarrierScacs on lc.Scac equals claimScac.Scac into claimScacJoin
                    from claimScac in claimScacJoin.DefaultIfEmpty()
                    join claimCarrier in _context.Carriers on claimScac.CarrierId equals claimCarrier.CarrierId into claimCarrierJoin
                    from claimCarrier in claimCarrierJoin.DefaultIfEmpty()
                    join contractedCarrierScac in _context.CustomerCarrierScacContracts on new { lc.Scac, l.CustomerId } equals new { contractedCarrierScac.Scac, contractedCarrierScac.CustomerId } into contractedCarrierScacJoin
                    from contractedCarrierScac in contractedCarrierScacJoin.DefaultIfEmpty()
                    join OriginLoadStop in _context.LoadStops.Where(ls => ls.StopNbr == 1) on l.LoadId equals OriginLoadStop.LoadId
                    join DestinationLoadStop in _context.LoadStops on new { l.LoadId, StopNbr = (int)l.Stops } equals new { DestinationLoadStop.LoadId, DestinationLoadStop.StopNbr }
                    where
                        transTypes.Contains(l.LatestTransactionTypeId)
                            &&
                            (
                                userAuthorizedScacs.Contains(lc.Scac)
                                && lc.LoadClaimId == (
                                    from lt in _context.LoadTransactions
                                    join lc in _context.LoadClaims on lt.LoadTransactionId equals lc.LoadTransactionId
                                    where lt.LoadId == l.LoadId
                                    orderby lc.CreateDtTm descending
                                    select lc).FirstOrDefault().LoadClaimId
                            )
                    select new LoadViewData()
                    {
                        LoadId = l.LoadId,
                        CustomerId = l.CustomerId,
                        ReferenceLoadId = l.ReferenceLoadId,
                        ReferenceLoadDisplay = l.ReferenceLoadDisplay,
                        Stops = l.Stops,
                        Miles = l.Miles,
                        LineHaulRate = lc != null ? lc.LineHaulRate : l.LineHaulRate,
                        FuelRate = l.FuelRate,
                        Commodity = l.Commodity,
                        Weight = l.Weight,
                        IsHazMat = l.IsHazMat,
                        TransactionTypeId = l.LatestTransactionTypeId,
                        DistanceFromOrig = 0,
                        DistanceFromDest = 0,
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
                        IsEstimatedFSC = FscUtilities.IsEstimatedFsc(c, (OriginLoadStop.EarlyDtTm ?? OriginLoadStop.LateDtTm), _dateTimeProvider.Now),

                        Scac = lc.Scac,


                        BookedUser = claimUser.Username,
                        BookedUserCarrierName = claimCarrier.CarrierName,
                        BillingLoadId = lc.BillingLoadId,
                        BillingLoadDisplay = lc.BillingLoadDisplay,
                        VisibilityPhoneNumber = lc.VisibilityPhoneNumber,
                        VisibilityTruckNumber = lc.VisibilityTruckNumber,
                        VisibilityChgDtTm = lc.VisibilityChgDtTm,
                        MobileExternallyEntered = lc.MobileExternallyEntered,

                        HasContractedCarrierScac = contractedCarrierScac != null,
                        FeeData = new LoadshopFeeData
                        {
                            LoadshopFlatFee = lc.FlatFee,
                            LoadshopPercentFee = lc.PercentFee,
                            LoadshopFeeAdd = lc.FeeAdd,
                            LoadshopFee = lc.LoadshopFee
                        },
                        // Load entire entity to prevent EF from doing a subselect
                        LoadServiceTypes = l.LoadServiceTypes
                    })
                     .OrderBy(l => l.OriginLateDtTm);
        }

        public bool ShouldShowVisibility(DateTime? visibilityPickupWindowDate, bool topsToGoCarrier, bool p44Carrier, DateTime currentDate, LoadViewData l)
        {
            return
             l.OriginLateDtTm > currentDate
             && l.OriginLateDtTm <= visibilityPickupWindowDate
             && l.DestLateDtTm >= currentDate
             && ((topsToGoCarrier && p44Carrier && l.VisibilityPhoneNumber == null && !l.MobileExternallyEntered && l.VisibilityTruckNumber == null)
             || (topsToGoCarrier && !p44Carrier && l.VisibilityPhoneNumber == null && !l.MobileExternallyEntered)
             || (!topsToGoCarrier && p44Carrier && l.VisibilityTruckNumber == null));
        }

        public virtual List<LoadDetailViewEntity> GetLoadDetailViews(GetLoadDetailOptions options)
        {
            //var loadIdParam = loadId.HasValue && loadId != Guid.Empty ? loadId.ToString() : null;
            //var referenceLoadIdParam = !string.IsNullOrWhiteSpace(referenceLoadId) ? referenceLoadId.Trim() : null;
            //var customerIdentIdParam = customerIdentUserId.HasValue && customerIdentUserId != Guid.Empty ? customerIdentUserId.Value.ToString() : null;
            //var txTypesParam = transactionTypes != null && transactionTypes.Count > 0 ? string.Join(",", transactionTypes) : null;

            //var loads = _context.LoadDetailViews
            //    .FromSql($"EXEC spGetLoadDetailView @LoadId = {loadIdParam}, @ReferenceLoadId = {referenceLoadIdParam}, @CustomerIdentId = {customerIdentIdParam}, @TransactionTypes = {txTypesParam}")
            //    .ToList();

            var loads = (from l in _context.Loads
                         join c in _context.Customers on l.CustomerId equals c.CustomerId
                         join lt in _context.LoadTransactions on l.LatestLoadTransactionId equals lt.LoadTransactionId
                         join lc in _context.LoadClaims on lt.LoadTransactionId equals lc.LoadTransactionId into lcJoin
                         from lc in lcJoin.DefaultIfEmpty()
                         join claimUser in _context.Users on lc.UserId equals claimUser.UserId into claimUserJoin
                         from claimUser in claimUserJoin.DefaultIfEmpty()
                         join claimScac in _context.CarrierScacs on lc.Scac equals claimScac.Scac into claimScacJoin
                         from claimScac in claimScacJoin.DefaultIfEmpty()
                         join claimCarrier in _context.Carriers on claimScac.CarrierId equals claimCarrier.CarrierId into claimCarrierJoin
                         from claimCarrier in claimCarrierJoin.DefaultIfEmpty()
                         join contractedCarrierScac in _context.CustomerCarrierScacContracts on new { lc.Scac, l.CustomerId } equals new { contractedCarrierScac.Scac, contractedCarrierScac.CustomerId } into contractedCarrierScacJoin
                         from contractedCarrierScac in contractedCarrierScacJoin.DefaultIfEmpty()
                             //join lcs in _context.LoadCarrierScacs on new {l.LoadId, Scac = primaryScac } equals new { lcs.LoadId, lcs.Scac } into lcsJoin
                             //from lcs in lcsJoin.DefaultIfEmpty()
                             //join cs in _context.CarrierScacs on lcs.Scac equals cs.Scac into csJoin
                             //from cs in csJoin.DefaultIfEmpty()
                             //join originLoadStop in _context.LoadStops on new { l.LoadId, StopNbr = 1 } equals new { originLoadStop.LoadId, originLoadStop.StopNbr }
                             //join destinationLoadStop in _context.LoadStops on new { l.LoadId, StopNbr = (int)l.Stops } equals new { destinationLoadStop.LoadId, destinationLoadStop.StopNbr }
                         orderby lt.CreateDtTm descending
                         where
                         (options.LoadId == null || options.LoadId == l.LoadId)
                         && (options.CustomerIdentUserId == null || options.CustomerIdentUserId == c.IdentUserId)
                         && (options.ReferenceLoadId == null || options.ReferenceLoadId == l.ReferenceLoadId)
                         && (options.TransactionTypes == null || options.TransactionTypes.Contains(l.LatestTransactionTypeId))
                         select new LoadDetailViewEntity()
                         {
                             LoadId = l.LoadId,
                             CustomerId = l.CustomerId,
                             ReferenceLoadId = l.ReferenceLoadId,
                             ReferenceLoadDisplay = l.ReferenceLoadDisplay,
                             Stops = l.Stops,
                             Miles = l.Miles,
                             LineHaulRate = l.LineHaulRate,
                             FuelRate = l.FuelRate,
                             Commodity = l.Commodity,
                             Cube = l.Cube,
                             Weight = l.Weight,
                             IsHazMat = l.IsHazMat,
                             TransactionTypeId = l.LatestTransactionTypeId,
                             IsAccepted = l.IsAccepted,
                             Comments = l.Comments,
                             CreateDtTm = l.CreateDtTm,
                             CreateBy = l.CreateBy,
                             LastChgDtTm = l.LastChgDtTm,
                             LastChgBy = l.LastChgBy,
                             ScacsSentWithLoad = l.ScacsSentWithLoad,
                             ManuallyCreated = l.ManuallyCreated,
                             TransportationModeId = l.TransportationModeId,
                             ShipperPickupNumber = l.ShipperPickupNumber,
                             PlatformPlusLoadId = l.PlatformPlusLoadId,
                             DATGuardRate = l.DATGuardRate,
                             MachineLearningRate = l.MachineLearningRate,
                             UsesAllInRates = l.UsesAllInRates,
                             //Equipment
                             EquipmentId = l.EquipmentId,
                             EquipmentType = l.Equipment.EquipmentDesc,
                             //EquipmentCategoryId = l.Equipment.CategoryId ?? "Unknown",
                             //EquipmentCategoryDesc = l.Equipment.CategoryEquipmentDesc,
                             //EquipmentTypeDisplay = l.Equipment.CategoryId == null ? l.Equipment.EquipmentDesc : l.Equipment.CategoryEquipmentDesc,

                             //OriginLat = (double)originLoadStop.Latitude,
                             //OriginLng = (double)originLoadStop.Longitude,
                             //OriginCity = originLoadStop.City,
                             //OriginState = originLoadStop.State,
                             //OriginEarlyDtTm = originLoadStop.EarlyDtTm,
                             //OriginLateDtTm = originLoadStop.LateDtTm,
                             //DestLat = (double)destinationLoadStop.Latitude,
                             //DestLng = (double)destinationLoadStop.Longitude,
                             //DestCity = destinationLoadStop.City,
                             //DestState = destinationLoadStop.State,
                             //DestEarlyDtTm = destinationLoadStop.EarlyDtTm,
                             //DestLateDtTm = destinationLoadStop.LateDtTm,
                             //IsEstimatedFSC = FscUtilities.IsEstimatedFsc(c, (originLoadStop.EarlyDtTm ?? originLoadStop.LateDtTm), _dateTimeProvider.Now),

                             Scac = lc.NullPropagate(x => x.Scac),

                             BookedUserId = claimUser.NullPropagate(cu => cu.UserId),
                             BookedUser = claimUser.NullPropagate(cu => cu.Username),
                             BookedUserCarrierName = claimCarrier.NullPropagate(cc => cc.CarrierId),
                             BillingLoadId = lc.NullPropagate(lc => lc.BillingLoadId),
                             BillingLoadDisplay = lc.NullPropagate(lc => lc.BillingLoadDisplay),
                             VisibilityPhoneNumber = lc.NullPropagate(lc => lc.VisibilityPhoneNumber),
                             VisibilityTruckNumber = lc.NullPropagate(lc => lc.VisibilityTruckNumber),
                             VisibilityChgDtTm = lc.NullPropagate(lc => lc.VisibilityChgDtTm),
                             MobileExternallyEntered = lc.NullPropagate(lc => lc.MobileExternallyEntered),
                             ProcessedDtTm = lt.NullPropagate(lt => lt.ProcessedDtTm),
                             LoadTransactionId = lt.NullPropagate(lt => lt.LoadTransactionId),
                             TransactionLineHaulRate = lc.NullPropagate(lc => lc.LineHaulRate),
                             TransactionFuelRate = lc.NullPropagate(lc => lc.FuelRate),
                             UserId = lc.NullPropagate(lc => lc.UserId),
                             TransactionLastUpdateTime = lt.NullPropagate(lt => lt.LastChgDtTm),
                             IsPlatformPlus = lc != null && lc.Scac != null && contractedCarrierScac == null,

                             FlatFee = lc.NullPropagate(lt => lt.FlatFee),
                             PercentFee = lc.NullPropagate(lt => lt.PercentFee),
                             FeeAdd = lc.NullPropagate(lc => lc.FeeAdd),
                             LoadshopFee = lc.NullPropagate(lc => lc.LoadshopFee),
                         })
                         .ToList();

            if (options.IncludeContacts || options.IncludeStops || options.IncludeEquipment)
            {
                var loadIds = loads.Select(x => x.LoadId).Distinct().ToList();
                var loadContactGroups = new List<IGrouping<Guid, LoadContactEntity>>();
                var loadStopGroups = new List<IGrouping<Guid, LoadStopEntity>>();
                var equipmentIds = new List<string>();
                var equipments = new List<EquipmentEntity>();
                var documentsGroups = new List<IGrouping<Guid, LoadDocumentEntity>>();
                var currentStatusGroups = new List<IGrouping<Guid, LoadCurrentStatusEntity>>();
                var loadServiceTypes = new List<IGrouping<Guid, LoadServiceTypeEntity>>();

                if (options.IncludeContacts)
                {
                    loadContactGroups = _context.LoadContacts.Where(x => loadIds.Contains(x.LoadId)).AsEnumerable().GroupBy(x => x.LoadId).ToList();
                }

                if (options.IncludeStops)
                {
                    loadStopGroups = _context.LoadStops.Where(x => loadIds.Contains(x.LoadId)).AsEnumerable().GroupBy(x => x.LoadId).ToList();
                }

                if (options.IncludeEquipment)
                {
                    equipmentIds = loads.Select(x => x.EquipmentId).Distinct().ToList();
                    equipments = _context.Equipment.Where(x => equipmentIds.Contains(x.EquipmentId)).ToList();
                }
                if (options.IncludeDocuments)
                {
                    documentsGroups = _context.LoadDocuments.Where(x => loadIds.Contains(x.LoadId)).AsEnumerable().GroupBy(x => x.LoadId).ToList();
                }

                if (options.IncludeCurrentStatuses)
                {
                    currentStatusGroups = _context.LoadCurrentStatuses.Where(x => loadIds.Contains(x.LoadId)).ToList().GroupBy(x => x.LoadId).ToList();
                }
                if (options.IncludeServiceTypes)
                {
                    loadServiceTypes = _context.LoadServiceTypes.Include(x => x.ServiceType).Where(x => loadIds.Contains(x.LoadId)).ToList().GroupBy(x => x.LoadId).ToList();
                }

                foreach (var load in loads)
                {
                    if (options.IncludeContacts)
                    {
                        var contacts = loadContactGroups.SingleOrDefault(x => x.Key == load.LoadId)?.ToList();
                        load.Contacts = contacts;
                    }
                    if (options.IncludeStops)
                    {
                        var loadStops = loadStopGroups.SingleOrDefault(x => x.Key == load.LoadId)?.ToList();
                        load.LoadStops = loadStops;
                    }
                    if (options.IncludeEquipment)
                    {
                        var equipment = equipments.SingleOrDefault(x => x.EquipmentId == load.EquipmentId);
                        load.Equipment = equipment;
                    }
                    if (options.IncludeDocuments)
                    {
                        load.LoadDocuments = documentsGroups.FirstOrDefault(x => x.Key == load.LoadId)?.ToList();
                    }
                    if (options.IncludeCurrentStatuses)
                    {
                        load.LoadCurrentStatuses = currentStatusGroups.FirstOrDefault(x => x.Key == load.LoadId)?.ToList();
                    }
                    if (options.IncludeServiceTypes)
                    {
                        load.LoadServiceTypes = loadServiceTypes.FirstOrDefault(x => x.Key == load.LoadId)?.ToList();
                    }
                }
            }

            return loads;
        }

        public async Task<List<LoadDetailViewEntity>> GetLoadDetailViewUnprocessedAsync()
        {

            var loads = await (from l in _context.Loads
                               join c in _context.Customers on l.CustomerId equals c.CustomerId
                               join lt in _context.LoadTransactions on l.LatestLoadTransactionId equals lt.LoadTransactionId
                               join lc in _context.LoadClaims on lt.LoadTransactionId equals lc.LoadTransactionId into lcJoin
                               from lc in lcJoin.DefaultIfEmpty()
                               join claimUser in _context.Users on lc.UserId equals claimUser.UserId into claimUserJoin
                               from claimUser in claimUserJoin.DefaultIfEmpty()
                               join claimScac in _context.CarrierScacs on lc.Scac equals claimScac.Scac into claimScacJoin
                               from claimScac in claimScacJoin.DefaultIfEmpty()
                               join claimCarrier in _context.Carriers on claimScac.CarrierId equals claimCarrier.CarrierId into claimCarrierJoin
                               from claimCarrier in claimCarrierJoin.DefaultIfEmpty()
                               join contractedCarrierScac in _context.CustomerCarrierScacContracts on new { lc.Scac, l.CustomerId } equals new { contractedCarrierScac.Scac, contractedCarrierScac.CustomerId } into contractedCarrierScacJoin
                               from contractedCarrierScac in contractedCarrierScacJoin.DefaultIfEmpty()
                                   //join lcs in _context.LoadCarrierScacs on new {l.LoadId, Scac = primaryScac } equals new { lcs.LoadId, lcs.Scac } into lcsJoin
                                   //from lcs in lcsJoin.DefaultIfEmpty()
                                   //join cs in _context.CarrierScacs on lcs.Scac equals cs.Scac into csJoin
                                   //from cs in csJoin.DefaultIfEmpty()
                                   //join originLoadStop in _context.LoadStops on new { l.LoadId, StopNbr = 1 } equals new { originLoadStop.LoadId, originLoadStop.StopNbr }
                                   //join destinationLoadStop in _context.LoadStops on new { l.LoadId, StopNbr = (int)l.Stops } equals new { destinationLoadStop.LoadId, destinationLoadStop.StopNbr }
                               where lt.ProcessedDtTm == null
                               //Sort by LoadTransaction so places that call FirstOrDefault get result with the latest load transaction
                               orderby lt.CreateDtTm descending
                               select new LoadDetailViewEntity()
                               {
                                   LoadId = l.LoadId,
                                   CustomerId = l.CustomerId,
                                   ReferenceLoadId = l.ReferenceLoadId,
                                   ReferenceLoadDisplay = l.ReferenceLoadDisplay,
                                   Stops = l.Stops,
                                   Miles = l.Miles,
                                   LineHaulRate = l.LineHaulRate,
                                   FuelRate = l.FuelRate,
                                   Commodity = l.Commodity,
                                   Cube = l.Cube,
                                   Weight = l.Weight,
                                   IsHazMat = l.IsHazMat,
                                   TransactionTypeId = l.LatestTransactionTypeId,
                                   IsAccepted = l.IsAccepted,
                                   Comments = l.Comments,
                                   CreateDtTm = l.CreateDtTm,
                                   CreateBy = l.CreateBy,
                                   LastChgDtTm = l.LastChgDtTm,
                                   LastChgBy = l.LastChgBy,
                                   ScacsSentWithLoad = l.ScacsSentWithLoad,
                                   ManuallyCreated = l.ManuallyCreated,
                                   TransportationModeId = l.TransportationModeId,
                                   ShipperPickupNumber = l.ShipperPickupNumber,
                                   PlatformPlusLoadId = l.PlatformPlusLoadId,
                                   DATGuardRate = l.DATGuardRate,
                                   MachineLearningRate = l.MachineLearningRate,
                                   UsesAllInRates = l.UsesAllInRates,
                                   //Equipment
                                   EquipmentId = l.EquipmentId,
                                   EquipmentType = l.Equipment.EquipmentDesc,
                                   //EquipmentCategoryId = l.Equipment.CategoryId ?? "Unknown",
                                   //EquipmentCategoryDesc = l.Equipment.CategoryEquipmentDesc,
                                   //EquipmentTypeDisplay = l.Equipment.CategoryId == null ? l.Equipment.EquipmentDesc : l.Equipment.CategoryEquipmentDesc,

                                   //OriginLat = (double)originLoadStop.Latitude,
                                   //OriginLng = (double)originLoadStop.Longitude,
                                   //OriginCity = originLoadStop.City,
                                   //OriginState = originLoadStop.State,
                                   //OriginEarlyDtTm = originLoadStop.EarlyDtTm,
                                   //OriginLateDtTm = originLoadStop.LateDtTm,
                                   //DestLat = (double)destinationLoadStop.Latitude,
                                   //DestLng = (double)destinationLoadStop.Longitude,
                                   //DestCity = destinationLoadStop.City,
                                   //DestState = destinationLoadStop.State,
                                   //DestEarlyDtTm = destinationLoadStop.EarlyDtTm,
                                   //DestLateDtTm = destinationLoadStop.LateDtTm,
                                   //IsEstimatedFSC = FscUtilities.IsEstimatedFsc(c, (originLoadStop.EarlyDtTm ?? originLoadStop.LateDtTm), _dateTimeProvider.Now),

                                   Scac = lc.NullPropagate(lc => lc.Scac),

                                   BookedUserId = claimUser.NullPropagate(cu => cu.UserId),
                                   BookedUser = claimUser.NullPropagate(cu => cu.Username),
                                   BookedUserCarrierName = claimCarrier.NullPropagate(cc => cc.CarrierName),
                                   BillingLoadId = lc.NullPropagate(lc => lc.BillingLoadId),
                                   BillingLoadDisplay = lc.NullPropagate(lc => lc.BillingLoadDisplay),
                                   VisibilityPhoneNumber = lc.NullPropagate(lc => lc.VisibilityPhoneNumber),
                                   VisibilityTruckNumber = lc.NullPropagate(lc => lc.VisibilityTruckNumber),
                                   VisibilityChgDtTm = lc.NullPropagate(lc => lc.VisibilityChgDtTm),
                                   MobileExternallyEntered = lc.NullPropagate(lc => lc.MobileExternallyEntered),
                                   ProcessedDtTm = lt.NullPropagate(lt => lt.ProcessedDtTm),
                                   LoadTransactionId = lt.NullPropagate(lt => lt.LoadTransactionId),
                                   TransactionLineHaulRate = lc.NullPropagate(lc => lc.LineHaulRate),
                                   TransactionFuelRate = lc.NullPropagate(lc => lc.FuelRate),
                                   UserId = lc.NullPropagate(lc => lc.UserId),
                                   TransactionLastUpdateTime = lt.NullPropagate(lt => lt.LastChgDtTm),
                                   IsPlatformPlus = lc != null && lc.Scac != null && contractedCarrierScac == null,

                                   FlatFee = lc.NullPropagate(lt => lt.FlatFee),
                                   PercentFee = lc.NullPropagate(lt => lt.PercentFee),
                                   FeeAdd = lc.NullPropagate(lc => lc.FeeAdd),
                                   LoadshopFee = lc.NullPropagate(lc => lc.LoadshopFee),

                               })
                            .ToListAsync();
            return loads;
        }
    }
}
