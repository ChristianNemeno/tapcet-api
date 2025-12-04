# Service Layer Documentation

## Overview

The service layer contains all business logic for the TAPCET Quiz API. This document details each service's responsibilities, methods, and implementation patterns.

---

## Architecture Pattern

### Service Layer Responsibilities

1. **Business Logic**: Implement all business rules and validation
2. **Data Access**: Interact with database through EF Core
3. **Error Handling**: Catch and log exceptions
4. **Data Transformation**: Map entities to DTOs using AutoMapper
5. **Transaction Management**: Ensure data consistency

### Service Pattern Structure

```csharp
public class ServiceName : IServiceName
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ServiceName> _logger;

    public ServiceName(ApplicationDbContext context, IMapper mapper, ILogger<ServiceName> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    // Service methods...
}
```

---

## 1. AuthService

### Purpose
Handle user authentication, registration, and JWT token generation.

### Location
`Services/Implementations/AuthService.cs`

### Interface
`Services/Interfaces/IAuthService.cs`

### Dependencies
- `UserManager<User>` - ASP.NET Identity user management
- `SignInManager<User>` - Authentication operations
- `IConfiguration` - Access JWT settings
- `ILogger<AuthService>` - Logging

---

### Methods

#### RegisterAsync

**Signature:**
```csharp
Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
```

**Purpose:** Register a new user account

**Business Logic:**
1. Check if email already exists
2. Create new User entity
3. Hash password using Identity
4. Assign "User" role
5. Generate JWT token
6. Return authentication response

**Returns:**
- `AuthResponseDto` on success
- `null` on failure (email exists, validation error, database error)

**Example:**
```csharp
var registerDto = new RegisterDto 
{ 
    Username = "john",
    Email = "john@example.com",
    Password = "Pass123" 
};
var result = await _authService.RegisterAsync(registerDto);
if (result != null)
{
    // Success - user registered and token generated
}
```

---

#### LoginAsync

**Signature:**
```csharp
Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
```

**Purpose:** Authenticate user and generate JWT token

**Business Logic:**
1. Find user by email
2. Verify password using Identity
3. Get user roles
4. Generate JWT token with claims
5. Return authentication response

**Returns:**
- `AuthResponseDto` on success
- `null` on failure (user not found, invalid password)

**JWT Claims Generated:**
- NameIdentifier: User ID
- Name: Username
- Email: User email
- Role: All assigned roles
- Jti: Unique token ID

---

#### UserExistsAsync

**Signature:**
```csharp
Task<bool> UserExistsAsync(string email)
```

**Purpose:** Check if email is already registered

**Returns:** `true` if email exists, `false` otherwise

---

#### GenerateJwtToken (Private)

**Signature:**
```csharp
private async Task<string> GenerateJwtToken(User user)
```

**Purpose:** Create JWT token for authenticated user

**Configuration Used:**
- SecretKey: Signing key
- Issuer: Token issuer
- Audience: Token audience
- ExpiryInMinutes: Token lifespan (default 60)

**Token Contents:**
- Header: Algorithm (HS256), Type (JWT)
- Payload: Claims (user info, roles)
- Signature: HMAC-SHA256

---

### Error Handling

```csharp
try
{
    // Business logic
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error message with context");
    return null;
}
```

**Logged Errors:**
- Registration failure with reasons
- Login failure (user not found, invalid password)
- Database errors

---

## 2. QuizService

### Purpose
Manage quiz CRUD operations and question management.

### Location
`Services/Implementations/QuizService.cs`

### Interface
`Services/Interfaces/IQuizService.cs`

### Dependencies
- `ApplicationDbContext` - Database access
- `IMapper` - Entity/DTO mapping
- `ILogger<QuizService>` - Logging

---

### Methods

#### CreateQuizAsync

**Signature:**
```csharp
Task<QuizResponseDto?> CreateQuizAsync(CreateQuizDto createDto, string userId)
```

**Purpose:** Create a new quiz with questions and choices

**Business Logic:**
1. Validate quiz has questions
2. For each question:
   - Validate 2-6 choices
   - Validate exactly 1 correct answer
3. Map DTO to entities
4. Set CreatedById to current user
5. Set CreatedAt to UTC now
6. Set IsActive to true
7. Save to database (cascade insert questions and choices)
8. Return full quiz details

**Validation Rules:**
- At least 1 question required
- Each question needs 2-6 choices
- Exactly 1 choice must be correct per question

**Returns:**
- `QuizResponseDto` on success
- `null` on validation failure or database error

**Example:**
```csharp
var createDto = new CreateQuizDto
{
    Title = "My Quiz",
    Description = "Quiz description",
    Questions = new List<CreateQuestionDto> { ... }
};
var result = await _quizService.CreateQuizAsync(createDto, userId);
```

---

#### GetQuizByIdAsync

**Signature:**
```csharp
Task<QuizResponseDto?> GetQuizByIdAsync(int quizId)
```

**Purpose:** Retrieve complete quiz details

**Includes:**
- Quiz metadata
- Creator information
- All questions
- All choices for each question

**EF Core Query:**
```csharp
var quiz = await _context.Quizzes
    .Include(q => q.CreatedBy)
    .Include(q => q.Questions)
        .ThenInclude(q => q.Choices)
    .FirstOrDefaultAsync(q => q.Id == quizId);
```

**Returns:**
- `QuizResponseDto` if found
- `null` if not found

---

#### GetAllQuizzesAsync

**Signature:**
```csharp
Task<List<QuizSummaryDto>> GetAllQuizzesAsync()
```

**Purpose:** Get summary of all quizzes

**Includes:**
- Quiz metadata
- Creator name
- Question count
- Attempt count

**Ordering:** Most recent first (CreatedAt DESC)

**Returns:** List of `QuizSummaryDto` (empty list on error)

---

#### GetActiveQuizzesAsync

**Signature:**
```csharp
Task<List<QuizSummaryDto>> GetActiveQuizzesAsync()
```

**Purpose:** Get summary of active quizzes only

**Filter:** `IsActive == true`

**Returns:** List of `QuizSummaryDto` (empty list on error)

---

#### UpdateQuizAsync

**Signature:**
```csharp
Task<QuizResponseDto?> UpdateQuizAsync(int quizId, UpdateQuizDto updateDto, string userId)
```

**Purpose:** Update quiz metadata

**Authorization:** Only creator can update (admin check could be added)

**Updatable Fields:**
- Title
- Description
- IsActive

**Non-Updatable Fields:**
- Id
- CreatedAt
- CreatedById
- Questions (use AddQuestionToQuizAsync instead)

**Returns:**
- `QuizResponseDto` on success
- `null` if not found or unauthorized

---

#### DeleteQuizAsync

**Signature:**
```csharp
Task<bool> DeleteQuizAsync(int quizId, string userId)
```

**Purpose:** Permanently delete a quiz

**Authorization:** Only creator can delete

**Cascade Behavior:**
- Questions are deleted (CASCADE)
- Choices are deleted (CASCADE via Questions)
- Attempts are NOT deleted (RESTRICT) - may cause FK error

**Returns:**
- `true` on success
- `false` on failure

**Recommendation:** Implement soft delete instead

---

#### ToggleQuizStatusAsync

**Signature:**
```csharp
Task<bool> ToggleQuizStatusAsync(int quizId, string userId)
```

**Purpose:** Toggle quiz IsActive status

**Authorization:** Only creator can toggle

**Effect:**
- Active ? Inactive: Prevents new attempts
- Inactive ? Active: Allows attempts again

**Returns:**
- `true` on success
- `false` on failure

---

#### AddQuestionToQuizAsync

**Signature:**
```csharp
Task<QuizResponseDto?> AddQuestionToQuizAsync(int quizId, CreateQuestionDto questionDto, string userId)
```

**Purpose:** Add a new question to existing quiz

**Authorization:** Only creator can add questions

**Validation:** Same as CreateQuizAsync for questions

**Returns:**
- Updated `QuizResponseDto` on success
- `null` on validation failure or unauthorized

---

### Error Handling Pattern

All methods follow consistent error handling:

```csharp
try
{
    // 1. Fetch data
    // 2. Validate business rules
    // 3. Perform operation
    // 4. Save changes
    // 5. Return result
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error context");
    return null; // or empty list
}
```

---

## 3. QuizAttemptService

### Purpose
Manage quiz attempts, answer submissions, scoring, and leaderboards.

### Location
`Services/Implementations/QuizAttemptService.cs`

### Interface
`Services/Interfaces/IQuizAttemptService.cs`

### Dependencies
- `ApplicationDbContext` - Database access
- `IMapper` - Entity/DTO mapping
- `ILogger<QuizAttemptService>` - Logging

---

### Methods

#### StartQuizAttemptAsync

**Signature:**
```csharp
Task<QuizAttemptResponseDto?> StartQuizAttemptAsync(int quizId, string userId)
```

**Purpose:** Start a new quiz attempt

**Business Logic:**
1. Find quiz by ID
2. Validate quiz exists and is active
3. Validate quiz has questions
4. Create new QuizAttempt:
   - QuizId = quizId
   - UserId = userId
   - StartedAt = UTC now
   - Score = 0
   - CompletedAt = null
5. Save to database
6. Load navigation properties
7. Return attempt details

**Returns:**
- `QuizAttemptResponseDto` on success
- `null` if quiz not found, inactive, or has no questions

**Example:**
```csharp
var attempt = await _attemptService.StartQuizAttemptAsync(quizId, userId);
if (attempt != null)
{
    // Attempt started successfully
    var attemptId = attempt.Id;
}
```

---

#### SubmitQuizAsync

**Signature:**
```csharp
Task<QuizResultDto?> SubmitQuizAsync(SubmitQuizDto submitDto, string userId)
```

**Purpose:** Submit quiz answers and calculate results

**Business Logic:**
1. Find quiz attempt with related data:
   - Quiz
   - Questions
   - Choices
   - User
2. Validate attempt exists and user owns it
3. Validate attempt not already completed
4. Validate answer count matches question count
5. For each answer:
   - Find corresponding question
   - Find selected choice
   - Find correct choice
   - Determine if answer is correct
   - Create UserAnswer record
   - Build question result
6. Calculate score: `(correctAnswers / totalQuestions) * 100`
7. Update attempt:
   - Set Score
   - Set CompletedAt to UTC now
8. Save all UserAnswers
9. Update user statistics
10. Return detailed results

**Scoring Formula:**
```csharp
int score = (int)Math.Round((double)correctAnswers / totalQuestions * 100);
```

**Returns:**
- `QuizResultDto` with detailed results on success
- `null` on validation failure or error

**Example:**
```csharp
var submitDto = new SubmitQuizDto
{
    QuizAttemptId = attemptId,
    Answers = answers
};
var result = await _attemptService.SubmitQuizAsync(submitDto, userId);
// result contains score, percentage, question-by-question breakdown
```

---

#### GetAttemptByIdAsync

**Signature:**
```csharp
Task<QuizAttemptResponseDto?> GetAttemptByIdAsync(int attemptId, string userId)
```

**Purpose:** Get basic attempt information

**Authorization:** User must own the attempt

**Returns:**
- `QuizAttemptResponseDto` if found and authorized
- `null` otherwise

---

#### GetUserAttemptsAsync

**Signature:**
```csharp
Task<List<QuizAttemptResponseDto>> GetUserAttemptsAsync(string userId)
```

**Purpose:** Get all attempts for a specific user

**Includes:**
- Completed and in-progress attempts
- Quiz information
- User information

**Ordering:** Most recent first (StartedAt DESC)

**Returns:** List of `QuizAttemptResponseDto` (empty list on error)

---

#### GetQuizAttemptsAsync

**Signature:**
```csharp
Task<List<QuizAttemptResponseDto>> GetQuizAttemptsAsync(int quizId)
```

**Purpose:** Get all completed attempts for a quiz

**Filter:** Only completed attempts (CompletedAt != null)

**Ordering:**
1. Score (highest first)
2. Duration (fastest first)

**Returns:** List of `QuizAttemptResponseDto` (empty list on error)

---

#### GetAttemptResultAsync

**Signature:**
```csharp
Task<QuizResultDto?> GetAttemptResultAsync(int attemptId, string userId)
```

**Purpose:** Get detailed results for a completed attempt

**Authorization:** User must own the attempt

**Validation:** Attempt must be completed

**Includes:**
- Overall statistics (score, percentage, counts)
- Question-by-question breakdown
- Selected vs correct answers
- Explanations

**Returns:**
- `QuizResultDto` on success
- `null` if not found, unauthorized, or not completed

---

#### GetQuizLeaderboardAsync

**Signature:**
```csharp
Task<List<QuizAttemptResponseDto>> GetQuizLeaderboardAsync(int quizId, int topCount = 10)
```

**Purpose:** Get top performers for a quiz

**Parameters:**
- `quizId`: Quiz to get leaderboard for
- `topCount`: Number of top results (default 10)

**Filter:** Only completed attempts

**Ordering:**
1. Score DESC (higher is better)
2. Duration ASC (faster is better)

**Duration Calculation:**
```csharp
Duration = CompletedAt - StartedAt
```

**Returns:** Top N attempts (empty list on error)

**Example:**
```csharp
var leaderboard = await _attemptService.GetQuizLeaderboardAsync(quizId, 20);
// Returns top 20 performers
```

---

#### UpdateUserStatisticsAsync (Private)

**Signature:**
```csharp
private async Task UpdateUserStatisticsAsync(string userId)
```

**Purpose:** Recalculate user statistics after quiz completion

**Updates:**
- `TotalQuizAttempts`: Count of completed attempts
- `AverageScore`: Average score across all attempts

**Calculation:**
```csharp
user.TotalQuizAttempts = completedAttempts.Count;
user.AverageScore = completedAttempts.Any() 
    ? completedAttempts.Average(a => a.Score) 
    : 0;
```

**Called By:** SubmitQuizAsync

---

### Complex Queries

#### Submit Quiz Query
```csharp
var attempt = await _context.QuizAttempts
    .Include(a => a.Quiz)
        .ThenInclude(q => q.Questions)
            .ThenInclude(q => q.Choices)
    .Include(a => a.User)
    .FirstOrDefaultAsync(a => a.Id == submitDto.QuizAttemptId && a.UserId == userId);
```

**Explanation:**
- Load attempt
- Load quiz with all questions
- Load all choices for each question
- Load user information
- Filter by attempt ID and user ID (authorization)

#### Get Attempt Result Query
```csharp
var attempt = await _context.QuizAttempts
    .Include(a => a.Quiz)
        .ThenInclude(q => q.Questions)
            .ThenInclude(q => q.Choices)
    .Include(a => a.UserAnswers)
        .ThenInclude(ua => ua.Choice)
    .Include(a => a.UserAnswers)
        .ThenInclude(ua => ua.Question)
    .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId);
```

**Explanation:**
- Load attempt with quiz, questions, choices
- Load all user answers
- Load choice for each answer
- Load question for each answer
- Needed to build detailed results

#### Leaderboard Query
```csharp
var attempts = await _context.QuizAttempts
    .Include(a => a.Quiz)
    .Include(a => a.User)
    .Where(a => a.QuizId == quizId && a.CompletedAt.HasValue)
    .OrderByDescending(a => a.Score)
    .ThenBy(a => a.CompletedAt!.Value - a.StartedAt) // Duration
    .Take(topCount)
    .ToListAsync();
```

---

## Service Best Practices

### 1. Consistent Error Handling

```csharp
try
{
    // Business logic
    return result;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Context: {Param1}, {Param2}", param1, param2);
    return null; // or appropriate default
}
```

### 2. Explicit Includes

Always specify what to load:
```csharp
.Include(q => q.CreatedBy)
.Include(q => q.Questions)
    .ThenInclude(q => q.Choices)
```

### 3. Authorization Checks

```csharp
if (quiz.CreatedById != userId)
{
    _logger.LogWarning("Unauthorized access attempt");
    return null;
}
```

### 4. Logging Levels

- **Information**: Successful operations
- **Warning**: Business rule violations, not found
- **Error**: Exceptions, database errors

### 5. Null Handling

```csharp
if (entity == null)
{
    _logger.LogWarning("Entity not found: {Id}", id);
    return null;
}
```

### 6. Transaction Management

EF Core auto-manages transactions for SaveChangesAsync(), but for complex operations:

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

---

## Testing Services

### Unit Test Example

```csharp
[Fact]
public async Task StartQuizAttemptAsync_ValidQuiz_ReturnsAttempt()
{
    // Arrange
    var context = GetInMemoryContext();
    var mapper = GetMapper();
    var logger = GetMockLogger();
    var service = new QuizAttemptService(context, mapper, logger);
    
    var quiz = new Quiz { Title = "Test", IsActive = true };
    quiz.Questions.Add(new Question { Text = "Q1" });
    context.Quizzes.Add(quiz);
    await context.SaveChangesAsync();

    // Act
    var result = await service.StartQuizAttemptAsync(quiz.Id, "user1");

    // Assert
    Assert.NotNull(result);
    Assert.Equal(quiz.Id, result.QuizId);
    Assert.False(result.IsCompleted);
}
```

---

## Performance Considerations

### 1. Use AsNoTracking for Read-Only Queries

```csharp
var quizzes = await _context.Quizzes
    .AsNoTracking()
    .Include(q => q.Questions)
    .ToListAsync();
```

### 2. Project to DTOs Directly

```csharp
var summaries = await _context.Quizzes
    .Select(q => new QuizSummaryDto
    {
        Id = q.Id,
        Title = q.Title,
        QuestionCount = q.Questions.Count
    })
    .ToListAsync();
```

### 3. Batch Operations

```csharp
_context.UserAnswers.AddRange(userAnswers);
await _context.SaveChangesAsync(); // One trip
```

### 4. Avoid N+1 Queries

Use Include/ThenInclude instead of lazy loading.

---

**Last Updated**: 2024
**Version**: 1.0.0
