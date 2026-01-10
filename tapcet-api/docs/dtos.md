# Data Transfer Objects (DTOs)

## Overview

DTOs define the structure of data exchanged between the client and API. They serve multiple purposes:
- Define API contracts
- Implement validation rules
- Prevent over-posting/under-posting
- Decouple internal models from external API

## Authentication DTOs

### RegisterDto

Used for user registration.

**Location**: `DTO/Auth/RegisterDto.cs`

```csharp
public class RegisterDto
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is requred")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is requred")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be atleast 6 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Confirm password is required")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }
}
```

### LoginDto

Used for user authentication.

**Location**: `DTO/Auth/LoginDto.cs`

```csharp
public class LoginDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}
```

### AuthResponseDto

Returned after successful authentication (login or registration).

**Location**: `DTO/Auth/AuthResponseDto.cs`

```csharp
public class AuthResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
}
```

### AuthResult

Wrapper for authentication operations that provides success/failure status with detailed error information.

**Location**: `DTO/Auth/AuthResult.cs`

```csharp
public class AuthResult
{
    public bool Succeeded { get; set; }
    public AuthResponseDto? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string>? Errors { get; set; }

    public static AuthResult Success(AuthResponseDto data) => new()
    {
        Succeeded = true,
        Data = data
    };

    public static AuthResult Failure(string message, List<string>? errors = null) => new()
    {
        Succeeded = false,
        ErrorMessage = message,
        Errors = errors
    };
}
```

**Usage**: The service layer returns `AuthResult` instead of nullable `AuthResponseDto`. The controller checks `result.Succeeded` and returns either `result.Data` or an error response with `result.ErrorMessage` and `result.Errors`.

**Error Codes**:
- `EMAIL_EXISTS` - Email already registered
- `USERNAME_EXISTS` - Username already taken
- `INVALID_CREDENTIALS` - Wrong email/password combination
- `INTERNAL_ERROR` - Unexpected server error

## Quiz DTOs

### CreateQuizDto

Used to create a new quiz with questions and choices.

**Location**: `DTO/Quiz/CreateQuizDto.cs`

```csharp
public class CreateQuizDto
{
    [Required(ErrorMessage = "Quiz title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public required string Title { get; set; }

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "At least one question is required")]
    [MinLength(1, ErrorMessage = "Quiz must have at least one question")]
    public required List<CreateQuestionDto> Questions { get; set; }
}
```

### UpdateQuizDto

Used to update quiz metadata.

**Location**: `DTO/Quiz/UpdateQuizDto.cs`

```csharp
public class UpdateQuizDto
{
    [Required(ErrorMessage = "Quiz title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public required string Title { get; set; }

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
```

### QuizResponseDto

Full quiz details including questions and choices.

**Location**: `DTO/Quiz/QuizResponseDto.cs`

```csharp
public class QuizResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int QuestionCount { get; set; }
    public List<QuestionResponseDto> Questions { get; set; } = new();
}
```

### QuizSummaryDto

Lightweight quiz information without questions.

**Location**: `DTO/Quiz/QuizSummaryDto.cs`

```csharp
public class QuizSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int QuestionCount { get; set; }
    public int AttemptCount { get; set; }
}
```

## Question DTOs

### CreateQuestionDto

Used to create a question with choices.

**Location**: `DTO/Question/CreateQuestionDto.cs`

```csharp
public class CreateQuestionDto
{
    [Required(ErrorMessage = "Question text is required")]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Question must be between 5 and 1000 characters")]
    public required string Text { get; set; }

    [StringLength(1000, ErrorMessage = "Explanation cannot exceed 1000 characters")]
    public string? Explanation { get; set; }

    [Url(ErrorMessage = "Invalid image URL format")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Choices are required")]
    [MinLength(2, ErrorMessage = "Question must have at least 2 choices")]
    [MaxLength(6, ErrorMessage = "Question cannot have more than 6 choices")]
    public required List<CreateChoiceDto> Choices { get; set; }
}
```

### QuestionResponseDto

Question details with choices.

**Location**: `DTO/Question/QuestionResponseDto.cs`

```csharp
public class QuestionResponseDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string? ImageUrl { get; set; }
    public int QuizId { get; set; }
    public List<ChoiceResponseDto> Choices { get; set; } = new();
}
```

## Choice DTOs

### CreateChoiceDto

Used to create answer choices.

**Location**: `DTO/Choice/CreateChoiceDto.cs`

```csharp
public class CreateChoiceDto
{
    [Required(ErrorMessage = "Choice text is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Choice must be between 1 and 500 characters")]
    public required string Text { get; set; }

    public bool IsCorrect { get; set; } = false;
}
```

### ChoiceResponseDto

Choice information including correctness indicator.

**Location**: `DTO/Choice/ChoiceResponseDto.cs`

```csharp
public class ChoiceResponseDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int QuestionId { get; set; }
}
```

## Quiz Attempt DTOs

### StartQuizAttemptDto

Used to start a quiz attempt.

**Location**: `DTO/Attempt/StartQuizAttemptDto.cs`

```csharp
public class StartQuizAttemptDto
{
    [Required(ErrorMessage = "Quiz ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid quiz ID")]
    public int QuizId { get; set; }
}
```

### QuizAttemptResponseDto

Basic attempt information.

**Location**: `DTO/Attempt/QuizAttemptResponseDto.cs`

```csharp
public class QuizAttemptResponseDto
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int Score { get; set; }
    public bool IsCompleted { get; set; }
}
```

### SubmitQuizDto

Used to submit quiz answers.

**Location**: `DTO/Attempt/SubmitQuizDto.cs`

```csharp
public class SubmitQuizDto
{
    [Required(ErrorMessage = "Quiz attempt ID is required")]
    public int QuizAttemptId { get; set; }

    [Required(ErrorMessage = "Answers are required")]
    [MinLength(1, ErrorMessage = "At least one answer is required")]
    public required List<SubmitAnswerDto> Answers { get; set; }
}
```

### SubmitAnswerDto

Individual answer within submission.

**Location**: `DTO/Attempt/SubmitAnswerDto.cs`

```csharp
public class SubmitAnswerDto
{
    [Required(ErrorMessage = "Question ID is required")]
    public int QuestionId { get; set; }

    [Required(ErrorMessage = "Choice ID is required")]
    public int ChoiceId { get; set; }
}
```

### QuizResultDto

Detailed results after submission.

**Location**: `DTO/Attempt/QuizResultDto.cs`

```csharp
public class QuizResultDto
{
    public int QuizAttemptId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public int Score { get; set; }
    public double Percentage { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public List<QuestionResultDto> QuestionResults { get; set; } = new();
}
```

### QuestionResultDto

Individual question result.

**Location**: `DTO/Attempt/QuestionResultDto.cs`

```csharp
public class QuestionResultDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public int SelectedChoiceId { get; set; }
    public string SelectedChoiceText { get; set; } = string.Empty;
    public int CorrectChoiceId { get; set; }
    public string CorrectChoiceText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
```

## DTO Validation Attributes

### Common Attributes

- `[Required]`: Field must be provided
- `[StringLength(max, MinimumLength = min)]`: String length constraints
- `[EmailAddress]`: Validates email format
- `[Compare("Property")]`: Compares with another property
- `[Range(min, max)]`: Numeric range validation
- `[MinLength(n)]`: Minimum collection length
- `[MaxLength(n)]`: Maximum collection length
- `[Url]`: Validates URL format
- `[DataType(DataType.Password)]`: Indicates password field type

### Custom Validation

Can be implemented using `IValidatableObject`:

```csharp
public class CreateQuizDto : IValidatableObject
{
    public IEnumerable<ValidationError> Validate(ValidationContext validationContext)
    {
        if (Questions != null)
        {
            foreach (var question in Questions)
            {
                var correctCount = question.Choices.Count(c => c.IsCorrect);
                if (correctCount != 1)
                {
                    yield return new ValidationError(
                        "Each question must have exactly one correct answer",
                        new[] { nameof(Questions) }
                    );
                }
            }
        }
    }
}
```

## AutoMapper Profiles

DTOs are mapped to entities using AutoMapper profiles.

### Example: QuizProfile

```csharp
public class QuizProfile : Profile
{
    public QuizProfile()
    {
        CreateMap<Quiz, QuizResponseDto>()
            .ForMember(dest => dest.CreatedByName, 
                opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.UserName : "Unknown"))
            .ForMember(dest => dest.QuestionCount, 
                opt => opt.MapFrom(src => src.Questions.Count));

        CreateMap<Quiz, QuizSummaryDto>()
            .ForMember(dest => dest.CreatedById,
                opt => opt.MapFrom(src => src.CreatedById))
            .ForMember(dest => dest.CreatedByName, 
                opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.UserName : "Unknown"))
            .ForMember(dest => dest.QuestionCount, 
                opt => opt.MapFrom(src => src.Questions.Count))
            .ForMember(dest => dest.AttemptCount, 
                opt => opt.MapFrom(src => src.QuizAttempts.Count));

        CreateMap<CreateQuizDto, Quiz>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());
    }
}
```

## DTO Usage Examples

### Creating a Quiz

```csharp
var createDto = new CreateQuizDto
{
    Title = "JavaScript Basics",
    Description = "Test your JS knowledge",
    Questions = new List<CreateQuestionDto>
    {
        new CreateQuestionDto
        {
            Text = "What is a closure?",
            Explanation = "A function with access to outer scope",
            Choices = new List<CreateChoiceDto>
            {
                new CreateChoiceDto { Text = "A loop", IsCorrect = false },
                new CreateChoiceDto { Text = "A function within a function", IsCorrect = true },
                new CreateChoiceDto { Text = "A variable", IsCorrect = false }
            }
        }
    }
};
```

### Submitting a Quiz

```csharp
var submitDto = new SubmitQuizDto
{
    QuizAttemptId = 1,
    Answers = new List<SubmitAnswerDto>
    {
        new SubmitAnswerDto { QuestionId = 1, ChoiceId = 2 },
        new SubmitAnswerDto { QuestionId = 2, ChoiceId = 5 }
    }
};
```

## DTO Design Principles

1. **Separation of Concerns**: DTOs separate API contract from domain models
2. **Validation**: All input validation at DTO level
3. **Immutability**: Use `required` for mandatory properties
4. **Documentation**: Use XML comments for Swagger generation
5. **Versioning**: Create new DTOs for API version changes
6. **Security**: Never expose sensitive data (password hashes, etc.)
7. **Performance**: Use projection for large result sets

## JSON Serialization

ASP.NET Core uses `System.Text.Json` by default with camelCase naming policy:

**C# DTO Property:**
```csharp
public string UserId { get; set; }
```

**JSON Response:**
```json
{
  "userId": "123",
  "userName": "john",
  "expiresAt": "2024-01-15T15:30:00Z"
}
```

## Future Enhancements

Potential DTO additions:

1. **Pagination DTOs**
   ```csharp
   public class PagedResultDto<T>
   {
       public List<T> Items { get; set; }
       public int TotalCount { get; set; }
       public int PageNumber { get; set; }
       public int PageSize { get; set; }
   }
   ```

2. **Filter DTOs**
   ```csharp
   public class QuizFilterDto
   {
       public string? SearchTerm { get; set; }
       public bool? IsActive { get; set; }
       public string? CreatedBy { get; set; }
       public DateTime? CreatedAfter { get; set; }
   }
   ```

3. **Batch Operation DTOs**
   ```csharp
   public class BatchDeleteDto
   {
       public List<int> QuizIds { get; set; }
   }
   ```

## DTO Validation Summary

| DTO | Key Constraints |
|-----|----------------|
| RegisterDto | Username: 3-50 chars, Email: valid format, Password: 6-100 chars with uppercase/lowercase/digit |
| CreateQuizDto | Title: 3-200 chars, Description: max 2000 chars, Min 1 question |
| CreateQuestionDto | Text: 5-1000 chars, Explanation: max 1000 chars, 2-6 choices, exactly 1 correct |
| CreateChoiceDto | Text: 1-500 chars |
| SubmitQuizDto | Must answer all questions in quiz |
