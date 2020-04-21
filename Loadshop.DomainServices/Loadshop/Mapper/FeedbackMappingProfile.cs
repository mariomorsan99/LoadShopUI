using AutoMapper;
using FeedbackService.Models.V1;
using Loadshop.DomainServices.Loadshop.Services.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loadshop.DomainServices.Loadshop.Mapper
{
    public class FeedbackMappingProfile : Profile
    {
        public FeedbackMappingProfile()
        {
            CreateMap<Question, QuestionData>().ReverseMap();
            CreateMap<QuestionReason, QuestionReasonData>().ReverseMap();
            CreateMap<Response, QuestionData>().ReverseMap();
        }
    }
}
