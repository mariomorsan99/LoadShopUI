using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class CustomerContactEntity: CustomerContact
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);

            modelBuilder.Entity<CustomerContactEntity>()
                .HasOne(customerContact => customerContact.Customer)
                .WithMany(customer => customer.CustomerContacts)
                .HasForeignKey(customerContact => customerContact.CustomerId);
        }

        public virtual CustomerEntity Customer { get; set; }
    }
}
