using Microsoft.EntityFrameworkCore;
using System;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadCarrierGroupEquipmentEntity : LoadCarrierGroupEquipment
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<LoadCarrierGroupEquipmentEntity>()
                .HasOne(loadCarrierGroupEquipment => loadCarrierGroupEquipment.LoadCarrierGroup)
                .WithMany(loadCarrierGroup => loadCarrierGroup.LoadCarrierGroupEquipment)
                .HasForeignKey(loadCarrierGroupEquipment => loadCarrierGroupEquipment.LoadCarrierGroupId);

            modelBuilder.Entity<LoadCarrierGroupEquipmentEntity>()
               .HasOne(loadCarrierGroupEquipment => loadCarrierGroupEquipment.Equipment)
               .WithMany(equipment => equipment.LoadCarrierGroupEquipment)
               .HasForeignKey(loadCarrierGroupEquipment => loadCarrierGroupEquipment.EquipmentId);
        }

        public LoadCarrierGroupEntity LoadCarrierGroup { get; set; }
        public EquipmentEntity Equipment { get; set; }
    }
}
