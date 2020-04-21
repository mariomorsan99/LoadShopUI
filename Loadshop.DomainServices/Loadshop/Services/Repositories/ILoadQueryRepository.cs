using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Loadshop.DomainServices.Loadshop.Services.Repositories
{
    public interface ILoadQueryRepository
    {
        List<LoadDetailViewEntity> GetLoadDetailViews(GetLoadDetailOptions options);
        Task<List<LoadDetailViewEntity>> GetLoadDetailViewUnprocessedAsync();
        IQueryable<LoadViewData> GetLoadsForCarrierMarketplace(string[] transTypes, string userPrimaryScac);
        IQueryable<LoadViewData> GetLoadsForCarrierMarketplaceAsAdmin(string[] transTypes);
        IQueryable<LoadViewData> GetLoadsForCarrierWithLoadClaim(string[] transTypes, string[] userAuthorizedScacs);
        int GetNumberOfBookedLoadsForCarrierByUserIdentId(Guid identUserId);
        bool ShouldShowVisibility(DateTime? visibilityPickupWindowDate, bool topsToGoCarrier, bool p44Carrier, DateTime currentDate, LoadViewData l);
    }
}