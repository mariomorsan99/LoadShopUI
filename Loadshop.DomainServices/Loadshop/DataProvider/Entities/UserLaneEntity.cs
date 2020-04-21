using System;
using System.Collections.Generic;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class UserLaneEntity : UserLane
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<UserLaneEntity>()
                .HasMany(x => x.UserLaneLoads);

            modelBuilder.Entity<UserLaneEntity>()
                .HasMany(x => x.UserLaneMessageTypes);

            modelBuilder.Entity<UserLaneEntity>()
                .HasMany(x => x.UserLaneEquipments);
        }

        public virtual List<UserLaneLoadEntity> UserLaneLoads { get; set; }

        public virtual List<UserLaneMessageTypeEntity> UserLaneMessageTypes { get; set; }

        public virtual List<UserLaneEquipmentEntity> UserLaneEquipments { get; set; }

        public virtual UserEntity User { get; set; }
    }
}
