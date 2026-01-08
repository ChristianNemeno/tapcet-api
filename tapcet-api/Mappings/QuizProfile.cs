using AutoMapper;
using tapcet_api.DTO.Quiz;
using tapcet_api.Models;


namespace tapcet_api.Mappings
{
    public class QuizProfile : Profile
    {
        public QuizProfile()
        {
            CreateMap<Quiz, QuizResponseDto>()
                .ForMember(dest => dest.CreatedByName, 
                    opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.UserName : "Unknown"))
                .ForMember(dest => dest.QuestionCount, 
                    opt => opt.MapFrom(src => src.Questions.Count))
                .ForMember(dest => dest.Questions, 
                    opt => opt.MapFrom(src => src.Questions));

            
            CreateMap<Quiz, QuizSummaryDto>()
                .ForMember(dest => dest.CreatedById,
                    opt => opt.MapFrom(src => src.CreatedById))
                .ForMember(dest => dest.CreatedByName, 
                    opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.UserName : "Unknown"))
                .ForMember(dest => dest.QuestionCount, 
                    opt => opt.MapFrom(src => src.Questions.Count))
                .ForMember(dest => dest.AttemptCount, 
                    opt => opt.MapFrom(src => src.QuizAttempts.Count));

            CreateMap<CreateQuizDto, Quiz>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.QuizAttempts, opt => opt.Ignore())
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));

            CreateMap<UpdateQuizDto, Quiz>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Questions, opt => opt.Ignore())
                .ForMember(dest => dest.QuizAttempts, opt => opt.Ignore());
        }
    }
}
