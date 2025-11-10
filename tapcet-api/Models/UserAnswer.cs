using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tapcet_api.Models
{
    public class UserAnswer
    {
        [Key]
        public int Id { get; set; }

        
        [Required]
        public int QuizAttemptId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        public int ChoiceId { get; set; }

        public DateTimeOffset AnsweredAt { get; set; } = DateTimeOffset.UtcNow;

        
        [ForeignKey("QuizAttemptId")]
        public QuizAttempt? QuizAttempt { get; set; }

        [ForeignKey("QuestionId")]
        public Question? Question { get; set; }

        [ForeignKey("ChoiceId")]
        public Choice? Choice { get; set; }




    }
}
