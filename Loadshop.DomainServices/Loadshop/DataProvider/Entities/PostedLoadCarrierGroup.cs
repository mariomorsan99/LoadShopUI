using Loadshop.Data.LoadShop.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class PostedLoadCarrierGroupEntity : PostedLoadCarrierGroup
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);

            modelBuilder.Entity<PostedLoadCarrierGroupEntity>()
                .HasOne(postedLoadCarrierGroup => postedLoadCarrierGroup.Load)
                .WithMany(load => load.PostedLoadCarrierGroups)
                .HasForeignKey(postedLoadCarrierGroup => postedLoadCarrierGroup.LoadId);

            modelBuilder.Entity<PostedLoadCarrierGroupEntity>()
                .HasOne(postedLoadCarrierGroup => postedLoadCarrierGroup.LoadCarrierGroup)
                .WithMany(loadCarrierGroup => loadCarrierGroup.PostedLoadCarrierGroups)
                .HasForeignKey(postedLoadCarrierGroup => postedLoadCarrierGroup.LoadCarrierGroupId);
        }

        public virtual LoadEntity Load { get; set; }
        public virtual LoadCarrierGroupEntity LoadCarrierGroup { get; set; }
    }
}
