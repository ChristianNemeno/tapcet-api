using tapcet_api.DTO.Choice;

namespace tapcet_api.DTO.Question
{
    public class QuestionResponseDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public string? ImageUrl { get; set; }
        public int QuizId { get; set; }
        public List<ChoiceResponseDto> Choices { get; set; } = new();
    }
}