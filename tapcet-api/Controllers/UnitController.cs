using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using tapcet_api.DTO.Quiz;
using tapcet_api.DTO.Unit;
using tapcet_api.Services.Interfaces;

namespace tapcet_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    public class UnitController : ControllerBase
    {
        private readonly IUnitService _unitService;
        private readonly ILogger<UnitController> _logger;

        public UnitController(IUnitService unitService, ILogger<UnitController> logger)
        {
            _unitService = unitService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(UnitResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateUnit([FromBody] CreateUnitDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _unitService.CreateUnitAsync(createDto);

            if (result == null)
            {
                return BadRequest(new { message = "Failed to create unit. Course may not exist or order index is already in use." });
            }

            _logger.LogInformation("Unit created: {UnitId}", result.Id);
            return CreatedAtAction(nameof(GetUnitById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [DisableRateLimiting]
        [ProducesResponseType(typeof(UnitWithQuizzesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUnitById(int id)
        {
            var result = await _unitService.GetUnitWithQuizzesAsync(id);

            if (result == null)
            {
                return NotFound(new { message = $"Unit with ID {id} not found" });
            }

            return Ok(result);
        }

        [HttpGet("course/{courseId}")]
        [AllowAnonymous]
        [DisableRateLimiting]
        [ProducesResponseType(typeof(List<UnitResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnitsByCourse(int courseId)
        {
            var result = await _unitService.GetUnitsByCourseAsync(courseId);
            return Ok(result);
        }

        [HttpGet("{unitId}/quizzes")]
        [AllowAnonymous]
        [DisableRateLimiting]
        [ProducesResponseType(typeof(List<QuizSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuizzesByUnit(int unitId)
        {
            var result = await _unitService.GetQuizzesByUnitAsync(unitId);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UnitResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUnit(int id, [FromBody] UpdateUnitDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _unitService.UpdateUnitAsync(id, updateDto);

            if (result == null)
            {
                return BadRequest(new { message = "Failed to update unit. Unit or course may not exist, or order index is already in use." });
            }

            _logger.LogInformation("Unit updated: {UnitId}", id);
            return Ok(result);
        }

        [HttpPatch("{id}/reorder")]
        [ProducesResponseType(typeof(UnitResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReorderUnit(int id, [FromBody] ReorderUnitDto reorderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _unitService.ReorderUnitAsync(id, reorderDto.OrderIndex);

            if (result == null)
            {
                return BadRequest(new { message = "Failed to reorder unit. Unit not found or order index is already in use." });
            }

            _logger.LogInformation("Unit reordered: {UnitId} to index {OrderIndex}", id, reorderDto.OrderIndex);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            var result = await _unitService.DeleteUnitAsync(id);

            if (!result)
            {
                _logger.LogWarning("Failed to delete unit {UnitId}", id);
                return BadRequest(new { message = "Cannot delete unit. Unit not found." });
            }

            _logger.LogInformation("Unit deleted: {UnitId} (associated quizzes orphaned)", id);
            return NoContent();
        }
    }
}
