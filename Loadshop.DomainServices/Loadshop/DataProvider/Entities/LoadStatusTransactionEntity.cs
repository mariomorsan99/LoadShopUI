using Loadshop.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadStatusTransactionEntity : LoadStatusTransaction
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
        }
    }
}
