using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadTransactionEntity : LoadTransaction
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<LoadTransactionEntity>()
                .HasOne(x => x.Load)
                .WithMany(x => x.LoadTransactions);

            modelBuilder.Entity<LoadTransactionEntity>()
                .HasOne(x => x.TransactionType)
                .WithMany(x => x.LoadTransactions);

            modelBuilder.Entity<LoadTransactionEntity>()
                .HasOne(x => x.Claim)
                .WithOne(x => x.Transaction);
        }

        public virtual LoadEntity Load { get; set; }

        public virtual TransactionTypeEntity TransactionType { get; set; }

        public virtual LoadClaimEntity Claim { get; set; }
    }
}
