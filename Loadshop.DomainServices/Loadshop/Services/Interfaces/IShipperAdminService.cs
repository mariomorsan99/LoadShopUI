using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.API.Models;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface IShipperAdminService
    {
        List<CustomerData> GetAllShippers();

        #region Maintain Shipper
        CustomerProfileData GetShipper(Guid customerId);
        CustomerProfileData AddShipper(CustomerProfileData customer, string username);
        CustomerProfileData UpdateShipper(CustomerProfileData customer, string username);
        #endregion  Maintain Shipper

        #region Maintain Shipper/Carrier Associations

        List<CarrierScacData> GetAvailableCarriersForShipper(Guid customerId);
        List<CarrierScacData> GetCarriersForShipper(Guid customerId);
        CustomerCarrierScacContractData AddCarrierToShipper(Guid customerId, string scac, Guid userId);
        List<CustomerCarrierScacContractData> AddCarriersToShipper(Guid customerId, List<string> scacs, Guid userId);
        bool DeleteCarrierFromShipper(Guid customerId, Guid CustomerCarrierContractId);
        bool DeleteCarriersFromShipper(Guid customerId, List<Guid> CustomerCarrierContractIds);
        bool DeleteAllCarriersFromShipper(Guid customerId);

        #endregion Maintain Shipper/Carrier Associations


        #region Maintain Shipper/User Associations

        List<UserProfileData> GetUsersForShipper(Guid customerId);
        List<UserProfileData> GetAvailableUsersForShipper(Guid customerId);
        UserShipperData AddUserToShipper(Guid customerId, Guid userId, Guid identUserId);
        List<UserShipperData> AddUsersToShipper(Guid customerId, List<Guid> userIds, Guid identUserId);
        bool DeleteUserFromShipper(Guid customerId, Guid userId);
        bool DeleteUsersFromShipper(Guid customerId, List<Guid> userIds);
        bool DeleteAllUsersFromShipper(Guid customerId);

        #endregion Maintain Shipper/User Associations

        #region Maintain Shipper Mappings

        Task<ResponseMessage<CustomerProfileData>> CreateCustomerUser(Guid customerId, string username);

        #endregion
    }
}
