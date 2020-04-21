using System;
using System.Collections.Generic;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadStopEntity : LoadStop
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<LoadStopEntity>()
                .HasOne(x => x.Load)
                .WithMany(x => x.LoadStops);

            modelBuilder.Entity<LoadStopEntity>()
                .HasOne(x => x.LoadView)
                .WithMany(x => x.LoadStops);

            modelBuilder.Entity<LoadStopEntity>()
                .HasOne(x => x.AppointmentSchedulerConfirmationType)
                .WithMany()
                .HasForeignKey(x => x.AppointmentSchedulerConfirmationTypeId);

            modelBuilder.Entity<LoadStopEntity>()
                .HasOne(x => x.StopType)
                .WithMany()
                .HasForeignKey(x => x.StopTypeId);

            modelBuilder.Entity<LoadStopEntity>()
                .HasMany(x => x.Contacts)
                .WithOne(x => x.LoadStop)
                .HasForeignKey(x => x.LoadStopId);

            modelBuilder.Entity<LoadStopEntity>()
                .HasMany(x => x.PickupLineItems)
                .WithOne(x => x.PickupStop)
                .HasForeignKey(x => x.PickupStopId);

            modelBuilder.Entity<LoadStopEntity>()
                .HasMany(x => x.DeliveryLineItems)
                .WithOne(x => x.DeliveryStop)
                .HasForeignKey(x => x.DeliveryStopId);
        }

        public virtual LoadEntity Load { get; set; }

        public virtual LoadDetailViewEntity LoadView { get; set; }
        public virtual AppointmentSchedulerConfirmationTypeEntity AppointmentSchedulerConfirmationType { get; set; }
        public virtual StopTypeEntity StopType { get; set; }
        public virtual List<LoadStopContactEntity> Contacts { get; set; }
        public virtual List<LoadLineItemEntity> PickupLineItems { get; set; }
        public virtual List<LoadLineItemEntity> DeliveryLineItems { get; set; }
    }
}
