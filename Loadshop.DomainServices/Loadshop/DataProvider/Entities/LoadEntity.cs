using System;
using System.Collections.Generic;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadEntity : Load
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<LoadEntity>()
                .HasMany(x => x.Contacts)
                .WithOne(x => x.Load)
                .HasForeignKey(x => x.LoadId);

            modelBuilder.Entity<LoadEntity>()
                .HasMany(x => x.LoadStops)
                .WithOne(x => x.Load)
                .HasForeignKey(x => x.LoadId);

            modelBuilder.Entity<LoadEntity>()
                .HasMany(x => x.LoadTransactions);

            modelBuilder.Entity<LoadEntity>()
                .HasMany(x => x.UserLaneLoads)
                .WithOne(x => x.Load)
                .HasForeignKey(x => x.LoadId);

            modelBuilder.Entity<LoadEntity>()
                .HasOne(x => x.Customer)
                .WithMany(x => x.Loads);

            modelBuilder.Entity<LoadEntity>()
                .HasOne(x => x.Equipment)
                .WithMany(x => x.Loads);

            modelBuilder.Entity<LoadEntity>()
                .HasMany(x => x.CarrierScacs)
                .WithOne(x => x.Load)
                .HasForeignKey(x => x.LoadId);

            modelBuilder.Entity<LoadEntity>()
                .HasMany(x => x.CarrierScacRestrictions)
                .WithOne(x => x.Load)
                .HasForeignKey(x => x.LoadId);

            modelBuilder.Entity<LoadEntity>()
                .HasMany(x => x.LoadServiceTypes)
                .WithOne(x => x.Load)
                .HasForeignKey(x => x.LoadId);

            modelBuilder.Entity<LoadEntity>()
                .HasOne(x => x.TransportationMode)
                .WithMany()
                .HasForeignKey(x => x.TransportationModeId);

            modelBuilder.Entity<LoadEntity>()
                .HasOne(x => x.LatestLoadTransaction)
                .WithOne()
                .HasForeignKey<LoadEntity>(x => x.LatestLoadTransactionId);

            modelBuilder.Entity<LoadEntity>()
                .HasOne(x => x.CustomerLoadType)
                .WithMany()
                .HasForeignKey(x => x.CustomerLoadTypeId);

            modelBuilder.Entity<LoadEntity>()
                .HasMany(x => x.LoadCurrentStatuses)
                .WithOne(x => x.Load)
                .HasForeignKey(x => x.LoadId);
        }

        public virtual List<LoadContactEntity> Contacts { get; set; }

        public virtual List<LoadStopEntity> LoadStops { get; set; }

        public virtual List<LoadTransactionEntity> LoadTransactions { get; set; }

        public virtual List<UserLaneLoadEntity> UserLaneLoads { get; set; }

        public virtual CustomerEntity Customer { get; set; }

        public virtual EquipmentEntity Equipment { get; set; }

        public virtual List<LoadCarrierScacEntity> CarrierScacs { get; set; }

        public virtual List<LoadCarrierScacRestrictionEntity> CarrierScacRestrictions { get; set; }

        public virtual List<NotificationDataEntity> NotificationDetails { get; set; }

        public virtual List<LoadServiceTypeEntity> LoadServiceTypes { get; set; }
        public virtual TransportationModeEntity TransportationMode { get; set; }

        public virtual LoadTransactionEntity LatestLoadTransaction { get; set; }
        public virtual CustomerLoadTypeEntity CustomerLoadType { get; set; }
        public virtual RatingQuestionAnswerEntity RatingQuestionAnswer { get; set; }

        public virtual List<PostedLoadCarrierGroupEntity> PostedLoadCarrierGroups { get; set; }
        public virtual List<LoadDocumentEntity> LoadDocuments { get; set; }
        public virtual List<LoadCurrentStatusEntity> LoadCurrentStatuses { get; set; }
    }
}
