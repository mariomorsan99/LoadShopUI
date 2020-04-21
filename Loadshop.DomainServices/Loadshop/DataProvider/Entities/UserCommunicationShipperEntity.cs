using Microsoft.EntityFrameworkCore;
using System;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class UserCommunicationShipperEntity : UserCommunicationShipper
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<UserCommunicationShipperEntity>()
                .HasOne(userCommunicationShipper => userCommunicationShipper.UserCommunication)
                .WithMany(userCommunication => userCommunication.UserCommunicationShippers)
                .HasForeignKey(userCommunicationShipper => userCommunicationShipper.UserCommunicationShipperId);

            modelBuilder.Entity<UserCommunicationShipperEntity>()
                .HasOne(userCommunicationShipper => userCommunicationShipper.Customer)
                .WithMany(customer => customer.UserCommunicationShippers)
                .HasForeignKey(userCommunicationShipper => userCommunicationShipper.CustomerId);
        }

        public virtual UserCommunicationEntity UserCommunication { get; set; }
        public virtual CustomerEntity Customer { get; set; }
    }
}
