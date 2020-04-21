using System;
using System.Collections.Generic;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class UserEntity : User
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<UserEntity>()
                .HasMany(x => x.UserNotifications)
                .WithOne(x => x.User);

            modelBuilder.Entity<UserEntity>()
                .HasMany(user => user.SecurityUserAccessRoles)
                .WithOne(securityUserAccessRole => securityUserAccessRole.User);

            modelBuilder.Entity<UserEntity>()
                .HasMany(user => user.UserCarrierScacs)
                .WithOne(userCarrierScac => userCarrierScac.User)
                .HasForeignKey(userCarrierScac => userCarrierScac.UserId);

            modelBuilder.Entity<UserEntity>()
                .HasOne(user => user.PrimaryScacEntity)
                .WithMany(carrierScac => carrierScac.Users)
                .HasForeignKey(user => user.PrimaryScac);

            modelBuilder.Entity<UserEntity>()
                .HasMany(user => user.UserShippers)
                .WithOne(userShipper => userShipper.User)
                .HasForeignKey(userShipper => userShipper.UserId);

            modelBuilder.Entity<UserEntity>()
                .HasOne(user => user.PrimaryCustomer)
                .WithMany(customer => customer.PrimaryUsers)
                .HasForeignKey(user => user.PrimaryCustomerId);

            modelBuilder.Entity<UserEntity>()
                .HasMany(user => user.SuccessManagerCustomers)
                .WithOne(customer => customer.SuccessManager)
                .HasForeignKey(customer => customer.SuccessManagerUserId);

            modelBuilder.Entity<UserEntity>()
                .HasMany(user => user.SuccessSpecialistCustomers)
                .WithOne(customer => customer.SuccessSpecialist)
                .HasForeignKey(customer => customer.SuccessSpecialistUserId);

            modelBuilder.Entity<UserEntity>()
                .HasMany(user => user.CarrierSuccessSpecialistCarriers)
                .WithOne(carrier => carrier.CarrierSuccessSpecialist)
                .HasForeignKey(carrier => carrier.CarrierSuccessSpecialistId);

            modelBuilder.Entity<UserEntity>()
                .HasMany(user => user.CarrierSuccessTeamLeadCarriers)
                .WithOne(carrier => carrier.CarrierSuccessTeamLead)
                .HasForeignKey(carrier => carrier.CarrierSuccessTeamLeadId);

            modelBuilder.Entity<UserEntity>()
                .HasMany(user => user.LoadClaims)
                .WithOne(loadClaim => loadClaim.User)
                .HasForeignKey(loadClaim => loadClaim.UserId);

            modelBuilder.Entity<UserEntity>()
               .HasMany(user => user.UserCommunicationUsers)
               .WithOne(userCommunicationUser => userCommunicationUser.User)
               .HasForeignKey(userCommunicationUser => userCommunicationUser.UserId);

            modelBuilder.Entity<UserEntity>()
               .HasMany(user => user.UserCommunicationAcknowledgements)
               .WithOne(userCommunicationAcknowledgements => userCommunicationAcknowledgements.User)
               .HasForeignKey(userCommunicationAcknowledgements => userCommunicationAcknowledgements.UserId);

            modelBuilder.Entity<UserEntity>()
              .HasMany(user => user.UserCommunications)
              .WithOne(userCommunication => userCommunication.Owner)
              .HasForeignKey(userCommunication => userCommunication.OwnerId);


            modelBuilder.Entity<UserEntity>()
              .HasMany(user => user.UserAgreements)
              .WithOne(agreements => agreements.User)
              .HasForeignKey(agreements => agreements.UserId);
        }

        public virtual List<UserNotificationEntity> UserNotifications { get; set; }

        public virtual List<NotificationDataEntity> NotificationDatas { get; set; }

        public virtual List<SecurityUserAccessRoleEntity> SecurityUserAccessRoles { get; set; } = new List<SecurityUserAccessRoleEntity>();

        public virtual List<UserCarrierScacEntity> UserCarrierScacs { get; set; } = new List<UserCarrierScacEntity>();

        public virtual CarrierScacEntity PrimaryScacEntity { get; set; }

        public virtual CustomerEntity PrimaryCustomer { get; set; }

        public virtual List<UserShipperEntity> UserShippers { get; set; } = new List<UserShipperEntity>();

        public virtual List<CustomerEntity> SuccessManagerCustomers { get; set; }

        public virtual List<CustomerEntity> SuccessSpecialistCustomers { get; set; }

        public virtual List<CarrierEntity> CarrierSuccessSpecialistCarriers { get; set; }

        public virtual List<CarrierEntity> CarrierSuccessTeamLeadCarriers { get; set; }

        public virtual List<LoadClaimEntity> LoadClaims { get; set; }

        public virtual List<UserCommunicationUserEntity> UserCommunicationUsers { get; set; }

        public virtual List<UserCommunicationAcknowledgementEntity> UserCommunicationAcknowledgements { get; set; }

        public virtual List<UserCommunicationEntity> UserCommunications { get; set; }
        public virtual List<UserAgreementDocumentEntity> UserAgreements { get; set; }
    }
}
