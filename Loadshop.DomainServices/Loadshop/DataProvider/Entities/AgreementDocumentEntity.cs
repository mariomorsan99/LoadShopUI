using Loadshop.Data.LoadShop.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class AgreementDocumentEntity : AgreementDocument
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<AgreementDocumentEntity>()
                .HasMany(x => x.UserAgreements)
                .WithOne(x => x.Agreement)
                .HasForeignKey(x => x.AgreementDocumentId);
        }

        public virtual List<UserAgreementDocumentEntity> UserAgreements { get; set; }
    }
}
