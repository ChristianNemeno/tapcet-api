namespace tapcet_api.DTO.Attempt
{
    public class QuizAttemptResponseDto
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public int Score { get; set; }
        public bool IsCompleted { get; set; }
    }
}