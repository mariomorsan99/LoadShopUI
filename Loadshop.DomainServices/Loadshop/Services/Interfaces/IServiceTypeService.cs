using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface IServiceTypeService
    {
        /// <summary>
        /// Gets all service types
        /// </summary>
        /// <returns></returns>
        List<ServiceTypeData> GetServiceTypes();
    }
}
