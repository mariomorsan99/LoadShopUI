using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.DomainServices.Common.Services.QueryWrappers;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Dto;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface ILoadService
    {
        // Client UI Methods
        Task<int> CreateLoadAuditLogEntryAsync(Guid id, AuditTypeData auditType);
        PageableQuery<LoadViewData> GetAllOpenLoads(Guid userId);
        PageableQuery<LoadViewData> GetBookedLoads(Guid userId);
        PageableQuery<LoadViewData> GetDeliveredLoads(Guid userId);
        Task<LoadData> GetLoadByIdAsync(Guid id, Guid userId);
        VisibilityBadge GetNumLoadsRequiringVisibilityInfo(Guid userId);
        LoadData PendingAcceptLoad(LoadData loadData, Guid userId);

        // Customer Methods
        LoadDetailData AcceptLoad(LoadDetailData loadData, Guid customerId, string username);
        Task<GenericResponse<LoadDetailData>> CreateLoad(LoadDetailData load, Guid customerId, string username, CreateLoadOptionsDto options, string urnPrefix = "urn:root");
        Task<GenericResponse<LoadDetailData[]>> CreateLoadsWithContinueOnFailure(LoadDetailData[] loadDataList,
            Guid customerId, string username, CreateLoadOptionsDto options);
        /// <summary>
        /// Deletes a load from loadshop, including pending/accepted loads
        /// </summary>
        /// <param name="loadData"></param>
        /// <param name="customerId"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        LoadDetailData DeleteLoad(LoadDetailData loadData, Guid customerId, string username);
        string GenerateReturnURL(LoadDetailData[] loads);
        List<LoadDetailData> GetAllOpenLoadsByCustomerId(Guid customerId);
        List<LoadDetailData> GetAllPendingAcceptedLoads(Guid customerId);
        bool GetAutoPostToLoadshop(Guid customerIdentUserId);
        LoadTransactionEntity GetLatestTransaction(Guid loadId);
        LoadDetailData GetLoadByCustomerReferenceId(string id, Guid customerId);
        List<LoadDetailData> HasPendingLoadshopClaim(string id);
        Task<UpdateLoadResult> PreviewUpdateLoad(LoadDetailData load, Guid customerId, string username, UpdateLoadOptions options);
        List<LoadStopData> SetApptTypeOnLoadStops(List<LoadStopData> stops);
        Task<GenericResponse<LoadDetailData>> UpdateLoad(LoadDetailData load, Guid customerId, string username, UpdateLoadOptions options);
        bool UpdateFuel(LoadUpdateFuelData loadFuel, Guid customerId, string username);
        /// <summary>
        /// Updates scacs and contract rates for a load
        /// </summary>
        /// <param name="loadScacs"></param>
        /// <param name="customerId"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        LoadUpdateScacData UpdateScacs(LoadUpdateScacData loadScacs, Guid customerId, string username);
    }
}
