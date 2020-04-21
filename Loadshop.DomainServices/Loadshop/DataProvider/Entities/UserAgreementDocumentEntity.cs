using Loadshop.Data.LoadShop.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class UserAgreementDocumentEntity : UserAgreementDocument
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<UserAgreementDocumentEntity>()
                .HasOne(x => x.Agreement)
                .WithMany(x => x.UserAgreements)
                .HasForeignKey(x => x.AgreementDocumentId);
        }

        public virtual AgreementDocumentEntity Agreement { get; set; }
        public virtual UserEntity User { get; set; }
    }
}