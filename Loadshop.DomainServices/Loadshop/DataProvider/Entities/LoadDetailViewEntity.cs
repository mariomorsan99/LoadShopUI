using System;
using System.Collections.Generic;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadDetailViewEntity : LoadDetailView
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<LoadDetailViewEntity>().HasKey(x => x.LoadId);

            modelBuilder.Entity<LoadDetailViewEntity>()
                .HasMany(x => x.Contacts)
                .WithOne(x => x.LoadView)
                .HasForeignKey(x => x.LoadId);

            modelBuilder.Entity<LoadDetailViewEntity>()
                .HasMany(x => x.LoadStops)
                .WithOne(x => x.LoadView)
                .HasForeignKey(x => x.LoadId);

            modelBuilder.Entity<LoadDetailViewEntity>()
                .HasOne(x => x.Equipment)
                .WithMany(x => x.LoadViews);
            
            modelBuilder.Entity<LoadDetailViewEntity>()
                .HasMany(x => x.LoadDocuments)
                .WithOne(x => x.LoadView)
                .HasForeignKey(x => x.LoadId);

            modelBuilder.Entity<LoadDetailViewEntity>()
                .HasMany(x => x.LoadCurrentStatuses)
                .WithOne(x => x.LoadView)
                .HasForeignKey(x => x.LoadId);

            modelBuilder.Entity<LoadDetailViewEntity>()
                .HasMany(x => x.LoadServiceTypes)
                .WithOne(x => x.LoadView)
                .HasForeignKey(x => x.LoadId);
        }

        public virtual List<LoadContactEntity> Contacts { get; set; }

        public virtual List<LoadStopEntity> LoadStops { get; set; }

        public virtual EquipmentEntity Equipment { get; set; }
        public virtual List<LoadDocumentEntity> LoadDocuments { get; set; }
        public virtual List<LoadCurrentStatusEntity> LoadCurrentStatuses { get; set; }
        public virtual List<LoadServiceTypeEntity> LoadServiceTypes { get; set; }
    }
}
