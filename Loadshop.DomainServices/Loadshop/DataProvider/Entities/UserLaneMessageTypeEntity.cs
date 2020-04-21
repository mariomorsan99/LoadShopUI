using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class UserLaneMessageTypeEntity : UserLaneMessageType
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<UserLaneMessageTypeEntity>()
                .HasOne(x => x.MessageType);

            modelBuilder.Entity<UserLaneMessageTypeEntity>()
                .HasOne(x => x.UserLane)
                .WithMany(x => x.UserLaneMessageTypes);
        }

        public virtual MessageTypeEntity MessageType { get; set; }

        public virtual UserLaneEntity UserLane { get; set; }
    }
}
