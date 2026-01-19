using AutoMapper;
using tapcet_api.DTO.Unit;
using tapcet_api.Models;

namespace tapcet_api.Mappings
{
    public class UnitProfile : Profile
    {
        public UnitProfile()
        {
            CreateMap<Unit, UnitResponseDto>()
                .ForMember(dest => dest.CourseTitle,
                    opt => opt.MapFrom(src => src.Course != null ? src.Course.Title : string.Empty))
                .ForMember(dest => dest.QuizCount,
                    opt => opt.MapFrom(src => src.Quizzes.Count));

            CreateMap<Unit, UnitWithQuizzesDto>()
                .ForMember(dest => dest.CourseTitle,
                    opt => opt.MapFrom(src => src.Course != null ? src.Course.Title : string.Empty));

            CreateMap<CreateUnitDto, Unit>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.Quizzes, opt => opt.Ignore());

            CreateMap<UpdateUnitDto, Unit>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.Quizzes, opt => opt.Ignore());
        }
    }
}
