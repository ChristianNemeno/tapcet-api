namespace tapcet_api.DTO.Attempt
{
    public class QuizResultDto
    {
        public int QuizAttemptId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public int Score { get; set; }
        public double Percentage { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset CompletedAt { get; set; }
        public TimeSpan Duration { get; set; }
        public List<QuestionResultDto> QuestionResults { get; set; } = new();
    }
}