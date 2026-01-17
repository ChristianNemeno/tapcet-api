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

        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Choice> Choices { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        
        // Educational Hierarchy
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Unit> Units { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
                
            builder.Entity<Quiz>()
                .HasOne(q => q.CreatedBy)
                .WithMany(u => u.CreatedQuizzes)
                .HasForeignKey(q => q.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Choice>()
               .HasOne(c => c.Question)
               .WithMany(q => q.Choices)
               .HasForeignKey(c => c.QuestionId)
               .OnDelete(DeleteBehavior.Cascade);


            // quiz attempt
            builder.Entity<QuizAttempt>()
                .HasOne(qa => qa.User)
                .WithMany(u => u.QuizAttempts)
                .HasForeignKey(qa => qa.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<QuizAttempt>()
                .HasOne(qa => qa.Quiz)
                .WithMany(q => q.QuizAttempts)
                .HasForeignKey(qa => qa.QuizId)
                .OnDelete(DeleteBehavior.Restrict);


            //user ans relationsips
            builder.Entity<UserAnswer>()
                .HasOne(ua => ua.QuizAttempt)
                .WithMany(qa => qa.UserAnswers)
                .HasForeignKey(ua => ua.QuizAttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserAnswer>()
                .HasOne(ua => ua.Question)
                .WithMany(q => q.UserAnswers)
                .HasForeignKey(ua => ua.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserAnswer>()
                .HasOne(ua => ua.Choice)
                .WithMany(c => c.UserAnswers)
                .HasForeignKey(ua => ua.ChoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Educational Hierarchy Relationships
            
            // Subject → Course (One-to-Many)
            builder.Entity<Course>()
                .HasOne(c => c.Subject)
                .WithMany(s => s.Courses)
                .HasForeignKey(c => c.SubjectId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Subject with Courses
            
            // Course → Unit (One-to-Many)
            builder.Entity<Unit>()
                .HasOne(u => u.Course)
                .WithMany(c => c.Units)
                .HasForeignKey(u => u.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // Delete Units when Course is deleted
            
            // Unit → Quiz (One-to-Many, Optional)
            builder.Entity<Quiz>()
                .HasOne(q => q.Unit)
                .WithMany(u => u.Quizzes)
                .HasForeignKey(q => q.UnitId)
                .OnDelete(DeleteBehavior.SetNull); // Keep Quiz if Unit is deleted, but clear UnitId
        }
    }
}
