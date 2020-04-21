using Loadshop.DomainServices.Loadshop.Services.Data;
using System.Threading.Tasks;

namespace Loadshop.DomainServices.Proxy.Visibility.Interfaces
{
    public interface IVisibilityProxyService
    {
        Task<LoadStatusNotificationsData> GetLoadStatusNotificationsAsync();
        Task<SaveLoadStatusNotificationsResponse> UpdateLoadStatusNotificationsAsync(LoadStatusNotificationsData notificationsData);
    }
}
