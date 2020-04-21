using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class CarrierEntity : Data.Carrier
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<CarrierEntity>()
                .HasMany(carrier => carrier.CarrierScacs)
                .WithOne(carrierScac => carrierScac.Carrier)
                .HasForeignKey(carrierScac => carrierScac.CarrierId);

            modelBuilder.Entity<CarrierEntity>()
                .HasMany(carrier => carrier.UserCarrierScacs)
                .WithOne(userCarrierScac => userCarrierScac.Carrier)
                .HasForeignKey(userCarrierScac => userCarrierScac.CarrierId);

            modelBuilder.Entity<CarrierEntity>()
                .HasMany(carrier => carrier.LoadCarrierGroupCarriers)
                .WithOne(loadCarrierGroupCarrier => loadCarrierGroupCarrier.Carrier)
                .HasForeignKey(loadCarrierGroupCarrier => loadCarrierGroupCarrier.CarrierId);

            modelBuilder.Entity<CarrierEntity>()
                .HasOne(carrier => carrier.CarrierSuccessSpecialist)
                .WithMany(user => user.CarrierSuccessSpecialistCarriers)
                .HasForeignKey(carrier => carrier.CarrierSuccessSpecialistId);


            modelBuilder.Entity<CarrierEntity>()
                .HasOne(carrier => carrier.CarrierSuccessTeamLead)
                .WithMany(user => user.CarrierSuccessTeamLeadCarriers)
                .HasForeignKey(carrier => carrier.CarrierSuccessTeamLeadId);

            modelBuilder.Entity<CarrierEntity>()
               .HasMany(carrier => carrier.UserCommunicationCarriers)
               .WithOne(userCommunicationCarrier => userCommunicationCarrier.Carrier)
               .HasForeignKey(userCommunicationCarrier => userCommunicationCarrier.CarrierId);
        }

        public virtual List<CarrierScacEntity> CarrierScacs { get; set; }
        public virtual List<UserCarrierScacEntity> UserCarrierScacs { get; set; }
        public virtual List<LoadCarrierGroupCarrierEntity> LoadCarrierGroupCarriers { get; set; }

        public virtual UserEntity CarrierSuccessTeamLead { get; set; }
        public virtual UserEntity CarrierSuccessSpecialist { get; set; }
        public virtual List<UserCommunicationCarrierEntity> UserCommunicationCarriers { get; set; }
    }
}
