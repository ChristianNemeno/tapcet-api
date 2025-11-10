using System.ComponentModel.DataAnnotations;
using tapcet_api.DTO.Question;

namespace tapcet_api.DTO.Quiz
{
    public class CreateQuizDto
    {
        [Required(ErrorMessage ="Quiz title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public required string Title { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "At least one question is required")]
        [MinLength(1, ErrorMessage = "Quiz must have at least one question")]
        public required List<CreateQuestionDto> Questions { get; set; }
    }
}
