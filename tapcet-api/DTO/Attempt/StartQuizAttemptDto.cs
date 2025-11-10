using System.ComponentModel.DataAnnotations;

namespace tapcet_api.DTO.Attempt
{
    public class StartQuizAttemptDto
    {
        [Required(ErrorMessage = "Quiz ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid quiz ID")]
        public int QuizId { get; set; }
    }
}
