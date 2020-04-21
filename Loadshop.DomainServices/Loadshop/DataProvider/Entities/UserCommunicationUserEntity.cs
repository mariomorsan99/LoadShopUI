using Microsoft.EntityFrameworkCore;
using System;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class UserCommunicationUserEntity : UserCommunicationUser
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<UserCommunicationUserEntity>()
                .HasOne(userCommunicationUser => userCommunicationUser.UserCommunication)
                .WithMany(userCommunication => userCommunication.UserCommunicationUsers)
                .HasForeignKey(userCommunicationUser => userCommunicationUser.UserCommunicationId);

            modelBuilder.Entity<UserCommunicationUserEntity>()
                .HasOne(userCommunicationUser => userCommunicationUser.User)
                .WithMany(user => user.UserCommunicationUsers)
                .HasForeignKey(userCommunicationUser => userCommunicationUser.UserId);
        }

        public virtual UserCommunicationEntity UserCommunication { get; set; }
        public virtual UserEntity User { get; set; }
    }
}
