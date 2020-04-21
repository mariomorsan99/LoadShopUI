using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class QuestionData
    {
        public FeedbackQuestionCodeEnum FeedbackQuestionCode { get; set; }

        public int? QuestionId { get; set; }
        public string ApplicationCode { get; set; }
        public string Category { get; set; }
        public string QuestionText { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public IList<QuestionReasonData> QuestionReasons { get; set; }
        public string UpdateUser { get; set; }
    }
}
