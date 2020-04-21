using Microsoft.EntityFrameworkCore;
using System;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadLineItemEntity : LoadLineItem
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);

            modelBuilder.Entity<LoadLineItemEntity>()
                .HasOne(x => x.PickupStop)
                .WithMany(x => x.PickupLineItems)
                .HasForeignKey(x => x.PickupStopId);

            modelBuilder.Entity<LoadLineItemEntity>()
                .HasOne(x => x.DeliveryStop)
                .WithMany(x => x.DeliveryLineItems)
                .HasForeignKey(x => x.DeliveryStopId);

            modelBuilder.Entity<LoadLineItemEntity>()
                .HasOne(x => x.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(x => x.UnitOfMeasureId);
        }

        public virtual LoadStopEntity PickupStop { get; set; }
        public virtual LoadStopEntity DeliveryStop { get; set; }
        public virtual UnitOfMeasureEntity UnitOfMeasure { get; set; }
    }
}
