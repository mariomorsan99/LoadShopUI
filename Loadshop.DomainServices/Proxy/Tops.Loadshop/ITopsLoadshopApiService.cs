using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.API.Models;
using Loadshop.API.Models.DataModels;
using Loadshop.API.Models.Models;
using Loadshop.API.Models.ViewModels;

namespace Loadshop.DomainServices.Proxy.Tops.Loadshop
{
    public interface ITopsLoadshopApiService
    {
        Task<ResponseMessage<Dictionary<string, List<string>>>> GetSourceSystemOwners(string ownerId);
        Task<ResponseMessage<List<LoadshopShipperMappingModel>>> GetShipperMappings(string ownerId);
        Task<ResponseMessage<LoadshopShipperMappingModel>> CreateShipperMapping(
            LoadshopShipperMappingModel shipperMappingModel);
        Task<ResponseMessage<LoadshopShipperMappingModel>> UpdateShipperMapping(
            LoadshopShipperMappingModel shipperMappingModel);
        Task<ResponseMessage<IdentityUserData>> GetIdentityUser(string username);
        Task<ResponseMessage<IdentityUserData>> CreateCustomerUser(RegisterViewModel newUser);
    }
}