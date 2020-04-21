using System;
using System.Collections.Generic;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class TransactionTypeEntity : TransactionType
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<TransactionTypeEntity>()
                .HasMany(x => x.LoadTransactions);
        }

        public virtual List<LoadTransactionEntity> LoadTransactions { get; set; }
    }
}
