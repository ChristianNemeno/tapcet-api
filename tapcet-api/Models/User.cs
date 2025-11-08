
using Microsoft.AspNetCore.Identity;

namespace tapcet_api.Models
{
    public class User : IdentityUser
    {
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int TotalQuizAttempts { get; set; } = 0;
        public double AverageScore { get; set; } = 0.0;

    }
}
