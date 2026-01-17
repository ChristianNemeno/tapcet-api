using System.ComponentModel.DataAnnotations;

namespace tapcet_api.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        public ICollection<Unit> Units { get; set; } = new List<Unit>();
    }
}
