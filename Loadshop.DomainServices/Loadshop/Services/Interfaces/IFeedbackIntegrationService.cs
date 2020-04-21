using System;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface IFeedbackIntegrationService
    {
        Task<QuestionData> GetQuestionAsync(FeedbackQuestionCodeEnum feedbackQuestionCode);
        Task<QuestionResponseData> GetResponseAsync(FeedbackQuestionCodeEnum feedbackQuestionCode, Guid? loadId);
        Task<QuestionResponseData> SaveResponseAsync(QuestionResponseData data);
    }
}