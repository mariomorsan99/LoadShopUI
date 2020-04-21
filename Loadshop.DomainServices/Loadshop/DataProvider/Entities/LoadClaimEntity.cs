using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class LoadClaimEntity : LoadClaim
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<LoadClaimEntity>()
                .HasOne(x => x.Transaction)
                .WithOne(x => x.Claim);

            modelBuilder.Entity<LoadClaimEntity>()
                .HasOne(x => x.User)
                .WithMany(x => x.LoadClaims)
                .HasForeignKey(x => x.UserId);

            modelBuilder.Entity<LoadClaimEntity>()
                .HasOne(loadClaim => loadClaim.CarrierScac)
                .WithMany(carrierScac => carrierScac.LoadClaims)
                .HasForeignKey(loadClaim => loadClaim.Scac);
        }

        public virtual LoadTransactionEntity Transaction { get; set; }

        public virtual UserEntity User { get; set; }

        public virtual RatingQuestionAnswerEntity RatingQuestionAnswer { get; set; }

        public virtual CarrierScacEntity CarrierScac { get; set; }
    }
}
