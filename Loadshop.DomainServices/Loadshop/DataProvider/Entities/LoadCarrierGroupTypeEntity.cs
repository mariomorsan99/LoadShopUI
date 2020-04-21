using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadCarrierGroupTypeEntity : LoadCarrierGroupType
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<LoadCarrierGroupTypeEntity>()
                .HasMany(loadCarrierGroupType => loadCarrierGroupType.LoadCarrierGroups)
                .WithOne(loadCarrierGroup => loadCarrierGroup.LoadCarrierGroupType)
                .HasForeignKey(loadCarrierGroup => loadCarrierGroup.LoadCarrierGroupTypeId);
        }

        public virtual List<LoadCarrierGroupEntity> LoadCarrierGroups { get; set; }
    }
}
