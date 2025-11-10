using System.ComponentModel.DataAnnotations;

namespace tapcet_api.DTO.Attempt
{
    public class SubmitQuizDto
    {
        [Required(ErrorMessage = "Quiz attempt ID is required")]
        public int QuizAttemptId { get; set; }

        [Required(ErrorMessage = "Answers are required")]
        [MinLength(1, ErrorMessage = "At least one answer is required")]
        public required List<SubmitAnswerDto> Answers { get; set; }
    }
}
