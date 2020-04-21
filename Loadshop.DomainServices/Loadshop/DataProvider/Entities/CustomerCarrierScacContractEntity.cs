using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class CustomerCarrierScacContractEntity : CustomerCarrierScacContract
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<CustomerCarrierScacContractEntity>()
                .HasOne(customerCarrierScacContract => customerCarrierScacContract.CarrierScac)
                .WithMany(carrierScac => carrierScac.CustomerCarrierScacContracts)
                .HasForeignKey(customerCarrierScacContract => customerCarrierScacContract.Scac);

            modelBuilder.Entity<CustomerCarrierScacContractEntity>()
              .HasOne(customerCarrierScacContract => customerCarrierScacContract.Customer)
              .WithMany(customer => customer.CustomerCarrierScacContracts)
              .HasForeignKey(customerCarrierScacContract => customerCarrierScacContract.CustomerId);
        }

        public virtual CarrierScacEntity CarrierScac { get; set; }
        public virtual CustomerEntity Customer { get; set; }
    }
}
