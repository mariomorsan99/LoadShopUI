using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationDetailsAsync();
        Task CreateNotificationsAsync();
        NotificationMessageEntity SendPendingEmail(LoadEntity load, UserEntity user, LoadContactEntity contact, string userContextEmail, LoadClaimEntity claim);

        /// <summary>
        /// Sends a fuel update email to user
        /// </summary>
        NotificationMessageEntity SendFuelUpdateEmail(LoadEntity load, UserEntity user, LoadContactEntity contact, decimal oldFuelRate, LoadClaimEntity loadClaim);
        NotificationMessageEntity SendCarrierRemovedEmail(LoadEntity load, UserEntity user, IEnumerable<LoadContactEntity> contacts, string userContextEmail, string reason);
        NotificationMessageEntity SendShipperFeeChangeEmail(CustomerEntity original, CustomerProfileData update, string changeUser);
    }
}