using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tapcet_api.Models
{
    public class Question
    {

        [Key]
        public int Id { get; set; }

        
        [Required]
        [MaxLength(100)]
        public required string Text { get; set; }

        [MaxLength(300)]
        public string? Explanation { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        public int QuizId { get; set; }

        [ForeignKey("QuizId")]
        public Quiz? Quiz { get; set; }

        public ICollection<Choice> Choices { get; set; } = new List<Choice>();
        
        public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();







    }
}
