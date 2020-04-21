using System;
using System.Collections.Generic;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class EquipmentEntity : Equipment
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<EquipmentEntity>()
                .HasMany(x => x.Loads);

            modelBuilder.Entity<EquipmentEntity>()
                .HasMany(x => x.LoadViews);

            modelBuilder.Entity<EquipmentEntity>()
                .HasMany(x => x.UserLaneEquipments);

            modelBuilder.Entity<EquipmentEntity>()
                .HasMany(x => x.LoadCarrierGroupEquipment)
                .WithOne(loadCarrierGroupEquipment => loadCarrierGroupEquipment.Equipment)
                .HasForeignKey(loadCarrierGroupEquipment => loadCarrierGroupEquipment.EquipmentId);

            modelBuilder.Entity<EquipmentEntity>()
                .HasMany(x => x.SpecialInstructionEquipment)
                .WithOne(e => e.Equipment)
                .HasForeignKey(e => e.EquipmentId);
        }

        public virtual List<LoadEntity> Loads { get; set; }

        public virtual List<LoadDetailViewEntity> LoadViews { get; set; }

        public virtual List<UserLaneEquipmentEntity> UserLaneEquipments { get; set; }

        public virtual List<LoadCarrierGroupEquipmentEntity> LoadCarrierGroupEquipment { get; set; }
        public virtual List<SpecialInstructionEquipmentEntity> SpecialInstructionEquipment { get; set; }
    }
}
