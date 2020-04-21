using Microsoft.EntityFrameworkCore;
using System;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class SpecialInstructionEquipmentEntity : SpecialInstructionEquipment
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<SpecialInstructionEquipmentEntity>()
                .HasOne(x => x.SpecialInstruction)
                .WithMany(x => x.SpecialInstructionEquipment)
                .HasForeignKey(x => x.SpecialInstructionId);

            modelBuilder.Entity<SpecialInstructionEquipmentEntity>()
               .HasOne(spe => spe.Equipment)
               .WithMany(equipment => equipment.SpecialInstructionEquipment)
               .HasForeignKey(spe => spe.EquipmentId);
        }

        public SpecialInstructionEntity SpecialInstruction { get; set; }
        public EquipmentEntity Equipment { get; set; }
    }
}
