using System;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface ILoadStatusTransactionService
    {
        Task<LoadStatusTransactionData> GetLatestStatus(Guid loadId);
        Task<BaseServiceResponse> AddInTransitStatus(LoadStatusInTransitData locationData);
        Task<BaseServiceResponse> AddStopStatuses(LoadStatusStopData stopData);
    }
}