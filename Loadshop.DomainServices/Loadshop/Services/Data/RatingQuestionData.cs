using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
   public class RatingQuestionData
    {
        public Guid RatingQuestionId { get; set; }
        public string Question { get; set; }
        public string DisplayText { get; set; }
    }
}
