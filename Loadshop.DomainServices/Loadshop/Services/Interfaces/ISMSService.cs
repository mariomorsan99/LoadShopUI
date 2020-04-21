using System.Threading.Tasks;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface ISMSService
    {
        Task<bool> ValidateNumber(string phoneNumber);
        Task<bool> SendMessage(string phoneNumber, string message);
    }
}