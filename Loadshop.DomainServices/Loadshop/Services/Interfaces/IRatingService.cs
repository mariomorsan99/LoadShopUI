using Loadshop.DomainServices.Loadshop.Services.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface IRatingService
    {
        Task<List<RatingQuestionData>> GetRatingQuestions();
        Task<RatingQuestionData> GetRatingQuestion(Guid questionId);
        Task<RatingQuestionAnswerData> GetLatestRatingQuestionAnswer(Guid loadId);
        Task AddRatingQuestionAnswer(RatingQuestionAnswerData ratingQuestionAnswer, bool saveChanges = false);

        /// <summary>
        /// Gets rating reason for a load
        /// </summary>
        /// <param name="loadId"></param>
        /// <returns></returns>
        Task<string> GetRatingReason(Guid loadId);
    }
}
