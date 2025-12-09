using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using tapcet_api.DTO.Quiz;
using tapcet_api.Services.Implementations;
using tapcet_api.Services.Interfaces;

namespace tapcet_api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuizController : ControllerBase
    {

        //mga dependencies 
        private readonly IQuizService _quizService;
        private readonly ILogger<QuizController> _logger;

        public QuizController(IQuizService quizService, ILogger<QuizController> logger)
        {
            _quizService = quizService;
            _logger = logger;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpPost]
        [ProducesResponseType(typeof(QuizResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizDto createDto)
        {
            //validation first , depending on the business rules, then call service to create

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var result = await _quizService.CreateQuizAsync(createDto, userId);

            if (result == null)
            {
                return BadRequest(new { message = "Failed to create quiz. Ensure each question has 2-6 choices and exactly one correct answer." });
            }
            _logger.LogInformation("Quiz created: {QuizId} by user {UserId}", result.Id, userId);
            return CreatedAtAction(nameof(GetQuizById), new { id = result.Id }, result);
        }


        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(QuizResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<IActionResult> GetQuizById(int id)
        {
            var result = await _quizService.GetQuizByIdAsync(id);

            if (result == null)
            {
                return NotFound(new { message = $"Quiz with ID {id} not found" });
            }

            return Ok(result);


        }

        [HttpGet]
        [ProducesResponseType(typeof(List<QuizSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllQuizzes()
        {
            var result = await _quizService.GetAllQuizzesAsync();
            return Ok(result);
        }

        [HttpGet("active")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<QuizSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveQuizzes()
        {
            var result = await _quizService.GetActiveQuizzesAsync();
            return Ok(result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(QuizResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateQuiz(int id, [FromBody] UpdateQuizDto updateQuizDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var result = await _quizService.UpdateQuizAsync(id, updateQuizDto, userId);

            if (result == null)
            {
                return NotFound(new { message = "Quiz not found" });
            }

            _logger.LogInformation("Updated quiz {QuizId}", id);

            return Ok(result);

        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteQuiz(int id)
        {


            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var result = await _quizService.DeleteQuizAsync(id, userId);

            if (result == false)
            {
                _logger.LogWarning("Failed to delete quiz {QuizId} by user {UserId}", id, userId);
                return StatusCode(StatusCodes.Status404NotFound);
            }

            _logger.LogInformation("Deleted quiz {QuizId} by user {UserId}", id, userId);
            return NoContent();

        }


        [HttpPatch("{id}/toggle")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleQuiz(int id)
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var result = await _quizService.ToggleQuizStatusAsync(id, userId);

            if (result == false)
            {
                _logger.LogWarning("Failed to toggle quiz {QuizId} by user {UserId}", id, userId);

                return NotFound();
            }
            _logger.LogInformation("Quiz status toggled: {QuizId} by user {UserId}", id, userId);

            return Ok(new { message = "Status toggled" });

        }


        [HttpPost("{id}/questions")]
        [ProducesResponseType(typeof(QuizResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddQuestion(int id, [FromBody] CreateQuestionDto createQuestionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var result = await _quizService.AddQuestionToQuizAsync(id, createQuestionDto, userId);

            if (result == null)
            {
                return BadRequest();
            }
            _logger.LogInformation("Question added to quiz {QuizId} by user {UserId}", id, userId);
            return Ok(result);

        }
    }
}
