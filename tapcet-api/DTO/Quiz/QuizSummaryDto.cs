namespace tapcet_api.DTO.Quiz
{
    public class QuizSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int QuestionCount { get; set; }
        public int AttemptCount { get; set; }
    }
}