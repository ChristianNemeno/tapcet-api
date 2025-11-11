using AutoMapper;
using tapcet_api.DTO.Choice;
using tapcet_api.Models;

namespace tapcet_api.Mappings
{
    public class ChoiceProfile : Profile
    {
        public ChoiceProfile()
        {
            CreateMap<Choice, ChoiceResponseDto>();

            CreateMap<CreateChoiceDto, Choice>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
                .ForMember(dest => dest.Question, opt => opt.Ignore())
                .ForMember(dest => dest.UserAnswers, opt => opt.Ignore());
        }
    }
}