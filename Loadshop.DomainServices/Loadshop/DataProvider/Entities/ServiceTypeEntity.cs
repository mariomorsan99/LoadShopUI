using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class ServiceTypeEntity : ServiceType
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);

            modelBuilder.Entity<ServiceTypeEntity>()
                .HasMany(x => x.LoadServiceTypes)
                .WithOne(x => x.ServiceType)
                .HasForeignKey(x => x.ServiceTypeId);
        }

        public virtual List<LoadServiceTypeEntity> LoadServiceTypes { get; set; }
    }
}
