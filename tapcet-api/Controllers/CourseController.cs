using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using tapcet_api.DTO.Course;
using tapcet_api.Services.Interfaces;

namespace tapcet_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ILogger<CourseController> _logger;

        public CourseController(ICourseService courseService, ILogger<CourseController> logger)
        {
            _courseService = courseService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _courseService.CreateCourseAsync(createDto);

            if (result == null)
            {
                return BadRequest(new { message = "Failed to create course. Subject may not exist." });
            }

            _logger.LogInformation("Course created: {CourseId}", result.Id);
            return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [DisableRateLimiting]
        [ProducesResponseType(typeof(CourseWithUnitsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var result = await _courseService.GetCourseWithUnitsAsync(id);

            if (result == null)
            {
                return NotFound(new { message = $"Course with ID {id} not found" });
            }

            return Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        [DisableRateLimiting]
        [ProducesResponseType(typeof(List<CourseResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCourses()
        {
            var result = await _courseService.GetAllCoursesAsync();
            return Ok(result);
        }

        [HttpGet("subject/{subjectId}")]
        [AllowAnonymous]
        [DisableRateLimiting]
        [ProducesResponseType(typeof(List<CourseResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCoursesBySubject(int subjectId)
        {
            var result = await _courseService.GetCoursesBySubjectAsync(subjectId);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _courseService.UpdateCourseAsync(id, updateDto);

            if (result == null)
            {
                return BadRequest(new { message = "Failed to update course. Course or subject may not exist." });
            }

            _logger.LogInformation("Course updated: {CourseId}", id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var result = await _courseService.DeleteCourseAsync(id);

            if (!result)
            {
                _logger.LogWarning("Failed to delete course {CourseId}", id);
                return BadRequest(new { message = "Cannot delete course. Course not found or has existing units." });
            }

            _logger.LogInformation("Course deleted: {CourseId}", id);
            return NoContent();
        }
    }
}
