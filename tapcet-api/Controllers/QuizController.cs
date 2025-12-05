using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using tapcet_api.DTO.Quiz;
using tapcet_api.Services.Implementations;

namespace tapcet_api.Controllers
{

    [Route("/api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuizController : ControllerBase
    {

        //mga dependencies 
        private readonly QuizService _quizService;
        private readonly ILogger<QuizController> _logger;

        public QuizController(QuizService quizService, ILogger<QuizController> logger)
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


        [HttpGet]
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
    }
}
