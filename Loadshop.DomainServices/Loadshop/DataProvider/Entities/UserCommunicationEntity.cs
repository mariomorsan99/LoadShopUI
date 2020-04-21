using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class UserCommunicationEntity : UserCommunication
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);

            modelBuilder.Entity<UserCommunicationEntity>()
                .HasMany(userCommunication => userCommunication.UserCommunicationAcknowledgements)
                .WithOne(userCommunicationAcknowledgements => userCommunicationAcknowledgements.UserCommunication)
                .HasForeignKey(userCommunicationAcknowledgements => userCommunicationAcknowledgements.UserCommunicationId);

            modelBuilder.Entity<UserCommunicationEntity>()
                .HasMany(userCommunication => userCommunication.UserCommunicationCarriers)
                .WithOne(userCommunicationCarrier => userCommunicationCarrier.UserCommunication)
                .HasForeignKey(userCommunicationCarrier => userCommunicationCarrier.UserCommunicationId);

            modelBuilder.Entity<UserCommunicationEntity>()
                .HasMany(userCommunication => userCommunication.UserCommunicationShippers)
                .WithOne(userCommunicationShipper => userCommunicationShipper.UserCommunication)
                .HasForeignKey(userCommunicationShipper => userCommunicationShipper.UserCommunicationId);

            modelBuilder.Entity<UserCommunicationEntity>()
                 .HasMany(userCommunication => userCommunication.UserCommunicationUsers)
                 .WithOne(userCommunicationUser => userCommunicationUser.UserCommunication)
                 .HasForeignKey(userCommunicationUser => userCommunicationUser.UserCommunicationId);

            modelBuilder.Entity<UserCommunicationEntity>()
                .HasMany(userCommunication => userCommunication.UserCommunicationSecurityAccessRoles)
                .WithOne(userCommunicationSecurityAccessRole => userCommunicationSecurityAccessRole.UserCommunication)
                .HasForeignKey(userCommunicationSecurityAccessRole => userCommunicationSecurityAccessRole.UserCommunicationId);

            modelBuilder.Entity<UserCommunicationEntity>()
                .HasOne(x => x.Owner)
                .WithMany(x => x.UserCommunications)
                .HasForeignKey(x => x.OwnerId);
        }

        public virtual List<UserCommunicationAcknowledgementEntity> UserCommunicationAcknowledgements { get; set; }
        public virtual List<UserCommunicationUserEntity> UserCommunicationUsers { get; set; }
        public virtual List<UserCommunicationCarrierEntity> UserCommunicationCarriers { get; set; }
        public virtual List<UserCommunicationShipperEntity> UserCommunicationShippers { get; set; }
        public virtual List<UserCommunicationSecurityAccessRoleEntity> UserCommunicationSecurityAccessRoles { get; set; }
        public virtual UserEntity Owner { get; set; }
    }
}
