namespace tapcet_api.DTO.Attempt
{
    public class QuestionResultDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public int SelectedChoiceId { get; set; }
        public string SelectedChoiceText { get; set; } = string.Empty;
        public int CorrectChoiceId { get; set; }
        public string CorrectChoiceText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}