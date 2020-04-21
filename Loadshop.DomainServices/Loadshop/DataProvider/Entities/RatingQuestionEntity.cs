using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class RatingQuestionEntity : RatingQuestion
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
        }


        public virtual List<RatingQuestionAnswerEntity> RatingQuestionAnswers { get; set; }
    }
}
