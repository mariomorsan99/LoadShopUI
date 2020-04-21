using Microsoft.EntityFrameworkCore;
using System;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadServiceTypeEntity : LoadServiceType
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);

            modelBuilder.Entity<LoadServiceTypeEntity>()
                .HasOne(x => x.Load)
                .WithMany(x => x.LoadServiceTypes)
                .HasForeignKey(x => x.LoadId);

            modelBuilder.Entity<LoadServiceTypeEntity>()
                .HasOne(x => x.ServiceType)
                .WithMany(x => x.LoadServiceTypes)
                .HasForeignKey(x => x.ServiceTypeId);
        }

        public virtual LoadEntity Load { get; set; }
        public virtual ServiceTypeEntity ServiceType { get; set; }
        public virtual LoadDetailViewEntity LoadView { get; set; }
    }
}
