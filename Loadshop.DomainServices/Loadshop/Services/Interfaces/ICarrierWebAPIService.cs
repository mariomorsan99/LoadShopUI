using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Data.CarrierWebAPI;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface ICarrierWebAPIService
    {
        Task<LoadStatusEvent<CarrierApiResponseMessages>> Send<T>(LoadStatusEvent<T> message);
    }
}