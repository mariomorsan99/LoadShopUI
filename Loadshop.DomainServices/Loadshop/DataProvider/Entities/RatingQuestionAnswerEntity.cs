using Microsoft.EntityFrameworkCore;
using System;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class RatingQuestionAnswerEntity : RatingQuestionAnswer
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<RatingQuestionAnswerEntity>()
                .HasOne(x => x.RatingQuestion)
                .WithMany(x => x.RatingQuestionAnswers);

            modelBuilder.Entity<RatingQuestionAnswerEntity>()
                .HasOne(x => x.LoadClaim)
                .WithOne(x => x.RatingQuestionAnswer);

            modelBuilder.Entity<RatingQuestionAnswerEntity>()
                .HasOne(x => x.Load)
                .WithOne(x => x.RatingQuestionAnswer);
        }

        public virtual RatingQuestionEntity RatingQuestion { get; set; }
        public virtual LoadClaimEntity LoadClaim { get; set; }
        public virtual LoadEntity Load { get; set; }
    }
}
