using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface ITransportationService
    {
        /// <summary>
        /// Gets all transportation modes
        /// </summary>
        /// <returns></returns>
        List<TransportationModeData> GetTransportationModes();
    }
}
