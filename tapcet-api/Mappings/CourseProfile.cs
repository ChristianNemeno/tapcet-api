using AutoMapper;
using tapcet_api.DTO.Course;
using tapcet_api.Models;

namespace tapcet_api.Mappings
{
    public class CourseProfile : Profile
    {
        public CourseProfile()
        {
            CreateMap<Course, CourseResponseDto>()
                .ForMember(dest => dest.SubjectName,
                    opt => opt.MapFrom(src => src.Subject != null ? src.Subject.Name : string.Empty))
                .ForMember(dest => dest.UnitCount,
                    opt => opt.MapFrom(src => src.Units.Count));

            CreateMap<Course, CourseWithUnitsDto>()
                .ForMember(dest => dest.SubjectName,
                    opt => opt.MapFrom(src => src.Subject != null ? src.Subject.Name : string.Empty));

            CreateMap<CreateCourseDto, Course>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Subject, opt => opt.Ignore())
                .ForMember(dest => dest.Units, opt => opt.Ignore());

            CreateMap<UpdateCourseDto, Course>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Subject, opt => opt.Ignore())
                .ForMember(dest => dest.Units, opt => opt.Ignore());
        }
    }
}
