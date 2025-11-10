using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tapcet_api.Models
{
    public class Choice
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public required string Text { get; set; }

        public bool IsCorrect { get; set; } = false;


        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public Question? Question { get; set; }


        public  ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();


    }
}
