using AutoMapper;
using tapcet_api.DTO.Attempt;
using tapcet_api.Models;

namespace tapcet_api.Mappings
{
    public class UserAnswerProfile : Profile
    {
        public UserAnswerProfile()
        {
            CreateMap<SubmitAnswerDto, UserAnswer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.QuizAttemptId, opt => opt.Ignore())
                .ForMember(dest => dest.AnsweredAt, opt => opt.Ignore())
                .ForMember(dest => dest.QuizAttempt, opt => opt.Ignore())
                .ForMember(dest => dest.Question, opt => opt.Ignore())
                .ForMember(dest => dest.Choice, opt => opt.Ignore());
        }
    }
}