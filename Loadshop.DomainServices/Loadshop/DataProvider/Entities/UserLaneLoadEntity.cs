using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class UserLaneLoadEntity : UserLaneLoad
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<UserLaneLoadEntity>()
                .HasOne(x => x.UserLane)
                .WithMany(x => x.UserLaneLoads);

            modelBuilder.Entity<UserLaneLoadEntity>()
                .HasOne(x => x.Load)
                .WithMany(x => x.UserLaneLoads);
        }

        public virtual UserLaneEntity UserLane { get; set; }

        public virtual LoadEntity Load { get; set; }
    }
}
