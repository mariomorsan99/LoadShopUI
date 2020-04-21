using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadCarrierScacRestrictionEntity : LoadCarrierScacRestriction
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<LoadCarrierScacRestrictionEntity>()
                .HasOne(loadCarrierScacRestriction => loadCarrierScacRestriction.LoadCarrierScacRestrictionType)
                .WithMany(loadCarrierScacRestrictionEntity => loadCarrierScacRestrictionEntity.LoadCarrierScacRestrictions)
                .HasForeignKey(loadCarrierScacRestriction => loadCarrierScacRestriction.LoadCarrierScacRestrictionTypeId);
        }

        public virtual LoadEntity Load { get; set; }
        public virtual LoadCarrierScacRestrictionTypeEntity LoadCarrierScacRestrictionType { get; set; }
    }
}
