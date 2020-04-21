using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadCarrierGroupCarrierEntity : LoadCarrierGroupCarrier
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<LoadCarrierGroupCarrierEntity>()
                .HasOne(x => x.LoadCarrierGroup)
                .WithMany(x => x.LoadCarrierGroupCarriers);

            modelBuilder.Entity<LoadCarrierGroupCarrierEntity>()
                .HasOne(loadCarrierGroupCarrier => loadCarrierGroupCarrier.Carrier)
                .WithMany(carrier => carrier.LoadCarrierGroupCarriers)
                .HasForeignKey(loadCarrierGroupCarrier => loadCarrierGroupCarrier.CarrierId);
        }

        public virtual LoadCarrierGroupEntity LoadCarrierGroup { get; set; }
        public virtual CarrierEntity Carrier { get; set; }
    }
}
