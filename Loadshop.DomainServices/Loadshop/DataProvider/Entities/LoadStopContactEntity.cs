using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadStopContactEntity : LoadStopContact
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<LoadStopContactEntity>()
                .HasOne(x => x.LoadStop)
                .WithMany(x => x.Contacts)
                .HasForeignKey(x => x.LoadStopId);
        }

        public virtual LoadStopEntity LoadStop { get; set; }
    }
}
