using System;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class QuestionResponseData
    {
        public FeedbackQuestionCodeEnum FeedbackQuestionCode { get; set; }
        public bool Answer { get; set; }
        public Guid? LoadId { get; set; }
    }
}
