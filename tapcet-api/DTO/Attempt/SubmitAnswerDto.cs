using System.ComponentModel.DataAnnotations;

namespace tapcet_api.DTO.Attempt
{
    public class SubmitAnswerDto
    {
        [Required(ErrorMessage = "Question ID is required")]
        public int QuestionId { get; set; }

        [Required(ErrorMessage = "Choice ID is required")]
        public int ChoiceId { get; set; }
    }
}
