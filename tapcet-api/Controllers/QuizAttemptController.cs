using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using tapcet_api.DTO.Attempt;
using tapcet_api.Services.Interfaces;

namespace tapcet_api.Controllers
{
    [Route("api/quiz-attempt")]
    [ApiController]
    [Authorize]
    public class QuizAttemptController : ControllerBase
    {
        private readonly IQuizAttemptService _attemptService;
        private readonly ILogger<QuizAttemptController> _logger;

        public QuizAttemptController(IQuizAttemptService attemptService, ILogger<QuizAttemptController> logger)
        {
            _attemptService = attemptService;
            _logger = logger;
        }

        /// <summary>
        /// Helper method to extract user ID from JWT claims
        /// </summary>
        /// <returns>User ID or null</returns>
        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Start a new quiz attempt
        /// </summary>
        /// <param name="startDto">Contains the quiz ID to attempt</param>
        /// <returns>The created attempt details</returns>
        [HttpPost("start")]
        [ProducesResponseType(typeof(QuizAttemptResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> StartQuizAttempt([FromBody] StartQuizAttemptDto startDto)
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

            var result = await _attemptService.StartQuizAttemptAsync(startDto.QuizId, userId);

            if (result == null)
            {
                _logger.LogWarning("Failed to start quiz attempt for quiz {QuizId} by user {UserId}",
                    startDto.QuizId, userId);
                return BadRequest(new { message = "Failed to start quiz. Quiz may be inactive, not found, or has no questions." });
            }

            _logger.LogInformation("Quiz attempt started: {AttemptId} for quiz {QuizId} by user {UserId}",
                result.Id, startDto.QuizId, userId);

            return CreatedAtAction(
                nameof(GetAttemptById),
                new { id = result.Id },
                result
            );
        }

        /// <summary>
        /// Submit quiz answers and get results
        /// </summary>
        /// <param name="submitDto">Quiz attempt ID and all answers</param>
        /// <returns>Detailed quiz results with score and answer breakdown</returns>
        [HttpPost("submit")]
        [ProducesResponseType(typeof(QuizResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizDto submitDto)
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

            var result = await _attemptService.SubmitQuizAsync(submitDto, userId);

            if (result == null)
            {
                _logger.LogWarning("Failed to submit quiz attempt {AttemptId} by user {UserId}",
                    submitDto.QuizAttemptId, userId);
                return BadRequest(new { 
                    message = "Failed to submit quiz. Ensure you've answered all questions and haven't already submitted this attempt." 
                });
            }

            _logger.LogInformation("Quiz submitted: Attempt {AttemptId} with score {Score}% by user {UserId}",
                submitDto.QuizAttemptId, result.Score, userId);

            return Ok(result);
        }

        /// <summary>
        /// Get attempt details by ID
        /// </summary>
        /// <param name="id">Attempt ID</param>
        /// <returns>Attempt details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(QuizAttemptResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAttemptById(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var result = await _attemptService.GetAttemptByIdAsync(id, userId);

            if (result == null)
            {
                _logger.LogWarning("Attempt not found or access denied: {AttemptId} for user {UserId}", id, userId);
                return NotFound(new { message = "Attempt not found or you don't have permission to view it" });
            }

            return Ok(result);
        }

        /// <summary>
        /// Get detailed results for a completed quiz attempt
        /// </summary>
        /// <param name="id">Attempt ID</param>
        /// <returns>Detailed quiz results with question-by-question breakdown</returns>
        [HttpGet("{id}/result")]
        [ProducesResponseType(typeof(QuizResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAttemptResult(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var result = await _attemptService.GetAttemptResultAsync(id, userId);

            if (result == null)
            {
                _logger.LogWarning("Result not found or attempt not completed: Attempt {AttemptId} for user {UserId}",
                    id, userId);
                return BadRequest(new { 
                    message = "Result not found. The attempt may not be completed, doesn't exist, or you don't have permission to view it." 
                });
            }

            return Ok(result);
        }

        /// <summary>
        /// Get all quiz attempts for the current user
        /// </summary>
        /// <returns>List of user's quiz attempts (both completed and in-progress)</returns>
        [HttpGet("user/me")]
        [ProducesResponseType(typeof(List<QuizAttemptResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyAttempts()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var result = await _attemptService.GetUserAttemptsAsync(userId);

            _logger.LogInformation("Retrieved {Count} attempts for user {UserId}", result.Count, userId);

            return Ok(result);
        }

        /// <summary>
        /// Get all completed attempts for a specific quiz
        /// </summary>
        /// <param name="quizId">Quiz ID</param>
        /// <returns>List of completed attempts for the quiz, ordered by score</returns>
        [HttpGet("quiz/{quizId}")]
        [ProducesResponseType(typeof(List<QuizAttemptResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuizAttempts(int quizId)
        {
            var result = await _attemptService.GetQuizAttemptsAsync(quizId);

            _logger.LogInformation("Retrieved {Count} attempts for quiz {QuizId}", result.Count, quizId);

            return Ok(result);
        }

        /// <summary>
        /// Get leaderboard for a quiz
        /// </summary>
        /// <param name="quizId">Quiz ID</param>
        /// <param name="topCount">Number of top results to return (default: 10, max: 100)</param>
        /// <returns>Top performers for the quiz, ranked by score and completion time</returns>
        [HttpGet("quiz/{quizId}/leaderboard")]
        [ProducesResponseType(typeof(List<QuizAttemptResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetLeaderboard(int quizId, [FromQuery] int topCount = 10)
        {
            // Validate topCount
            if (topCount < 1 || topCount > 100)
            {
                return BadRequest(new { message = "topCount must be between 1 and 100" });
            }

            var result = await _attemptService.GetQuizLeaderboardAsync(quizId, topCount);

            _logger.LogInformation("Retrieved top {TopCount} leaderboard entries for quiz {QuizId}", 
                topCount, quizId);

            return Ok(result);
        }
    }
}
