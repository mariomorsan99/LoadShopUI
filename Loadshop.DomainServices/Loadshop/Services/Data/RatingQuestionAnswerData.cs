using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class RatingQuestionAnswerData
    {
        public Guid RatingQuestionId { get; set; }
        public Guid? LoadClaimId { get; set; }
        public Guid? LoadId { get; set; }
        public bool AnswerYN { get; set; }
        public string AdditionalComment { get; set; }
        public RatingQuestionData RatingQuestion { get; set; }
    }
}
