using tapcet_api.DTO.Question;

namespace tapcet_api.DTO.Quiz
{
    public class QuizResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedById { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int QuestionCount { get; set; }
        public List<QuestionResponseDto> Questions { get; set; } = new();
    }
}