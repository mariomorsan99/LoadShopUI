using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface ISmartSpotPriceService
    {
        Task<SmartSpotPrice> GetSmartSpotPriceAsync(LoadEntity load);
        Task<List<SmartSpotPrice>> GetSmartSpotPricesAsync(List<LoadshopSmartSpotPriceRequest> requests);
        Task<GenericResponse<decimal?>> GetSmartSpotQuoteAsync(RecaptchaRequest<LoadshopSmartSpotQuoteRequest> request);
    }
}
