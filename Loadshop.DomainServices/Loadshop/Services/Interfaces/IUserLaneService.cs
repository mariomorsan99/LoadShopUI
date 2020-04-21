using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface IUserLaneService
    {
        Task<List<UserLaneData>> GetSavedLanesAsync(Guid userId);
        Task<UserLaneData> UpdateLaneAsync(UserLaneData lane, Guid identUserId, string username);
        Task<UserLaneData> CreateLaneAsync(UserLaneData lane, Guid identUserId, string username);
        Task DeleteLaneAsync(Guid userLaneId);
    }
}
