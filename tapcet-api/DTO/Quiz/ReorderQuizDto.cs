using System.ComponentModel.DataAnnotations;

namespace tapcet_api.DTO.Quiz
{
    public class ReorderQuizDto
    {
        [Required(ErrorMessage = "Order index is required")]
        [Range(1, 999, ErrorMessage = "Order index must be between 1 and 999")]
        public int OrderIndex { get; set; }
    }
}
