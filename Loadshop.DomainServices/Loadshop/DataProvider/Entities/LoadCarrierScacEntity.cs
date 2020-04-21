using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadCarrierScacEntity : LoadCarrierScac
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<LoadCarrierScacEntity>()
                    .Property(x => x.ContractRate)
                    .HasColumnType("decimal(11,4)");

            modelBuilder.Entity<LoadCarrierScacEntity>()
                .HasOne(lcs => lcs.CarrierScac)
                .WithMany()
                .HasForeignKey(lcs => lcs.Scac);
        }

        public virtual LoadEntity Load { get; set; }
        public virtual CarrierScacEntity CarrierScac { get; set; }
    }
}
