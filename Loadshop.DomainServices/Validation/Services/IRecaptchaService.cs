using Loadshop.DomainServices.Loadshop.Services.Data;
using System.Threading.Tasks;

namespace Loadshop.DomainServices.Validation.Services
{
    public interface IRecaptchaService
    {
        Task ValidateToken<T>(RecaptchaRequest<T> request);
    }
}
