using AutoMapper;
using Loadshop.DomainServices.Common.Services;
using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Loadshop.Services.Utility;
using Loadshop.DomainServices.Security;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class RatingService : IRatingService
    {
        private const string ErrorPrefix = "urn:RatingService";

        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly ISecurityService _securityService;
        private readonly ICommonService _commonService;
        private readonly ServiceUtilities _serviceUtilities;

        public RatingService(LoadshopDataContext context,
            IMapper mapper,
            IUserContext userContext,
            ISecurityService securityService,
            ICommonService commonService,
            ServiceUtilities serviceUtilities)
        {
            _context = context;
            _mapper = mapper;
            _userContext = userContext;
            _securityService = securityService;
            _commonService = commonService;
            _serviceUtilities = serviceUtilities;
        }

        public async Task<List<RatingQuestionData>> GetRatingQuestions()
        {
            var entities = await _context.RatingQuestions.AsNoTracking()
                .ToListAsync();

            var result = _mapper.Map<List<RatingQuestionData>>(entities);

            return result;
        }
        public async Task<RatingQuestionData> GetRatingQuestion(Guid questionId)
        {
            var entity = await _context.RatingQuestions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.RatingQuestionId == questionId);

            if (entity == null)
            {
                throw new ValidationException("Question was not found with id");
            }

            var result = _mapper.Map<RatingQuestionData>(entity);

            return result;
        }

        public async Task<RatingQuestionAnswerData> GetLatestRatingQuestionAnswer(Guid loadId)
        {
            var entity = await _context.RatingQuestionAnswers.AsNoTracking()
                                .Include(x => x.RatingQuestion)
                                .Where(x=> x.LoadId == loadId)
                                .OrderByDescending(x=> x.CreateDtTm)
                                .FirstOrDefaultAsync();

            if (entity == null)
            {
                throw new ValidationException("Question was not found with id");
            }

            var result = _mapper.Map<RatingQuestionAnswerData>(entity);

            return result;
        }


        public async Task AddRatingQuestionAnswer(RatingQuestionAnswerData ratingQuestionAnswer, bool saveChanges = false)
        {
            if (ratingQuestionAnswer.RatingQuestionId == null ||
                ratingQuestionAnswer.RatingQuestionId == Guid.Empty)
            {
                throw new ValidationException("Answer must have a question");
            }

            if (!ratingQuestionAnswer.LoadId.HasValue)
            {
                throw new ValidationException("Answer must have a load attached");
            }

            if (!ratingQuestionAnswer.LoadClaimId.HasValue)
            {
                throw new ValidationException("Answer must be tied to a load claim");
            }

            var entity = _mapper.Map<RatingQuestionAnswerEntity>(ratingQuestionAnswer);

            _context.RatingQuestionAnswers.Add(entity);

            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string> GetRatingReason(Guid loadId)
        {
            try
            {
                var ratingQuestionAnswer = await GetLatestRatingQuestionAnswer(loadId);

                var ratingQuestion = ratingQuestionAnswer.RatingQuestion;
                if (ratingQuestion == null)
                {
                    ratingQuestion = await GetRatingQuestion(ratingQuestionAnswer.RatingQuestionId);
                }

                return ratingQuestion.DisplayText;
            }
            catch (ValidationException)
            {
                return string.Empty;
            }
        }
    }
}
