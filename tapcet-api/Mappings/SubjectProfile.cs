using AutoMapper;
using tapcet_api.DTO.Subject;
using tapcet_api.Models;

namespace tapcet_api.Mappings
{
    public class SubjectProfile : Profile
    {
        public SubjectProfile()
        {
            CreateMap<Subject, SubjectResponseDto>()
                .ForMember(dest => dest.CourseCount,
                    opt => opt.MapFrom(src => src.Courses.Count));

            CreateMap<Subject, SubjectWithCoursesDto>();

            CreateMap<CreateSubjectDto, Subject>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Courses, opt => opt.Ignore());

            CreateMap<UpdateSubjectDto, Subject>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Courses, opt => opt.Ignore());
        }
    }
}
