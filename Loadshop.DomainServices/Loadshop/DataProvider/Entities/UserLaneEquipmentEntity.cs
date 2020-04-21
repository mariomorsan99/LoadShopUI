using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class UserLaneEquipmentEntity : UserLaneEquipment
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<UserLaneEquipmentEntity>()
                .HasOne(x => x.UserLane)
                .WithMany(x => x.UserLaneEquipments);

            modelBuilder.Entity<UserLaneEquipmentEntity>()
                .HasOne(x => x.Equipment)
                .WithMany(x => x.UserLaneEquipments);
        }

        public virtual UserLaneEntity UserLane { get; set; }

        public virtual EquipmentEntity Equipment { get; set; }
    }
}
