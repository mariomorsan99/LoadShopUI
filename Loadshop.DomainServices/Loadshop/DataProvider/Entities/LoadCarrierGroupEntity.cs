using System;
using System.Collections.Generic;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadCarrierGroupEntity : LoadCarrierGroup
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<LoadCarrierGroupEntity>()
                .HasOne(x => x.Customer)
                .WithMany(x => x.LoadCarrierGroups);

            modelBuilder.Entity<LoadCarrierGroupEntity>()
                .HasMany(x => x.LoadCarrierGroupCarriers);


            modelBuilder.Entity<LoadCarrierGroupEntity>()
                .HasMany(x => x.LoadCarrierGroupEquipment)
                .WithOne(x => x.LoadCarrierGroup)
                .HasForeignKey(x => x.LoadCarrierGroupId);

            modelBuilder.Entity<LoadCarrierGroupEntity>()
                .HasOne(loadCarrierGroup => loadCarrierGroup.LoadCarrierGroupType)
                .WithMany(loadCarrierGroupEntity => loadCarrierGroupEntity.LoadCarrierGroups)
                .HasForeignKey(loadCarrierGroup => loadCarrierGroup.LoadCarrierGroupTypeId);
        }

        public virtual CustomerEntity Customer { get; set; }

        public virtual LoadCarrierGroupTypeEntity LoadCarrierGroupType { get; set; }

        public virtual List<LoadCarrierGroupCarrierEntity> LoadCarrierGroupCarriers { get; set; }

        public virtual List<LoadCarrierGroupEquipmentEntity> LoadCarrierGroupEquipment { get; set; }

        public virtual List<PostedLoadCarrierGroupEntity> PostedLoadCarrierGroups { get; set; }
    }
}
