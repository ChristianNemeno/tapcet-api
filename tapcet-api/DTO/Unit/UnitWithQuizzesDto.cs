using tapcet_api.DTO.Quiz;

namespace tapcet_api.DTO.Unit
{
    public class UnitWithQuizzesDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public List<QuizSummaryDto> Quizzes { get; set; } = new();
    }
}
