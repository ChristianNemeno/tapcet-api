using System.ComponentModel.DataAnnotations;

namespace tapcet_api.DTO.Quiz
{
    public class UpdateQuizDto
    {
        [Required(ErrorMessage = "Quiz title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public required string Title { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        public int? UnitId { get; set; }

        [Range(1, 999, ErrorMessage = "Order index must be between 1 and 999")]
        public int OrderIndex { get; set; } = 1;

        public bool IsActive { get; set; } = true;
    }
}