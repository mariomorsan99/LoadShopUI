namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class SaveUserProfileResponse : BaseServiceResponse
    {
        public UserProfileData UserProfile { get; set; }
    }
}