using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class UserCarrierScacEntity : UserCarrierScac
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<UserCarrierScacEntity>()
                .HasOne(userCarrierScac => userCarrierScac.Carrier)
                .WithMany(carrier => carrier.UserCarrierScacs)
                .HasForeignKey(userCarrierScac => userCarrierScac.CarrierId);

            modelBuilder.Entity<UserCarrierScacEntity>()
                .HasOne(userCarrierScac => userCarrierScac.User)
                .WithMany(user => user.UserCarrierScacs)
                .HasForeignKey(userCarrierScac => userCarrierScac.UserId);

            modelBuilder.Entity<UserCarrierScacEntity>()
                .HasOne(userCarrierScac => userCarrierScac.CarrierScac)
                .WithMany(carrierScac => carrierScac.UserCarrierScacs)
                .HasForeignKey(userCarrierScac => userCarrierScac.Scac);
        }
        public virtual CarrierEntity Carrier { get; set; }
        public virtual UserEntity User { get; set; }

        public virtual CarrierScacEntity CarrierScac { get; set; }
    }
}
