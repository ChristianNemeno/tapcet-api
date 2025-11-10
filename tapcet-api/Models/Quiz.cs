using System.ComponentModel.DataAnnotations;

namespace tapcet_api.Models
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }


        [Required]
        [MaxLength(200)]
        public required string Title { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        public required string CreatedById { get; set; }
        public User? CreatedBy { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Question> Questions { get; set; } = new List<Question>();

        public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    }
}
