using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class UserShipperEntity : UserShipper
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<UserShipperEntity>()
                .HasOne(userShipper => userShipper.User)
                .WithMany(user => user.UserShippers)
                .HasForeignKey(userShipper => userShipper.UserId);

            modelBuilder.Entity<UserShipperEntity>()
               .HasOne(userShipper => userShipper.Customer)
               .WithMany(customer => customer.UserShippers)
               .HasForeignKey(userShipper => userShipper.CustomerId);

            modelBuilder.Entity<UserShipperEntity>()
                .HasKey(userShipper => new { userShipper.UserId, userShipper.CustomerId });
        }

        public virtual UserEntity User { get; set; }

        public virtual CustomerEntity Customer { get; set; }
    }
}
