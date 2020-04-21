using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface ISecurityService
    {
        Guid? OverrideUserIdentId { get; set; }
        IReadOnlyCollection<SecurityAccessRoleData> GetUserRoles();
        Task<IReadOnlyCollection<SecurityAccessRoleData>> GetUserRolesAsync();
        bool UserHasAction(params string[] actionNames);
        Task<bool> UserHasActionAsync(params string[] actionName);
        bool UserHasRole(params Guid[] roleIds);
        bool UserHasRole(params string[] roleNames);
        Task<bool> UserHasRoleAsync(params Guid[] roleIds);
        Task<bool> UserHasRoleAsync(params string[] roleNames);
        Task<IReadOnlyCollection<CarrierScacData>> GetCurrentUserAuthorizedScacsForCarrierAsync(string carrierId);
        IReadOnlyCollection<CarrierScacData> GetAuthorizedScacsForCarrier(string carrierId);
        Task<IReadOnlyCollection<CustomerData>> GetAuthorizedCustomersforUserAsync();
        IReadOnlyCollection<CustomerData> GetAuthorizedCustomersforUser();
        Task<IReadOnlyCollection<CarrierScacData>> GetAuthorizedScasForCarrierByPrimaryScacAsync();
        IReadOnlyCollection<CarrierScacData> GetAuthorizedScasForCarrierByPrimaryScac();
        Task<IReadOnlyCollection<CustomerData>> GetAuthorizedCustomersByScacAync();
        IReadOnlyCollection<CustomerData> GetAuthorizedCustomersByScac();
        Task<IReadOnlyCollection<CarrierScacData>> GetCustomerContractedScacsByPrimaryCustomerAsync();
        IReadOnlyCollection<CarrierScacData> GetCustomerContractedScacsByPrimaryCustomer();
        Task<IReadOnlyCollection<CarrierData>> GetContractedCarriersByPrimaryCustomerIdAsync();
        IReadOnlyCollection<CarrierData> GetContractedCarriersByPrimaryCustomerId();
        Task<IReadOnlyCollection<UserFocusEntityData>> GetAllMyAuthorizedEntitiesAsync();
        IReadOnlyCollection<UserFocusEntityData> GetAllMyAuthorizedEntities();
        void ResetInit();
        Task<IReadOnlyCollection<CarrierData>> GetAllMyAuthorizedCarriersAsync();
        IReadOnlyCollection<CarrierData> GetAllMyAuthorizedCarriers();
        Task<IReadOnlyCollection<CarrierScacData>> GetAuthorizedScacsForCarrierAsync(string carrierId, Guid userId);
    }
}