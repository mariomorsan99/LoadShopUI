using System;
using System.Collections.Generic;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class CarrierScacEntity : CarrierScac
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<CarrierScacEntity>()
                .HasOne(carrierScacEntity => carrierScacEntity.Carrier)
                .WithMany(carrier => carrier.CarrierScacs)
                .HasForeignKey(carrierScacEntity => carrierScacEntity.CarrierId);

            //modelBuilder.Entity<CarrierScacEntity>()
            //    .HasMany(carrierScac => carrierScac.Users)
            //    .WithOne(user => user.PrimaryScac)
            //    .HasForeignKey(user => user.Scac);

            modelBuilder.Entity<CarrierScacEntity>()
               .HasMany(carrierScac => carrierScac.UserCarrierScacs)
               .WithOne(userCarrierScac => userCarrierScac.CarrierScac)
               .HasForeignKey(userCarrierScac => userCarrierScac.Scac);

            modelBuilder.Entity<CarrierScacEntity>()
               .HasMany(carrierScac => carrierScac.CustomerCarrierScacContracts)
               .WithOne(customerCarrierScacContracts => customerCarrierScacContracts.CarrierScac)
               .HasForeignKey(userCarrierScac => userCarrierScac.Scac);

            modelBuilder.Entity<CarrierScacEntity>()
                .HasMany(carrierScac => carrierScac.LoadClaims)
                .WithOne(loadClaim => loadClaim.CarrierScac)
                .HasForeignKey(loadClaim => loadClaim.Scac);
        }

        public virtual CarrierEntity Carrier { get; set; }

        public virtual List<UserEntity> Users { get; set; }

        public virtual List<UserCarrierScacEntity> UserCarrierScacs { get; set; }

        public virtual List<CustomerCarrierScacContractEntity> CustomerCarrierScacContracts { get; set; }

        public virtual List<LoadClaimEntity> LoadClaims { get; set; } 
    }
}
