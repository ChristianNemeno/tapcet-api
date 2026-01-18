using System.ComponentModel.DataAnnotations;

namespace tapcet_api.DTO.Course
{
    public class UpdateCourseDto
    {
        [Required(ErrorMessage = "Course title is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
        public required string Title { get; set; }
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Subject ID is required")]
        public int SubjectId { get; set; }
    }
}
