using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.API.Models.DataModels;
using Loadshop.DomainServices.Common.Services.Crud;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface IUserAdminService : ICrudService<UserData>
    {
        Task<List<CarrierScacData>> GetAuthorizedCarrierScacs();
        Task<List<SecurityAccessRoleListData>> GetAvailableSecurityRoles(Guid identUserId);
        Task<CrudResult<IEnumerable<TListData>>> GetCollection<TListData>(string query, int? take = null, int? skip = null);
        Task<IReadOnlyCollection<UserData>> GetAllAdminUsers();
        Task<List<CustomerData>> GetAuthorizedCustomers();
        Task<IdentityUserData> GetIdentityUserForCreate(string userName);
        /// <summary>
        /// Get all users associated with the given Carrier Id
        /// </summary>
        /// <param name="carrierId"></param>
        /// <returns></returns>
        Task<List<UserData>> GetCarrierUsersAsync(string carrierId, bool excludeAdminUsers = true);
        /// <summary>
        /// Get just the names and IDs for all the current user's authorized carriers
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyCollection<CarrierData>> GetAllMyAuthorizedCarriersAsync();
    }
}
