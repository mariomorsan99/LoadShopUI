using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadCurrentStatusEntity : LoadCurrentStatus
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);

            modelBuilder.Entity<LoadCurrentStatusEntity>()
                .HasOne(_ => _.Load)
                .WithMany(_ => _.LoadCurrentStatuses)
                .HasForeignKey(_ => _.LoadId);
        }

        public virtual LoadDetailViewEntity LoadView { get; set; }
        public virtual LoadEntity Load { get; set; }
    }
}
