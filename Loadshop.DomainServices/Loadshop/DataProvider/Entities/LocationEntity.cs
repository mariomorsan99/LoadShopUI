using Microsoft.EntityFrameworkCore;
using System;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LocationEntity : Location
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);

            modelBuilder.Entity<LocationEntity>()
                .HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId);

            modelBuilder.Entity<LocationEntity>()
                .HasOne(x => x.AppointmentSchedulerConfirmationType)
                .WithMany()
                .HasForeignKey(x => x.AppointmentSchedulerConfirmationTypeId);
        }

        public virtual CustomerEntity Customer { get; set; }
        public virtual AppointmentSchedulerConfirmationTypeEntity AppointmentSchedulerConfirmationType { get; set; }
    }
}
