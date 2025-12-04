# Implementation Guide

## Overview

This guide provides step-by-step instructions to complete the TAPCET Quiz API implementation. The service layer is complete, but controllers need to be implemented.

---

## Current Implementation Status

### ? Completed Components

1. **Models** - All entity classes defined
2. **Database Context** - ApplicationDbContext configured with relationships
3. **Data Transfer Objects (DTOs)** - All request/response DTOs defined
4. **AutoMapper Profiles** - All mapping configurations
5. **Service Layer** - All business logic implemented:
   - `AuthService` - Authentication and registration
   - `QuizService` - Quiz CRUD operations
   - `QuizAttemptService` - Quiz attempt management
6. **Database Migrations** - Schema created
7. **Program.cs** - Dependency injection and middleware configured
8. **Authentication** - JWT configuration complete

### ?? Missing Components

1. **QuizController** - Not implemented
2. **QuizAttemptController** - Not implemented
3. **CORS Policy** - Should be added for frontend integration
4. **Input Validation Middleware** - Optional enhancement
5. **Exception Handling Middleware** - Optional enhancement
6. **Unit Tests** - Not implemented
7. **Integration Tests** - Not implemented

---

## Step 1: Implement QuizController

### Location
Create file: `tapcet-api/Controllers/QuizController.cs`

### Dependencies
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using tapcet_api.DTO.Quiz;
using tapcet_api.DTO.Question;
using tapcet_api.Services.Interfaces;
```

### Implementation

```csharp
namespace tapcet_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All endpoints require authentication
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly ILogger<QuizController> _logger;

        public QuizController(IQuizService quizService, ILogger<QuizController> logger)
        {
            _quizService = quizService;
            _logger = logger;
        }

        // Helper method to get current user ID
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        /// <summary>
        /// Create a new quiz
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(QuizResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var result = await _quizService.CreateQuizAsync(createDto, userId);

            if (result == null)
                return BadRequest(new { message = "Failed to create quiz. Check validation rules." });

            _logger.LogInformation("Quiz created: {QuizId} by user {UserId}", result.Id, userId);
            return CreatedAtAction(nameof(GetQuiz), new { id = result.Id }, result);
        }

        /// <summary>
        /// Get quiz by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(QuizResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetQuiz(int id)
        {
            var result = await _quizService.GetQuizByIdAsync(id);

            if (result == null)
                return NotFound(new { message = $"Quiz with ID {id} not found." });

            return Ok(result);
        }

        /// <summary>
        /// Get all quizzes
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<QuizSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllQuizzes()
        {
            var result = await _quizService.GetAllQuizzesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get only active quizzes
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(List<QuizSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveQuizzes()
        {
            var result = await _quizService.GetActiveQuizzesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Update quiz metadata
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(QuizResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateQuiz(int id, [FromBody] UpdateQuizDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var result = await _quizService.UpdateQuizAsync(id, updateDto, userId);

            if (result == null)
                return NotFound(new { message = $"Quiz with ID {id} not found or you don't have permission." });

            _logger.LogInformation("Quiz updated: {QuizId} by user {UserId}", id, userId);
            return Ok(result);
        }

        /// <summary>
        /// Delete quiz
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _quizService.DeleteQuizAsync(id, userId);

            if (!result)
                return NotFound(new { message = $"Quiz with ID {id} not found or you don't have permission." });

            _logger.LogInformation("Quiz deleted: {QuizId} by user {UserId}", id, userId);
            return NoContent();
        }

        /// <summary>
        /// Toggle quiz active status
        /// </summary>
        [HttpPatch("{id}/toggle")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleQuizStatus(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _quizService.ToggleQuizStatusAsync(id, userId);

            if (!result)
                return NotFound(new { message = $"Quiz with ID {id} not found or you don't have permission." });

            _logger.LogInformation("Quiz status toggled: {QuizId} by user {UserId}", id, userId);
            return Ok(new { message = "Quiz status toggled successfully." });
        }

        /// <summary>
        /// Add question to existing quiz
        /// </summary>
        [HttpPost("{id}/questions")]
        [ProducesResponseType(typeof(QuizResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddQuestion(int id, [FromBody] CreateQuestionDto questionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var result = await _quizService.AddQuestionToQuizAsync(id, questionDto, userId);

            if (result == null)
                return NotFound(new { message = $"Quiz with ID {id} not found or you don't have permission." });

            _logger.LogInformation("Question added to quiz {QuizId} by user {UserId}", id, userId);
            return Ok(result);
        }
    }
}
```

---

## Step 2: Implement QuizAttemptController

### Location
Create file: `tapcet-api/Controllers/QuizAttemptController.cs`

### Implementation

```csharp
namespace tapcet_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttemptController : ControllerBase
    {
        private readonly IQuizAttemptService _attemptService;
        private readonly ILogger<AttemptController> _logger;

        public AttemptController(IQuizAttemptService attemptService, ILogger<AttemptController> logger)
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
        [HttpPost("start")]
        [ProducesResponseType(typeof(QuizAttemptResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> StartQuizAttempt([FromBody] StartQuizAttemptDto startDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var result = await _attemptService.StartQuizAttemptAsync(startDto.QuizId, userId);

            if (result == null)
                return BadRequest(new { message = "Failed to start quiz attempt. Quiz may be inactive or not found." });

            _logger.LogInformation("Quiz attempt started: {AttemptId} by user {UserId}", result.Id, userId);
            return CreatedAtAction(nameof(GetAttempt), new { id = result.Id }, result);
        }

        /// <summary>
        /// Submit quiz answers and get results
        /// </summary>
        [HttpPost("submit")]
        [ProducesResponseType(typeof(QuizResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizDto submitDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var result = await _attemptService.SubmitQuizAsync(submitDto, userId);

            if (result == null)
                return BadRequest(new { message = "Failed to submit quiz. Check that all questions are answered." });

            _logger.LogInformation("Quiz submitted: {AttemptId} with score {Score}", result.QuizAttemptId, result.Score);
            return Ok(result);
        }

        /// <summary>
        /// Get attempt by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(QuizAttemptResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAttempt(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _attemptService.GetAttemptByIdAsync(id, userId);

            if (result == null)
                return NotFound(new { message = $"Attempt with ID {id} not found." });

            return Ok(result);
        }

        /// <summary>
        /// Get all attempts for current user
        /// </summary>
        [HttpGet("user")]
        [ProducesResponseType(typeof(List<QuizAttemptResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserAttempts()
        {
            var userId = GetCurrentUserId();
            var result = await _attemptService.GetUserAttemptsAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Get all attempts for a specific quiz
        /// </summary>
        [HttpGet("quiz/{quizId}")]
        [ProducesResponseType(typeof(List<QuizAttemptResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuizAttempts(int quizId)
        {
            var result = await _attemptService.GetQuizAttemptsAsync(quizId);
            return Ok(result);
        }

        /// <summary>
        /// Get detailed results for a completed attempt
        /// </summary>
        [HttpGet("{id}/result")]
        [ProducesResponseType(typeof(QuizResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAttemptResult(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _attemptService.GetAttemptResultAsync(id, userId);

            if (result == null)
                return NotFound(new { message = $"Result for attempt {id} not found or attempt not completed." });

            return Ok(result);
        }

        /// <summary>
        /// Get leaderboard for a quiz
        /// </summary>
        [HttpGet("quiz/{quizId}/leaderboard")]
        [ProducesResponseType(typeof(List<QuizAttemptResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLeaderboard(int quizId, [FromQuery] int topCount = 10)
        {
            if (topCount < 1 || topCount > 100)
                return BadRequest(new { message = "topCount must be between 1 and 100." });

            var result = await _attemptService.GetQuizLeaderboardAsync(quizId, topCount);
            return Ok(result);
        }
    }
}
```

---

## Step 3: Add CORS Policy (Optional but Recommended)

### Update Program.cs

Add before `var app = builder.Build();`:

```csharp
// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:4200") // Add your frontend URLs
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

Add after `app.UseHttpsRedirection();`:

```csharp
app.UseCors("AllowFrontend");
```

---

## Step 4: Add Global Exception Handler (Optional)

### Create Middleware

Create file: `tapcet-api/Middleware/ExceptionHandlingMiddleware.cs`

```csharp
namespace tapcet_api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = new
            {
                message = "An unexpected error occurred. Please try again later.",
                detail = exception.Message // Remove in production
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
```

### Register in Program.cs

Add before `app.UseHttpsRedirection();`:

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

---

## Step 5: Testing

### Manual Testing with Swagger

1. **Start the application**
   ```bash
   dotnet run
   ```

2. **Navigate to Swagger UI**
   ```
   https://localhost:{port}/swagger
   ```

3. **Test Authentication**
   - Register a new user
   - Login and copy the JWT token
   - Click "Authorize" and enter: `Bearer {token}`

4. **Test Quiz Creation**
   - Use POST /api/quiz
   - Create a quiz with 2-3 questions

5. **Test Quiz Attempt**
   - Use POST /api/attempt/start
   - Use POST /api/attempt/submit
   - Check results

### Unit Testing Setup

Install testing packages:
```bash
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package xUnit
dotnet add package xUnit.runner.visualstudio
dotnet add package Moq
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

### Sample Unit Test

Create file: `tapcet-api.Tests/Services/QuizServiceTests.cs`

```csharp
public class QuizServiceTests
{
    private readonly Mock<ILogger<QuizService>> _mockLogger;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public QuizServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        
        var config = new MapperConfiguration(cfg => {
            cfg.AddProfile<QuizProfile>();
            cfg.AddProfile<QuestionProfile>();
            cfg.AddProfile<ChoiceProfile>();
        });
        _mapper = config.CreateMapper();
        
        _mockLogger = new Mock<ILogger<QuizService>>();
    }

    [Fact]
    public async Task CreateQuizAsync_ValidQuiz_ReturnsQuizResponse()
    {
        // Arrange
        var service = new QuizService(_context, _mapper, _mockLogger.Object);
        var createDto = new CreateQuizDto
        {
            Title = "Test Quiz",
            Description = "Test Description",
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
        Assert.Equal("Test Quiz", result.Title);
        Assert.Equal(1, result.QuestionCount);
    }
}
```

---

## Step 6: Database Seeding (Optional)

### Create Seed Data

Create file: `tapcet-api/Data/SeedData.cs`

```csharp
namespace tapcet_api.Data
{
    public static class SeedData
    {
        public static async Task SeedQuizzesAsync(ApplicationDbContext context, string adminUserId)
        {
            if (await context.Quizzes.AnyAsync())
                return; // Already seeded

            var quiz = new Quiz
            {
                Title = "C# Basics Quiz",
                Description = "Test your knowledge of C# fundamentals",
                CreatedById = adminUserId,
                IsActive = true,
                Questions = new List<Question>
                {
                    new Question
                    {
                        Text = "What is the latest version of C#?",
                        Explanation = "C# 12 was released with .NET 8",
                        Choices = new List<Choice>
                        {
                            new Choice { Text = "C# 10", IsCorrect = false },
                            new Choice { Text = "C# 11", IsCorrect = false },
                            new Choice { Text = "C# 12", IsCorrect = true },
                            new Choice { Text = "C# 13", IsCorrect = false }
                        }
                    },
                    new Question
                    {
                        Text = "Which keyword is used for inheritance?",
                        Explanation = "The colon (:) is used for inheritance in C#",
                        Choices = new List<Choice>
                        {
                            new Choice { Text = "extends", IsCorrect = false },
                            new Choice { Text = ":", IsCorrect = true },
                            new Choice { Text = "inherits", IsCorrect = false }
                        }
                    }
                }
            };

            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();
        }
    }
}
```

### Call in Program.cs

Add after role and admin user seeding:

```csharp
await SeedData.SeedQuizzesAsync(services.GetRequiredService<ApplicationDbContext>(), adminUser.Id);
```

---

## Step 7: Deployment Preparation

### Update appsettings.Production.json

Create file: `tapcet-api/appsettings.Production.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your-Production-Connection-String"
  },
  "JwtSettings": {
    "SecretKey": "Use-A-Long-Secure-Random-Key-Here",
    "Issuer": "TapcetAPI",
    "Audience": "TapcetClient",
    "ExpiryInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Environment Variables

For production, use environment variables instead of appsettings:

```bash
export ConnectionStrings__DefaultConnection="your-connection-string"
export JwtSettings__SecretKey="your-secret-key"
```

---

## Step 8: Build and Run

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run
```

### Publish
```bash
dotnet publish -c Release -o ./publish
```

---

## Testing Checklist

- [ ] User can register
- [ ] User can login and receive token
- [ ] User can create quiz
- [ ] User can view quizzes
- [ ] User can update own quiz
- [ ] User cannot update others' quiz
- [ ] User can start quiz attempt
- [ ] User can submit quiz answers
- [ ] Score is calculated correctly
- [ ] User can view attempt results
- [ ] Leaderboard shows correct ranking
- [ ] User statistics update correctly

---

## Common Issues & Solutions

### Issue: CORS errors in frontend
**Solution:** Add proper CORS policy in Program.cs

### Issue: JWT token not working
**Solution:** Ensure token is in format: `Bearer {token}`

### Issue: DbContext errors
**Solution:** Run migrations: `dotnet ef database update`

### Issue: AutoMapper errors
**Solution:** Ensure all profiles are registered in Program.cs

---

## Next Steps

1. Implement controllers (Step 1 & 2)
2. Test all endpoints
3. Add unit tests
4. Add integration tests
5. Implement CORS (Step 3)
6. Add global exception handler (Step 4)
7. Prepare for deployment (Step 7)

---

**Last Updated**: 2024
**Version**: 1.0.0
