
using Microsoft.AspNetCore.Identity;

namespace tapcet_api.Models
{
    public class User : IdentityUser
    {
        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
        public int TotalQuizAttempts { get; set; } = 0;
        public double AverageScore { get; set; } = 0.0;
        
        public ICollection<Quiz> CreatedQuizzes { get; set; } = new List<Quiz>();
        public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    }
}

