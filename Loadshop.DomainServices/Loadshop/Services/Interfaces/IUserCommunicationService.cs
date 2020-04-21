using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.DomainServices.Common.Services.Crud;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface IUserCommunicationService : ICrudService<UserCommunicationDetailData>
    {
        Task<CrudResult<List<UserCommunicationData>>> Acknowledge(Guid identUserId, Dto.AcknowledgeUserCommunication acknowledgeUserCommunication);
        Task<CrudResult<List<UserCommunicationData>>> GetUserCommunicationsForDisplay(Guid identUserId);
    }
}
