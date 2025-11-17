# Service Layer Quick Reference

## How to Use the Services in Controllers

### Example: Quiz Controller Setup

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using tapcet_api.DTO.Quiz;
using tapcet_api.Services.Interfaces;

namespace tapcet_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all endpoints
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly ILogger<QuizController> _logger;

        public QuizController(IQuizService quizService, ILogger<QuizController> logger)
        {
            _quizService = quizService;
            _logger = logger;
        }

        // Get current user ID from JWT token
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        /// <summary>
        /// Create a new quiz
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")] // Admin only
        [ProducesResponseType(typeof(QuizResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizDto createDto)
        {
            var userId = GetCurrentUserId();
            var result = await _quizService.CreateQuizAsync(createDto, userId);

            if (result == null)
                return BadRequest(new { message = "Failed to create quiz. Check validation." });

            return CreatedAtAction(nameof(GetQuiz), new { id = result.Id }, result);
        }

        /// <summary>
        /// Get quiz by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous] // Public endpoint
        [ProducesResponseType(typeof(QuizResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetQuiz(int id)
        {
            var result = await _quizService.GetQuizByIdAsync(id);

            if (result == null)
                return NotFound(new { message = $"Quiz {id} not found" });

            return Ok(result);
        }

        /// <summary>
        /// Get all active quizzes
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<QuizSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveQuizzes()
        {
            var result = await _quizService.GetActiveQuizzesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Update quiz
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(QuizResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateQuiz(int id, [FromBody] UpdateQuizDto updateDto)
        {
            var userId = GetCurrentUserId();
            var result = await _quizService.UpdateQuizAsync(id, updateDto, userId);

            if (result == null)
                return NotFound(new { message = "Quiz not found or permission denied" });

            return Ok(result);
        }

        /// <summary>
        /// Delete quiz
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _quizService.DeleteQuizAsync(id, userId);

            if (!result)
                return NotFound(new { message = "Quiz not found or permission denied" });

            return NoContent();
        }

        /// <summary>
        /// Get quizzes created by current user
        /// </summary>
        [HttpGet("my")]
        [ProducesResponseType(typeof(List<QuizSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyQuizzes()
        {
            var userId = GetCurrentUserId();
            var result = await _quizService.GetUserCreatedQuizzesAsync(userId);
            return Ok(result);
        }
    }
}
```

---

### Example: Quiz Attempt Controller Setup

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using tapcet_api.DTO.Attempt;
using tapcet_api.Services.Interfaces;

namespace tapcet_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuizAttemptController : ControllerBase
    {
        private readonly IQuizAttemptService _attemptService;
        private readonly ILogger<QuizAttemptController> _logger;

        public QuizAttemptController(
            IQuizAttemptService attemptService,
            ILogger<QuizAttemptController> logger)
        {
            _attemptService = attemptService;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        /// <summary>
        /// Start a new quiz attempt
        /// </summary>
        [HttpPost("quiz/{quizId}/start")]
        [ProducesResponseType(typeof(QuizAttemptResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> StartQuiz(int quizId)
        {
            var userId = GetCurrentUserId();
            var result = await _attemptService.StartQuizAttemptAsync(quizId, userId);

            if (result == null)
                return BadRequest(new { message = "Quiz not found or inactive" });

            return Ok(result);
        }

        /// <summary>
        /// Submit quiz answers
        /// </summary>
        [HttpPost("submit")]
        [ProducesResponseType(typeof(QuizResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizDto submitDto)
        {
            var userId = GetCurrentUserId();
            var result = await _attemptService.SubmitQuizAsync(submitDto, userId);

            if (result == null)
                return BadRequest(new { message = "Invalid submission or attempt already completed" });

            return Ok(result);
        }

        /// <summary>
        /// Get attempt result
        /// </summary>
        [HttpGet("{attemptId}/result")]
        [ProducesResponseType(typeof(QuizResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetResult(int attemptId)
        {
            var userId = GetCurrentUserId();
            var result = await _attemptService.GetAttemptResultAsync(attemptId, userId);

            if (result == null)
                return NotFound(new { message = "Result not found or not completed" });

            return Ok(result);
        }

        /// <summary>
        /// Get user's quiz attempts
        /// </summary>
        [HttpGet("my")]
        [ProducesResponseType(typeof(List<QuizAttemptResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyAttempts()
        {
            var userId = GetCurrentUserId();
            var result = await _attemptService.GetUserAttemptsAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Get quiz leaderboard
        /// </summary>
        [HttpGet("quiz/{quizId}/leaderboard")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<QuizAttemptResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLeaderboard(int quizId, [FromQuery] int top = 10)
        {
            var result = await _attemptService.GetQuizLeaderboardAsync(quizId, top);
            return Ok(result);
        }
    }
}
```

---

## Service Methods Reference

### IQuizService

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `CreateQuizAsync` | `CreateQuizDto`, `userId` | `QuizResponseDto?` | Create new quiz |
| `GetQuizByIdAsync` | `quizId` | `QuizResponseDto?` | Get quiz details |
| `GetAllQuizzesAsync` | - | `List<QuizSummaryDto>` | Get all quizzes |
| `GetActiveQuizzesAsync` | - | `List<QuizSummaryDto>` | Get active quizzes |
| `UpdateQuizAsync` | `quizId`, `UpdateQuizDto`, `userId` | `QuizResponseDto?` | Update quiz |
| `DeleteQuizAsync` | `quizId`, `userId` | `bool` | Delete quiz |
| `ToggleQuizStatusAsync` | `quizId`, `userId` | `bool` | Toggle active status |
| `AddQuestionToQuizAsync` | `quizId`, `CreateQuestionDto`, `userId` | `QuizResponseDto?` | Add question |
| `RemoveQuestionFromQuizAsync` | `quizId`, `questionId`, `userId` | `bool` | Remove question |
| `GetUserCreatedQuizzesAsync` | `userId` | `List<QuizSummaryDto>` | Get user's quizzes |

### IQuizAttemptService

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `StartQuizAttemptAsync` | `quizId`, `userId` | `QuizAttemptResponseDto?` | Start quiz |
| `SubmitQuizAsync` | `SubmitQuizDto`, `userId` | `QuizResultDto?` | Submit answers |
| `GetAttemptByIdAsync` | `attemptId`, `userId` | `QuizAttemptResponseDto?` | Get attempt |
| `GetUserAttemptsAsync` | `userId` | `List<QuizAttemptResponseDto>` | Get user attempts |
| `GetQuizAttemptsAsync` | `quizId` | `List<QuizAttemptResponseDto>` | Get quiz attempts |
| `GetAttemptResultAsync` | `attemptId`, `userId` | `QuizResultDto?` | Get detailed result |
| `GetQuizLeaderboardAsync` | `quizId`, `topCount` | `List<QuizAttemptResponseDto>` | Get leaderboard |

---

## Common Patterns

### Getting Current User ID
```csharp
private string GetCurrentUserId()
{
    return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
}
```

### Checking User Role
```csharp
var isAdmin = User.IsInRole("Admin");
```

### Standard Response Patterns
```csharp
// Success
return Ok(result);

// Created
return CreatedAtAction(nameof(GetQuiz), new { id = result.Id }, result);

// Not Found
return NotFound(new { message = "Resource not found" });

// Bad Request
return BadRequest(new { message = "Validation failed" });

// No Content (delete)
return NoContent();

// Forbidden
return Forbid();
```

---

## Testing Services

### Unit Test Example (with Moq)

```csharp
[Fact]
public async Task CreateQuizAsync_ValidQuiz_ReturnsQuizResponseDto()
{
    // Arrange
    var mockContext = new Mock<ApplicationDbContext>();
    var mockMapper = new Mock<IMapper>();
    var mockLogger = new Mock<ILogger<QuizService>>();
    
    var service = new QuizService(
        mockContext.Object,
        mockMapper.Object,
        mockLogger.Object
    );
    
    var createDto = new CreateQuizDto
    {
        Title = "Test Quiz",
        Questions = new List<CreateQuestionDto>
        {
            new CreateQuestionDto
            {
                Text = "Question 1",
                Choices = new List<CreateChoiceDto>
                {
                    new CreateChoiceDto { Text = "A", IsCorrect = true },
                    new CreateChoiceDto { Text = "B", IsCorrect = false }
                }
            }
        }
    };
    
    // Act
    var result = await service.CreateQuizAsync(createDto, "user123");
    
    // Assert
    Assert.NotNull(result);
}
```

---

## Next Steps

1. ? Services completed
2. ?? Create controllers (use examples above)
3. ?? Test endpoints with Swagger
4. ?? Add integration tests
5. ?? Frontend integration

---

*Last updated: $(date)*
