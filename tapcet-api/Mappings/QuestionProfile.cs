using AutoMapper;
using tapcet_api.DTO.Question;
using tapcet_api.DTO.Quiz;
using tapcet_api.Models;

namespace tapcet_api.Mappings
{
    public class QuestionProfile : Profile
    {
        public QuestionProfile()
        {
            CreateMap<Question, QuestionResponseDto>()
                .ForMember(dest => dest.Choices, 
                    opt => opt.MapFrom(src => src.Choices));

            CreateMap<CreateQuestionDto, Question>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.QuizId, opt => opt.Ignore())
                .ForMember(dest => dest.Quiz, opt => opt.Ignore())
                .ForMember(dest => dest.UserAnswers, opt => opt.Ignore())
                .ForMember(dest => dest.Choices, opt => opt.MapFrom(src => src.Choices));
        }
    }
}