using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface ICarrierService
    {
        bool IsActiveCarrier(string carrierId);
        bool IsPlanningEligible(string scac);
        Task<IReadOnlyCollection<CarrierCarrierScacGroupData>> GetAllCarrierScacsAsync();
    }
}