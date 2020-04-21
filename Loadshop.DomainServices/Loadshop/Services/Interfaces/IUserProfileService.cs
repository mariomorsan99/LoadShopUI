using System;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface IUserProfileService
    {
        /// <summary>
        /// Gets user profile
        /// </summary>
        /// <param name="identUserId"></param>
        /// <param name="carrierId"></param>
        /// <returns></returns>
        Task<UserProfileData> GetUserProfileAsync(Guid identUserId);

        /// <summary>
        /// Saves user profile
        /// </summary>
        /// <param name="userProfile"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<SaveUserProfileResponse> SaveUserProfileAsync(UserProfileData userProfile, string username);

        /// <summary>
        /// Updates user data
        /// </summary>
        /// <param name="identUserId"></param>
        /// <param name="username"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        void UpdateUserData(Guid identUserId, string username, string firstName, string lastName);

        /// <summary>
        /// Creates user profile
        /// </summary>
        /// <param name="identUserId"></param>
        /// <param name="carrierId"></param>
        /// <param name="email"></param>
        /// <param name="username"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns></returns>
        Task<UserProfileData> CreateUserProfileAsync(Guid identUserId, string carrierId, string email, string username, string firstName = null, string lastName = null);

        /// <summary>
        /// Checks if user is a view only user
        /// </summary>
        /// <param name="identUserId"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        Task<bool> IsViewOnlyUserAsync(Guid identUserId);

        /// <summary>
        /// Sets the Focus Primary Customer or Shipper for the User
        /// </summary>
        /// <param name="identUserId"></param>
        /// <param name="userFocusEntityData"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<UserProfileData> UpdateFocusEntityAsync(Guid identUserId, UserFocusEntityData userFocusEntityData, string userName);

        Task<string> GetPrimaryCustomerOwner(Guid identUserId);
        Task<Guid?> GetPrimaryCustomerId(Guid identUserId);
        Task<Guid?> GetPrimaryCustomerIdentUserId(Guid identUserId);
    }
}
