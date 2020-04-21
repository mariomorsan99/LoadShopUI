using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadHistoryEntity : LoadHistory
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);

            modelBuilder.Entity<LoadHistoryEntity>()
                .HasOne(x => x.Load);

            modelBuilder.Entity<LoadHistoryEntity>()
                .HasOne(x => x.Customer);
        }

        public virtual LoadEntity Load { get; set; }
        public virtual CustomerEntity Customer { get; set; }
    }
}
