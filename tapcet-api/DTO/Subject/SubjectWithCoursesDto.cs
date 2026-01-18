using tapcet_api.DTO.Course;

namespace tapcet_api.DTO.Subject
{
    public class SubjectWithCoursesDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<CourseResponseDto> Courses { get; set; } = new();
    }
}
