using System.Collections.Generic;
using Loadshop.DomainServices.Common.Services.Data;

namespace Loadshop.DomainServices.Common.Services
{
    public interface ICommonService
    {
        /// <summary>
        /// Gets US and Canada states and provinces
        /// </summary>
        /// <returns></returns>
        List<StateData> GetUSCANStateProvinces();

        /// <summary>
        /// Gets US and Canada state and province
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        StateData GetUSCANStateProvince(string stateName);

        /// <summary>
        /// Gets carrier visibility type for a user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="carrierId"></param>
        /// <returns></returns>
        List<string> GetCarrierVisibilityTypes(string username, string carrierId);

        /// <summary>
        /// Gets cap rates from TOPS
        /// </summary>
        /// <param name="loadId"></param>
        /// <returns></returns>
        CapRateData GetCapRates(string loadId);
    }
}
