using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadDocumentEntity : LoadDocument
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);

            modelBuilder.Entity<LoadDocumentEntity>()
                .HasOne(_ => _.Load)
                .WithMany(_ => _.LoadDocuments)
                .HasForeignKey(_ => _.LoadId);
        }

        public virtual LoadDetailViewEntity LoadView { get; set; }
        public virtual LoadEntity Load { get; set; }
    }
}
