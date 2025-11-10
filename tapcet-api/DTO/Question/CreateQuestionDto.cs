using System.ComponentModel.DataAnnotations;
using tapcet_api.DTO.Choice;

namespace tapcet_api.DTO.Quiz
{
    public class CreateQuestionDto
    {
        [Required(ErrorMessage = "Question text is required")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Question must be between 5 and 100 characters")]
        public required string Text { get; set; }

        [StringLength(300, ErrorMessage = "Explanation cannot exceed 300 characters")]
        public string? Explanation { get; set; }

        [Url(ErrorMessage = "Invalid image URL format")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Choices are required")]
        [MinLength(2, ErrorMessage = "Question must have at least 2 choices")]
        [MaxLength(6, ErrorMessage = "Question cannot have more than 6 choices")]
        public required List<CreateChoiceDto> Choices { get; set; }
    }
}