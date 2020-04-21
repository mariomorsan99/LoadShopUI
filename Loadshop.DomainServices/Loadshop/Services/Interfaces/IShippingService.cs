using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.DomainServices.Common.Services.QueryWrappers;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface IShippingService
    {
        ShippingLoadData GetLoad(Guid loadId);
        OrderEntryLoadDetailData GetLoadDetailById(Guid loadId);
        OrderEntryLoadDetailData GetDefaultLoadDetail(Guid identUserId);
        //List<ShippingLoadViewData> GetLoadsBySearchType(ShipperSearchTypeData searchType);
        List<ShippingLoadData> GetLoadsForHomeTab(Guid identUserId);
        Task<ShippingLoadData> RemoveLoad(Guid loadId, string username);
        Task<ShippingLoadData> DeleteLoad(Guid loadId, string username, RatingQuestionAnswerData ratingQuestionAnswer = null);
        List<LoadAuditLogData> GetLoadAuditLogs(Guid loadId);
        List<LoadCarrierScacData> GetLoadCarrierScacs(Guid loadId);
        List<LoadCarrierScacRestrictionData> GetLoadCarrierScacRestrictions(Guid loadId);
        Task<PostLoadsResponse> PostLoadsAsync(PostLoadsRequest cmd);
        Task<ShippingLoadData> RemoveCarrierFromLoad(Guid loadId, string username, RatingQuestionAnswerData ratingQuestionAnswer);
        PageableQuery<ShippingLoadViewData> GetLoadsBySearchType(ShipperSearchTypeData searchType, DateTime? visibilityPickupWindowDate = null, bool topsToGoCarrier = false, bool p44Carrier = false);
    }
}
