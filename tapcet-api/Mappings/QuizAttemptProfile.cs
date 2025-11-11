using AutoMapper;
using tapcet_api.DTO.Attempt;
using tapcet_api.Models;

namespace tapcet_api.Mappings
{
    public class QuizAttemptProfile : Profile
    {
        public QuizAttemptProfile()
        {
            CreateMap<QuizAttempt, QuizAttemptResponseDto>()
                .ForMember(dest => dest.QuizTitle, 
                    opt => opt.MapFrom(src => src.Quiz != null ? src.Quiz.Title : "Unknown"))
                .ForMember(dest => dest.UserName, 
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Unknown"))
                .ForMember(dest => dest.IsCompleted, 
                    opt => opt.MapFrom(src => src.CompletedAt.HasValue));

            CreateMap<StartQuizAttemptDto, QuizAttempt>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Score, opt => opt.Ignore())
                .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Quiz, opt => opt.Ignore())
                .ForMember(dest => dest.UserAnswers, opt => opt.Ignore());
        }
    }
}