using System;
using System.Collections.Generic;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class SpecialInstructionEntity : SpecialInstruction
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<SpecialInstructionEntity>()
                .HasOne(x => x.Customer)
                .WithMany(x => x.SpecialInstructions);

            modelBuilder.Entity<SpecialInstructionEntity>()
                .HasMany(x => x.SpecialInstructionEquipment)
                .WithOne(x => x.SpecialInstruction)
                .HasForeignKey(x => x.SpecialInstructionEquipmentId);
        }

        public virtual CustomerEntity Customer { get; set; }

        public virtual List<SpecialInstructionEquipmentEntity> SpecialInstructionEquipment { get; set; }
    }
}
