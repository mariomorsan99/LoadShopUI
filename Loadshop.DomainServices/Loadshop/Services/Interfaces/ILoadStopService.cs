using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface ILoadStopService
    {
        /// <summary>
        /// Gets all stop types
        /// </summary>
        /// <returns></returns>
        List<StopTypeData> GetStopTypes();

        /// <summary>
        /// Gets all appointment scheduler confirmation types
        /// </summary>
        /// <returns></returns>
        List<AppointmentSchedulerConfirmationTypeData> GetAppointmentSchedulerConfirmationTypes();
    }
}
