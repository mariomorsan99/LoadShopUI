using Microsoft.EntityFrameworkCore;
using System;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class SmartSpotPriceQuoteLogEntity : SmartSpotPriceQuoteLog
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);

            modelBuilder.Entity<SmartSpotPriceQuoteLogEntity>()
                .HasOne(_ => _.User)
                .WithMany()
                .HasForeignKey(_ => _.UserId);
        }

        public virtual UserEntity User { get; set; }
    }
}
