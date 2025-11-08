using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using tapcet_api.Models;

namespace tapcet_api.Data   
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for quiz-related entities will be added later
        // public DbSet<Quiz> Quizzes { get; set; }
        // public DbSet<Question> Questions { get; set; }
        // public DbSet<Choice> Choices { get; set; }
        // public DbSet<QuizAttempt> QuizAttempts { get; set; }
        // public DbSet<UserAnswer> UserAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Additional configurations will be added here
            // when we implement quiz models
        }
    }
}
