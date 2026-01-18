using System.ComponentModel.DataAnnotations;

namespace tapcet_api.DTO.Quiz
{
    public class AssignQuizToUnitDto
    {
        [Required(ErrorMessage = "Unit ID is required")]
        public int UnitId { get; set; }
        
        [Required(ErrorMessage = "Order index is required")]
        [Range(1, 999, ErrorMessage = "Order index must be between 1 and 999")]
        public int OrderIndex { get; set; }
    }
}
