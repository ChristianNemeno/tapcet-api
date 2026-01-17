using System.ComponentModel.DataAnnotations;

namespace tapcet_api.Models
{
    public class Unit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Title { get; set; }

        public int OrderIndex { get; set; }

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }
}
