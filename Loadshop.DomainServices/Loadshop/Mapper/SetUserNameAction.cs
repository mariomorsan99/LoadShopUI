using AutoMapper;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Security;

namespace Loadshop.DomainServices.Loadshop.Mapper
{
    public class SetUserNameAction : IMappingAction<UserEntity, UserProfileData>
    {
        private readonly IUserContext userContext;

        /*
        //unit tests need this to run
        public SetUserNameAction()
        {
        }
        */

        public SetUserNameAction()
        {
        }

        public SetUserNameAction(IUserContext userContext)
        {
            this.userContext = userContext;
        }

        public void Process(UserEntity source, UserProfileData destination)
        {
            destination.Name = $"{userContext?.FirstName} {userContext?.LastName}";
        }
    }
}
