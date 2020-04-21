using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadCarrierScacRestrictionTypeEntity : LoadCarrierScacRestrictionType
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<LoadCarrierScacRestrictionTypeEntity>()
                .HasMany(loadCarrierScacRestrictionType => loadCarrierScacRestrictionType.LoadCarrierScacRestrictions)
                .WithOne(loadCarrierScacRestriction => loadCarrierScacRestriction.LoadCarrierScacRestrictionType)
                .HasForeignKey(loadCarrierScacRestriction => loadCarrierScacRestriction.LoadCarrierScacRestrictionTypeId);
        }

        public virtual List<LoadCarrierScacRestrictionEntity> LoadCarrierScacRestrictions { get; set; }
    }
}
