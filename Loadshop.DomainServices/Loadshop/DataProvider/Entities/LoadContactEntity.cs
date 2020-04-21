using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadContactEntity : LoadContact
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<LoadContactEntity>()
                .HasOne(x => x.Load)
                .WithMany(x => x.Contacts);

            modelBuilder.Entity<LoadContactEntity>()
                .HasOne(x => x.LoadView)
                .WithMany(x => x.Contacts);
        }

        public virtual LoadEntity Load { get; set; }

        public virtual LoadDetailViewEntity LoadView { get; set; }
    }
}
