using System.ComponentModel.DataAnnotations;

namespace tapcet_api.DTO.Choice
{
    public class CreateChoiceDto
    {
        [Required(ErrorMessage = "Choice text is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Choice must be between 1 and 500 characters")]
        public required string Text { get; set; }

        public bool IsCorrect { get; set; } = false;
    }
}