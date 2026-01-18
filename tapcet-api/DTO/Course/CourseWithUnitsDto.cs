using tapcet_api.DTO.Unit;

namespace tapcet_api.DTO.Course
{
    public class CourseWithUnitsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public List<UnitResponseDto> Units { get; set; } = new();
    }
}
