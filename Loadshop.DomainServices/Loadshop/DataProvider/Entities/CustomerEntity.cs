using System;
using System.Collections.Generic;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class CustomerEntity : Customer
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<CustomerEntity>()
                .HasMany(x => x.Loads);

            modelBuilder.Entity<CustomerEntity>()
                .HasMany(customer => customer.UserShippers)
                .WithOne(userShipper => userShipper.Customer)
                .HasForeignKey(userShipper => userShipper.CustomerId);


            modelBuilder.Entity<CustomerEntity>()
                .HasMany(customer => customer.CustomerCarrierScacContracts)
                .WithOne(customerCarrierScacContracts => customerCarrierScacContracts.Customer)
                .HasForeignKey(customerCarrierScacContracts => customerCarrierScacContracts.CustomerId);

            modelBuilder.Entity<CustomerEntity>()
                .HasMany(x => x.LoadCarrierGroups);

            modelBuilder.Entity<CustomerEntity>()
                .HasMany(x => x.SpecialInstructions);

            modelBuilder.Entity<CustomerEntity>()
                .HasMany(customer => customer.PrimaryUsers)
                .WithOne(user => user.PrimaryCustomer)
                .HasForeignKey(user => user.PrimaryCustomerId);

            modelBuilder.Entity<CustomerEntity>()
                .HasMany(customer => customer.CustomerContacts)
                .WithOne(customerContact => customerContact.Customer)
                .HasForeignKey(customerContact => customerContact.CustomerId);

            modelBuilder.Entity<CustomerEntity>()
                .HasOne(customer => customer.SuccessManager)
                .WithMany(user => user.SuccessManagerCustomers)
                .HasForeignKey(customer => customer.SuccessManagerUserId);

            modelBuilder.Entity<CustomerEntity>()
                .HasOne(customer => customer.SuccessSpecialist)
                .WithMany(user => user.SuccessSpecialistCustomers)
                .HasForeignKey(customer => customer.SuccessSpecialistUserId);

            modelBuilder.Entity<CustomerEntity>()
                .HasOne(customer => customer.CustomerLoadType)
                .WithMany()
                .HasForeignKey(customer => customer.CustomerLoadTypeId);

            modelBuilder.Entity<CustomerEntity>()
                .HasMany(customer => customer.UserCommunicationShippers)
                .WithOne(userCommunicationShipper => userCommunicationShipper.Customer)
                .HasForeignKey(userCommunicationShipper => userCommunicationShipper.CustomerId);
        }

        public virtual List<LoadEntity> Loads { get; set; }

        public virtual List<UserShipperEntity> UserShippers { get; set; }

        public virtual List<CustomerCarrierScacContractEntity> CustomerCarrierScacContracts { get; set; }

        public virtual List<LoadCarrierGroupEntity> LoadCarrierGroups { get; set; }

        public virtual List<UserEntity> PrimaryUsers { get; set; }

        public virtual List<CustomerContactEntity> CustomerContacts { get; set; }
        public virtual UserEntity SuccessManager { get; set; }
        public virtual UserEntity SuccessSpecialist { get; set; }
        public virtual List<SpecialInstructionEntity> SpecialInstructions { get; set; }
        public virtual CustomerLoadTypeEntity CustomerLoadType { get; set; }
        public virtual List<UserCommunicationShipperEntity> UserCommunicationShippers { get; set; }
    }
}
