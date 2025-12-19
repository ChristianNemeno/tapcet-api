# Service Layer Documentation

## Overview

The service layer contains business logic and orchestrates data access operations. Services implement specific interfaces and are injected into controllers via dependency injection.

## Service Responsibilities

- Implement business rules and validation
- Orchestrate database operations
- Transform entities to DTOs using AutoMapper
- Handle exceptions and logging
- Manage transactions
- Update related data (e.g., user statistics)

## Authentication Service

### IAuthService

**Location**: `Services/Interfaces/IAuthService.cs`

```csharp
public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
}
```

### AuthService

**Location**: `Services/Implementations/AuthService.cs`

#### RegisterAsync

Registers a new user account.

**Method Signature**:
```csharp
public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
```

**Process**:
1. Create User entity from DTO
2. Hash password using Identity
3. Create user in database
4. Assign "User" role
5. Return null on failure, success message on success

**Returns**: `null` on failure, `AuthResponseDto` on success

**Validation**:
- Email uniqueness (handled by Identity)
- Password requirements (configured in Program.cs)
- Username uniqueness (handled by Identity)

**Error Handling**:
- Logs errors
- Returns null on any failure
- Errors available in Identity.Result.Errors

#### LoginAsync

Authenticates user and generates JWT token.

**Method Signature**:
```csharp
public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
```

**Process**:
1. Find user by email
2. Validate password using Identity
3. Check account lockout
4. Generate JWT token with claims
5. Return token with expiration time

**Returns**: `null` on failure, `AuthResponseDto` with token on success

**JWT Claims**:
- Subject (user ID)
- Email
- JWT ID (unique token identifier)
- Issuer
- Audience
- Expiration time

**Token Configuration**:
- Algorithm: HS256
- Expiration: 60 minutes (configurable)
- Secret key from appsettings.json

## Quiz Service

### IQuizService

**Location**: `Services/Interfaces/IQuizService.cs`

```csharp
public interface IQuizService
{
    Task<QuizResponseDto?> CreateQuizAsync(CreateQuizDto createDto, string userId);
    Task<QuizResponseDto?> GetQuizByIdAsync(int quizId);
    Task<List<QuizSummaryDto>> GetAllQuizzesAsync();
    Task<List<QuizSummaryDto>> GetActiveQuizzesAsync();
    Task<List<QuizSummaryDto>> GetUserCreatedQuizzesAsync(string userId);
    Task<QuizResponseDto?> UpdateQuizAsync(int quizId, UpdateQuizDto updateDto, string userId);
    Task<bool> DeleteQuizAsync(int quizId, string userId);
    Task<bool> ToggleQuizStatusAsync(int quizId, string userId);
    Task<QuizResponseDto?> AddQuestionToQuizAsync(int quizId, CreateQuestionDto questionDto, string userId);
    Task<bool> RemoveQuestionFromQuizAsync(int quizId, int questionId, string userId);
}
```

### QuizService

**Location**: `Services/Implementations/QuizService.cs`

#### CreateQuizAsync

Creates a new quiz with questions and choices.

**Method Signature**:
```csharp
public async Task<QuizResponseDto?> CreateQuizAsync(CreateQuizDto createDto, string userId)
```

**Business Rules**:
- Each question must have 2-6 choices
- Each question must have exactly 1 correct answer
- Title required (3-200 characters)
- Description optional (max 2000 characters)

**Process**:
1. Validate question and choice count
2. Validate exactly one correct answer per question
3. Map DTO to Quiz entity
4. Set CreatedById and CreatedAt
5. Save to database
6. Load navigation properties
7. Map to response DTO

**Returns**: `null` on validation failure, `QuizResponseDto` on success

#### GetQuizByIdAsync

Retrieves quiz details including all questions and choices.

**Method Signature**:
```csharp
public async Task<QuizResponseDto?> GetQuizByIdAsync(int quizId)
```

**Process**:
1. Query database with Include for Questions, Choices, and User
2. Map entity to DTO
3. Return null if not found

**Returns**: `null` if not found, `QuizResponseDto` if found

**Performance**: Uses eager loading to avoid N+1 queries

#### GetAllQuizzesAsync

Retrieves all quizzes (summary view).

**Method Signature**:
```csharp
public async Task<List<QuizSummaryDto>> GetAllQuizzesAsync()
```

**Process**:
1. Query all quizzes with User navigation
2. Include question count
3. Map to summary DTOs
4. Order by CreatedAt descending

**Returns**: List of `QuizSummaryDto` (empty list if none)

#### GetActiveQuizzesAsync

Retrieves only active quizzes.

**Method Signature**:
```csharp
public async Task<List<QuizSummaryDto>> GetActiveQuizzesAsync()
```

**Process**:
1. Filter by IsActive = true
2. Same as GetAllQuizzesAsync

**Returns**: List of active `QuizSummaryDto`

#### GetUserCreatedQuizzesAsync

Retrieves quizzes created by specific user.

**Method Signature**:
```csharp
public async Task<List<QuizSummaryDto>> GetUserCreatedQuizzesAsync(string userId)
```

**Process**:
1. Filter by CreatedById = userId
2. Return all quizzes by user

**Returns**: List of user's `QuizSummaryDto`

#### UpdateQuizAsync

Updates quiz metadata.

**Method Signature**:
```csharp
public async Task<QuizResponseDto?> UpdateQuizAsync(int quizId, UpdateQuizDto updateDto, string userId)
```

**Business Rules**:
- Only quiz owner can update
- Cannot update questions (use separate endpoint)

**Process**:
1. Find quiz by ID and user ID (ownership check)
2. Update Title, Description, IsActive
3. Save changes
4. Return updated quiz

**Returns**: `null` if not found or unauthorized, `QuizResponseDto` on success

#### DeleteQuizAsync

Deletes a quiz and its questions/choices.

**Method Signature**:
```csharp
public async Task<bool> DeleteQuizAsync(int quizId, string userId)
```

**Business Rules**:
- Only quiz owner can delete
- Cascade deletes questions and choices
- May fail if quiz has attempts (referential integrity)

**Process**:
1. Find quiz by ID and user ID
2. Remove from context
3. Save changes

**Returns**: `false` if not found or unauthorized, `true` on success

**Note**: Consider soft delete for production

#### ToggleQuizStatusAsync

Toggles quiz IsActive status.

**Method Signature**:
```csharp
public async Task<bool> ToggleQuizStatusAsync(int quizId, string userId)
```

**Process**:
1. Find quiz by ID and user ID
2. Toggle IsActive
3. Save changes

**Returns**: `false` if not found or unauthorized, `true` on success

#### AddQuestionToQuizAsync

Adds a new question to existing quiz.

**Method Signature**:
```csharp
public async Task<QuizResponseDto?> AddQuestionToQuizAsync(int quizId, CreateQuestionDto questionDto, string userId)
```

**Business Rules**:
- Question must have 2-6 choices
- Exactly 1 correct answer

**Process**:
1. Find quiz by ID and user ID
2. Validate question
3. Create Question entity
4. Add to quiz
5. Save changes
6. Return updated quiz

**Returns**: `null` if validation fails, `QuizResponseDto` on success

#### RemoveQuestionFromQuizAsync

Removes a question from quiz.

**Method Signature**:
```csharp
public async Task<bool> RemoveQuestionFromQuizAsync(int quizId, int questionId, string userId)
```

**Business Rules**:
- Only quiz owner can remove questions
- Cascade deletes choices
- May fail if question has user answers

**Process**:
1. Find quiz by ID and user ID
2. Find question in quiz
3. Remove question
4. Save changes

**Returns**: `false` if not found or unauthorized, `true` on success

## Quiz Attempt Service

### IQuizAttemptService

**Location**: `Services/Interfaces/IQuizAttemptService.cs`

```csharp
public interface IQuizAttemptService
{
    Task<QuizAttemptResponseDto?> StartQuizAttemptAsync(int quizId, string userId);
    Task<QuizResultDto?> SubmitQuizAsync(SubmitQuizDto submitDto, string userId);
    Task<QuizAttemptResponseDto?> GetAttemptByIdAsync(int attemptId, string userId);
    Task<List<QuizAttemptResponseDto>> GetUserAttemptsAsync(string userId);
    Task<List<QuizAttemptResponseDto>> GetQuizAttemptsAsync(int quizId);
    Task<QuizResultDto?> GetAttemptResultAsync(int attemptId, string userId);
    Task<List<QuizAttemptResponseDto>> GetQuizLeaderboardAsync(int quizId, int topCount = 10);
}
```

### QuizAttemptService

**Location**: `Services/Implementations/QuizAttemptService.cs`

#### StartQuizAttemptAsync

Starts a new quiz attempt.

**Method Signature**:
```csharp
public async Task<QuizAttemptResponseDto?> StartQuizAttemptAsync(int quizId, string userId)
```

**Business Rules**:
- Quiz must exist
- Quiz must be active (IsActive = true)
- Quiz must have at least 1 question
- No limit on attempts per user

**Process**:
1. Find quiz with IsActive check
2. Verify quiz has questions
3. Create QuizAttempt entity
4. Set StartedAt, Score = 0
5. Save to database
6. Load navigation properties
7. Map to response DTO

**Returns**: `null` if quiz invalid, `QuizAttemptResponseDto` on success

#### SubmitQuizAsync

Submits quiz answers and calculates score.

**Method Signature**:
```csharp
public async Task<QuizResultDto?> SubmitQuizAsync(SubmitQuizDto submitDto, string userId)
```

**Business Rules**:
- User must own the attempt
- Attempt must not be already completed
- Must answer all questions
- All question IDs must belong to the quiz
- All choice IDs must belong to the questions

**Process**:
1. Find attempt with quiz, questions, and choices
2. Validate ownership and completion status
3. Validate answer count matches question count
4. Process each answer:
   - Find question and selected choice
   - Determine if correct
   - Save UserAnswer entity
   - Build QuestionResultDto
5. Calculate score: (Correct / Total) * 100
6. Update attempt with score and CompletedAt
7. Update user statistics
8. Return QuizResultDto with results

**Scoring Algorithm**:
```
Score = Round((CorrectAnswers / TotalQuestions) * 100)
```

**Returns**: `null` on validation failure, `QuizResultDto` on success

#### GetAttemptByIdAsync

Retrieves attempt details.

**Method Signature**:
```csharp
public async Task<QuizAttemptResponseDto?> GetAttemptByIdAsync(int attemptId, string userId)
```

**Business Rules**:
- User must own the attempt

**Process**:
1. Find attempt by ID and user ID
2. Include Quiz and User navigation
3. Map to response DTO

**Returns**: `null` if not found or unauthorized, `QuizAttemptResponseDto` on success

#### GetUserAttemptsAsync

Retrieves all attempts by user.

**Method Signature**:
```csharp
public async Task<List<QuizAttemptResponseDto>> GetUserAttemptsAsync(string userId)
```

**Process**:
1. Query attempts by user ID
2. Include Quiz and User navigation
3. Order by StartedAt descending
4. Map to response DTOs

**Returns**: List of `QuizAttemptResponseDto` (empty if none)

#### GetQuizAttemptsAsync

Retrieves all completed attempts for a quiz.

**Method Signature**:
```csharp
public async Task<List<QuizAttemptResponseDto>> GetQuizAttemptsAsync(int quizId)
```

**Business Rules**:
- Only completed attempts (CompletedAt != null)

**Process**:
1. Query by quiz ID and completed status
2. Order by score (desc), then duration (asc)
3. Map to response DTOs

**Returns**: List of completed `QuizAttemptResponseDto`

#### GetAttemptResultAsync

Retrieves detailed results for completed attempt.

**Method Signature**:
```csharp
public async Task<QuizResultDto?> GetAttemptResultAsync(int attemptId, string userId)
```

**Business Rules**:
- User must own the attempt
- Attempt must be completed

**Process**:
1. Find attempt with quiz, questions, choices, user answers
2. Verify ownership and completion
3. Build QuestionResultDto for each question
4. Calculate statistics
5. Return QuizResultDto

**Returns**: `null` if not found, not completed, or unauthorized; `QuizResultDto` on success

#### GetQuizLeaderboardAsync

Retrieves top performers for a quiz.

**Method Signature**:
```csharp
public async Task<List<QuizAttemptResponseDto>> GetQuizLeaderboardAsync(int quizId, int topCount = 10)
```

**Business Rules**:
- Only completed attempts
- topCount: 1-100 (validated in controller)

**Ranking Algorithm**:
1. Primary: Score (highest first)
2. Tiebreaker: Duration (fastest first)
   - Duration = CompletedAt - StartedAt

**Process**:
1. Query by quiz ID and completed status
2. Order by Score DESC, Duration ASC
3. Take top N results
4. Map to response DTOs

**Returns**: List of top `QuizAttemptResponseDto`

#### UpdateUserStatisticsAsync

Private method to update user statistics after quiz submission.

**Method Signature**:
```csharp
private async Task UpdateUserStatisticsAsync(string userId)
```

**Process**:
1. Find user by ID
2. Query all completed attempts
3. Calculate TotalQuizAttempts (count)
4. Calculate AverageScore (average)
5. Update user entity
6. Save changes

**Called After**: Every quiz submission

## Service Layer Best Practices

### Error Handling

Services return `null` or empty collections on failure:

```csharp
try
{
    // Business logic
    return result;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error context");
    return null; // or empty list
}
```

### Logging

Use structured logging with context:

```csharp
_logger.LogInformation("Quiz created: {QuizId} by user {UserId}", quiz.Id, userId);
_logger.LogWarning("Quiz not found or inactive: {QuizId}", quizId);
_logger.LogError(ex, "Error creating quiz for user {UserId}", userId);
```

### Transactions

Entity Framework Core tracks changes and manages transactions automatically:

```csharp
// All changes in single transaction
_context.Quizzes.Add(quiz);
_context.UserAnswers.AddRange(answers);
await _context.SaveChangesAsync(); // Commits transaction
```

For explicit transactions:

```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // Multiple operations
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### Eager Loading

Prevent N+1 queries with Include:

```csharp
var quiz = await _context.Quizzes
    .Include(q => q.Questions)
        .ThenInclude(q => q.Choices)
    .Include(q => q.CreatedBy)
    .FirstOrDefaultAsync(q => q.Id == quizId);
```

### Mapping

Use AutoMapper for entity-DTO conversion:

```csharp
// Entity to DTO
var dto = _mapper.Map<QuizResponseDto>(quiz);

// DTO to Entity
var quiz = _mapper.Map<Quiz>(createDto);

// Collection mapping
var dtos = _mapper.Map<List<QuizSummaryDto>>(quizzes);
```

### Async/Await

All database operations should be async:

```csharp
// Good
var quiz = await _context.Quizzes.FindAsync(id);

// Avoid
var quiz = _context.Quizzes.Find(id); // Synchronous
```

## Testing Services

### Unit Testing with In-Memory Database

```csharp
public class QuizServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<QuizService>> _loggerMock;
    private readonly QuizService _service;

    public QuizServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        var config = new MapperConfiguration(cfg => {
            cfg.AddProfiles(new[] { typeof(QuizProfile) });
        });
        _mapper = config.CreateMapper();

        _loggerMock = new Mock<ILogger<QuizService>>();
        _service = new QuizService(_context, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateQuizAsync_ValidQuiz_ReturnsQuizResponse()
    {
        // Arrange
        var createDto = new CreateQuizDto { /* ... */ };

        // Act
        var result = await _service.CreateQuizAsync(createDto, "user123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Quiz", result.Title);
    }
}
```

## Performance Considerations

### Query Optimization

- Use projection for large result sets
- Avoid loading unnecessary navigation properties
- Use AsNoTracking for read-only queries
- Implement caching for frequently accessed data

### Batch Operations

For bulk operations, use AddRange/UpdateRange:

```csharp
_context.UserAnswers.AddRange(userAnswers);
await _context.SaveChangesAsync(); // Single database round-trip
```

### Lazy Loading

Disabled by default. Use explicit Include() instead:

```csharp
// Explicit loading (recommended)
var quiz = await _context.Quizzes
    .Include(q => q.Questions)
    .FirstOrDefaultAsync(q => q.Id == id);

// Lazy loading (not recommended)
var quiz = await _context.Quizzes.FindAsync(id);
var questions = quiz.Questions; // Triggers separate query
```

## Future Enhancements

Potential service improvements:

1. **Caching Layer**: Cache active quizzes, leaderboards
2. **Background Jobs**: Process statistics updates asynchronously
3. **Event Sourcing**: Track all quiz changes
4. **CQRS**: Separate read and write models
5. **Validation Service**: Extract validation logic
6. **Notification Service**: Email notifications for quiz completion
