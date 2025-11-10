using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tapcet_api.Models
{
    public class QuizAttempt
    {
        [Key]
        public int Id { get; set; }
        
        public int Score { get; set; }
        
        public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
        
        public DateTimeOffset? CompletedAt { get; set; }
        
        [Required]
        public required string UserId { get; set; }
        
        [Required]
        public int QuizId { get; set; }
        
        [ForeignKey("UserId")]
        public User? User { get; set; }
        
        [ForeignKey("QuizId")]
        public Quiz? Quiz { get; set; }
        
        public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}