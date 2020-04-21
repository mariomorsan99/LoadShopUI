using Microsoft.EntityFrameworkCore;
using System;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class UserCommunicationAcknowledgementEntity : UserCommunicationAcknowledgement
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<UserCommunicationAcknowledgementEntity>()
                .HasOne(userCommunicationAcknowledgement => userCommunicationAcknowledgement.UserCommunication)
                .WithMany(userCommunication => userCommunication.UserCommunicationAcknowledgements)
                .HasForeignKey(userCommunicationAcknowledgement => userCommunicationAcknowledgement.UserCommunicationId);

            modelBuilder.Entity<UserCommunicationAcknowledgementEntity>()
                .HasOne(userCommunicationAcknowledgement => userCommunicationAcknowledgement.User)
                .WithMany(user => user.UserCommunicationAcknowledgements)
                .HasForeignKey(userCommunicationAcknowledgement => userCommunicationAcknowledgement.UserId);
        }

        public virtual UserCommunicationEntity UserCommunication { get; set; }
        public virtual UserEntity User { get; set; }
    }
}
