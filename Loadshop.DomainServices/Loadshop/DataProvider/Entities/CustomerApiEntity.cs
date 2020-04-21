using Microsoft.EntityFrameworkCore;
using System;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class CustomerApiEntity : CustomerApi
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<CustomerApiEntity>()
                .HasOne(x => x.Customer);
        }

        public virtual CustomerEntity Customer { get; set; }
    }
}
