using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Loadshop.DomainServices.Utilities
{
    public static class QueryFilters
    {
        public static Expression<Func<CarrierScacEntity, bool>> GetActiveCarrierScacFilter(DateTime? filterDate = null)
        {
            filterDate = filterDate ?? DateTime.Today;

            return carrierScac =>
                    carrierScac.Carrier.IsLoadshopActive
                    && carrierScac.IsActive
                    && carrierScac.IsBookingEligible
                    && filterDate >= carrierScac.EffectiveDate
                    && (carrierScac.ExpirationDate == null || filterDate <= carrierScac.ExpirationDate);
        }

        public static Func<CarrierScacEntity, bool> GetActiveCarrierScacFilterAsFunc()
        {
            var filterDate = DateTime.Today;

            return carrierScac =>
                    carrierScac.Carrier.IsLoadshopActive
                    && carrierScac.IsActive
                    && carrierScac.IsBookingEligible
                    && filterDate >= carrierScac.EffectiveDate
                    && (carrierScac.ExpirationDate == null || filterDate <= carrierScac.ExpirationDate);
        }

        public static Func<CarrierScacData, bool> GetActiveCarrierScacDataFilter(DateTime? filterDate = null)
        {
            filterDate = filterDate ?? DateTime.Today;

            return carrierScac =>
                    carrierScac.IsActive
                    && carrierScac.IsBookingEligible
                    && filterDate >= carrierScac.EffectiveDate
                    && (carrierScac.ExpirationDate == null || filterDate <= carrierScac.ExpirationDate);
        }

        public static Expression<Func<LoadViewData, bool>> GetCarrierLoadFilter(LoadSearchCriteria loadSearchCriteria)
        {

            var originStartDate = loadSearchCriteria.OrigDateStart;
            var originEndDate = loadSearchCriteria.OrigDateEnd.HasValue ? loadSearchCriteria.OrigDateEnd.Value.AddDays(1) : originStartDate?.AddDays(1);

            var destStartDate = loadSearchCriteria.DestDateStart;
            var destEndDate = loadSearchCriteria.DestDateStart.HasValue ? loadSearchCriteria.DestDateEnd.Value.AddDays(1) : destStartDate?.AddDays(1);

            var equipmentIds = loadSearchCriteria.EquipmentType != null ? Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(loadSearchCriteria.EquipmentType) : new List<string>();

            var serviceTypeIds = loadSearchCriteria.ServiceTypes != null ? loadSearchCriteria.ServiceTypes.ToList() : new List<int>();

            Guid quickSearchGuid;
            var quickSearchIsGuid = Guid.TryParse(loadSearchCriteria.QuickSearch, out quickSearchGuid);

            return x =>
            (
                string.IsNullOrWhiteSpace(loadSearchCriteria.QuickSearch) ||
                (
                    x.ReferenceLoadDisplay.Contains(loadSearchCriteria.QuickSearch)
                    || x.ReferenceLoadId.Contains(loadSearchCriteria.QuickSearch)
                    || (quickSearchIsGuid && x.LoadId == quickSearchGuid)
                    || x.EquipmentCategoryDesc.Contains(loadSearchCriteria.QuickSearch)
                    || x.EquipmentCategoryId.Contains(loadSearchCriteria.QuickSearch)
                    || x.EquipmentId.Contains(loadSearchCriteria.QuickSearch)
                    || x.EquipmentType.Contains(loadSearchCriteria.QuickSearch)
                    || x.OriginCity.Contains(loadSearchCriteria.QuickSearch)
                    || x.DestCity.Contains(loadSearchCriteria.QuickSearch)
                    || x.OriginState.Contains(loadSearchCriteria.QuickSearch)
                    || x.DestState.Contains(loadSearchCriteria.QuickSearch)
                    || (x.DestCity + ", " + x.DestState).Contains(loadSearchCriteria.QuickSearch)
                    || (x.OriginCity + ", " + x.OriginState).Contains(loadSearchCriteria.QuickSearch)
                    || x.Scac.Contains(loadSearchCriteria.QuickSearch)
                )
            )
            &&
            (!loadSearchCriteria.OrigDateStart.HasValue
                || ((x.OriginEarlyDtTm >= loadSearchCriteria.OrigDateStart && x.OriginEarlyDtTm <= originEndDate)
                    || (x.OriginLateDtTm >= loadSearchCriteria.OrigDateStart && x.OriginLateDtTm <= originEndDate))
            )
            &&
            (!loadSearchCriteria.DestDateStart.HasValue
                || ((x.DestEarlyDtTm >= loadSearchCriteria.DestDateStart && x.DestEarlyDtTm <= destEndDate)
                    || (x.DestLateDtTm >= loadSearchCriteria.DestDateStart && x.DestLateDtTm <= destEndDate))
            )
            &&
            (!equipmentIds.Any() || equipmentIds.Contains(x.EquipmentId))
            &&
            (
              !loadSearchCriteria.OrigLat.HasValue
            || (
                7926 * Math.Asin(Math.Sqrt(Math.Sin(((Math.PI / 180)
                * (x.OriginLat - loadSearchCriteria.OrigLat.Value)) / 2)
                * Math.Sin(((Math.PI / 180) * (x.OriginLat - loadSearchCriteria.OrigLat.Value)) / 2)
                + Math.Cos((Math.PI / 180) * loadSearchCriteria.OrigLat.Value) * Math.Cos((Math.PI / 180)
                * (x.OriginLat))
                * Math.Sin(((Math.PI / 180) * (x.OriginLng - loadSearchCriteria.OrigLng.Value)) / 2)
                * Math.Sin(((Math.PI / 180) * (x.OriginLng - loadSearchCriteria.OrigLng.Value)) / 2)))
                ) <= loadSearchCriteria.OrigDH
            )
            &&
            (
              !loadSearchCriteria.DestLat.HasValue
            || (
                7926 * Math.Asin(Math.Sqrt(Math.Sin(((Math.PI / 180)
                * (x.DestLat - loadSearchCriteria.DestLat.Value)) / 2)
                * Math.Sin(((Math.PI / 180) * (x.DestLat - loadSearchCriteria.DestLat.Value)) / 2)
                + Math.Cos((Math.PI / 180) * loadSearchCriteria.DestLat.Value) * Math.Cos((Math.PI / 180)
                * (x.OriginLat))
                * Math.Sin(((Math.PI / 180) * (x.DestLng - loadSearchCriteria.DestLng.Value)) / 2)
                * Math.Sin(((Math.PI / 180) * (x.DestLng - loadSearchCriteria.DestLng.Value)) / 2)))
                ) <= loadSearchCriteria.DestDH
            )
            && (loadSearchCriteria.OrigState == null || x.OriginState == loadSearchCriteria.OrigState)
            && (loadSearchCriteria.DestState == null || x.DestState == loadSearchCriteria.DestState)
            && (!serviceTypeIds.Any() || x.LoadServiceTypes.Any(y=> serviceTypeIds.Contains(y.ServiceTypeId)));
        }

        public static Expression<Func<ShippingLoadViewData, bool>> GetShippingLoadFilter(LoadSearchCriteria loadSearchCriteria)
        {

            var originStartDate = loadSearchCriteria.OrigDateStart;
            var originEndDate = loadSearchCriteria.OrigDateEnd.HasValue ? loadSearchCriteria.OrigDateEnd.Value.AddDays(1) : originStartDate?.AddDays(1);

            var destStartDate = loadSearchCriteria.DestDateStart;
            var destEndDate = loadSearchCriteria.DestDateStart.HasValue ? loadSearchCriteria.DestDateEnd.Value.AddDays(1) : destStartDate?.AddDays(1);

            var equipmentIds = loadSearchCriteria.EquipmentType != null ? JsonConvert.DeserializeObject<List<string>>(loadSearchCriteria.EquipmentType) : new List<string>();

            var serviceTypeIds = loadSearchCriteria.ServiceTypes != null ? loadSearchCriteria.ServiceTypes.ToList() : new List<int>();

            Guid quickSearchGuid;
            var quickSearchIsGuid = Guid.TryParse(loadSearchCriteria.QuickSearch, out quickSearchGuid);

            return x =>
            (
                string.IsNullOrWhiteSpace(loadSearchCriteria.QuickSearch) ||
                (
                    x.ReferenceLoadDisplay.Contains(loadSearchCriteria.QuickSearch)
                    || x.ReferenceLoadId.Contains(loadSearchCriteria.QuickSearch)
                    || (quickSearchIsGuid && x.LoadId == quickSearchGuid)
                    || x.EquipmentCategoryDesc.Contains(loadSearchCriteria.QuickSearch)
                    || x.EquipmentCategoryId.Contains(loadSearchCriteria.QuickSearch)
                    || x.EquipmentId.Contains(loadSearchCriteria.QuickSearch)
                    || x.EquipmentType.Contains(loadSearchCriteria.QuickSearch)
                    || x.OriginCity.Contains(loadSearchCriteria.QuickSearch)
                    || x.DestCity.Contains(loadSearchCriteria.QuickSearch)
                    || x.OriginState.Contains(loadSearchCriteria.QuickSearch)
                    || x.DestState.Contains(loadSearchCriteria.QuickSearch)
                    || (x.DestCity + ", " + x.DestState).Contains(loadSearchCriteria.QuickSearch)
                    || (x.OriginCity + ", " + x.OriginState).Contains(loadSearchCriteria.QuickSearch)
                    || x.Scac.Contains(loadSearchCriteria.QuickSearch)
                )
            )
            &&
            (!loadSearchCriteria.OrigDateStart.HasValue
                || ((x.OriginEarlyDtTm >= loadSearchCriteria.OrigDateStart && x.OriginEarlyDtTm <= originEndDate)
                    || (x.OriginLateDtTm >= loadSearchCriteria.OrigDateStart && x.OriginLateDtTm <= originEndDate))
            )
            &&
            (!loadSearchCriteria.DestDateStart.HasValue
                || ((x.DestEarlyDtTm >= loadSearchCriteria.DestDateStart && x.DestEarlyDtTm <= destEndDate)
                    || (x.DestLateDtTm >= loadSearchCriteria.DestDateStart && x.DestLateDtTm <= destEndDate))
            )
            &&
            (!equipmentIds.Any() || equipmentIds.Contains(x.EquipmentId))
            &&
            (
              !loadSearchCriteria.OrigLat.HasValue
            || (
                7926 * Math.Asin(Math.Sqrt(Math.Sin(((Math.PI / 180)
                * (x.OriginLat - loadSearchCriteria.OrigLat.Value)) / 2)
                * Math.Sin(((Math.PI / 180) * (x.OriginLat - loadSearchCriteria.OrigLat.Value)) / 2)
                + Math.Cos((Math.PI / 180) * loadSearchCriteria.OrigLat.Value) * Math.Cos((Math.PI / 180)
                * (x.OriginLat))
                * Math.Sin(((Math.PI / 180) * (x.OriginLng - loadSearchCriteria.OrigLng.Value)) / 2)
                * Math.Sin(((Math.PI / 180) * (x.OriginLng - loadSearchCriteria.OrigLng.Value)) / 2)))
                ) <= loadSearchCriteria.OrigDH
            )
            &&
            (
              !loadSearchCriteria.DestLat.HasValue
            || (
                7926 * Math.Asin(Math.Sqrt(Math.Sin(((Math.PI / 180)
                * (x.DestLat - loadSearchCriteria.DestLat.Value)) / 2)
                * Math.Sin(((Math.PI / 180) * (x.DestLat - loadSearchCriteria.DestLat.Value)) / 2)
                + Math.Cos((Math.PI / 180) * loadSearchCriteria.DestLat.Value) * Math.Cos((Math.PI / 180)
                * (x.OriginLat))
                * Math.Sin(((Math.PI / 180) * (x.DestLng - loadSearchCriteria.DestLng.Value)) / 2)
                * Math.Sin(((Math.PI / 180) * (x.DestLng - loadSearchCriteria.DestLng.Value)) / 2)))
                ) <= loadSearchCriteria.DestDH
            )
            && (loadSearchCriteria.OrigState == null || x.OriginState == loadSearchCriteria.OrigState)
            && (loadSearchCriteria.DestState == null || x.DestState == loadSearchCriteria.DestState)
            && (!serviceTypeIds.Any() || x.LoadServiceTypes.Any(y => serviceTypeIds.Contains(y.ServiceTypeId)));
        }
    }
}
