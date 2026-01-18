using System.ComponentModel.DataAnnotations;

namespace tapcet_api.DTO.Unit
{
    public class UpdateUnitDto
    {
        [Required(ErrorMessage = "Unit title is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
        public required string Title { get; set; }
        
        [Required(ErrorMessage = "Order index is required")]
        [Range(1, 999, ErrorMessage = "Order index must be between 1 and 999")]
        public int OrderIndex { get; set; }
        
        [Required(ErrorMessage = "Course ID is required")]
        public int CourseId { get; set; }
    }
}
