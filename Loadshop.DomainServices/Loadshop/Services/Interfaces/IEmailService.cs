using Loadshop.DomainServices.Loadshop.DataProvider.Entities;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface IEmailService
    {
        void SendMailMessage(NotificationMessageEntity notificationMessage, bool addBcc);
    }
}