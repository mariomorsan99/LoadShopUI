namespace Loadshop.DomainServices.Proxy.Visibility.Models
{
    /// <summary>
    /// Request for Visibility to register
    /// </summary>
    public class VisibilityNotificationRegistrationModel
    {
        public bool IsArrived { get; set; }
        public bool IsDeparted { get; set; }
        public bool IsDelivered { get; set; }
        public bool IsEmail { get; set; }
        public bool IsMobilePush { get; set; }
        public bool IsSMS { get; set; }
        public bool IsWebPush { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Sender { get; set; }
    }
}
