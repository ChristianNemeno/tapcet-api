using System.ComponentModel.DataAnnotations;

namespace tapcet_api.Models
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
