using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using tapcet_api.DTO.Subject;
using tapcet_api.Services.Interfaces;

namespace tapcet_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("authenticated")]
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService _subjectService;
        private readonly ILogger<SubjectController> _logger;

        public SubjectController(ISubjectService subjectService, ILogger<SubjectController> logger)
        {
            _subjectService = subjectService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(SubjectResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _subjectService.CreateSubjectAsync(createDto);

            if (result == null)
            {
                return BadRequest(new { message = "Failed to create subject. A subject with this name may already exist." });
            }

            _logger.LogInformation("Subject created: {SubjectId}", result.Id);
            return CreatedAtAction(nameof(GetSubjectById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [DisableRateLimiting]
        [ProducesResponseType(typeof(SubjectWithCoursesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSubjectById(int id)
        {
            var result = await _subjectService.GetSubjectWithCoursesAsync(id);

            if (result == null)
            {
                return NotFound(new { message = $"Subject with ID {id} not found" });
            }

            return Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        [DisableRateLimiting]
        [ProducesResponseType(typeof(List<SubjectResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllSubjects()
        {
            var result = await _subjectService.GetAllSubjectsAsync();
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(SubjectResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] UpdateSubjectDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _subjectService.UpdateSubjectAsync(id, updateDto);

            if (result == null)
            {
                return BadRequest(new { message = "Failed to update subject. Subject not found or name already exists." });
            }

            _logger.LogInformation("Subject updated: {SubjectId}", id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            var result = await _subjectService.DeleteSubjectAsync(id);

            if (!result)
            {
                _logger.LogWarning("Failed to delete subject {SubjectId}", id);
                return BadRequest(new { message = "Cannot delete subject. Subject not found or has existing courses." });
            }

            _logger.LogInformation("Subject deleted: {SubjectId}", id);
            return NoContent();
        }
    }
}
