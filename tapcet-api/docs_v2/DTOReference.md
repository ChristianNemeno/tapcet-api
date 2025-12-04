# DTO Reference Documentation

## Overview

This document provides a complete reference of all Data Transfer Objects (DTOs) used in the TAPCET Quiz API, including validation rules and usage examples.

---

## Authentication DTOs

### RegisterDto

**Purpose:** User registration request

**Location:** `DTO/Auth/RegisterDto.cs`

**Properties:**
```csharp
public class RegisterDto
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public required string Password { get; set; }
}
```

**Usage Example:**
```json
{
  "username": "johndoe",
  "email": "john.doe@example.com",
  "password": "SecurePass123"
}
```

---

### LoginDto

**Purpose:** User login request

**Location:** `DTO/Auth/LoginDto.cs`

**Properties:**
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

**Usage Example:**
```json
{
  "email": "john.doe@example.com",
  "password": "SecurePass123"
}
```

---

### AuthResponseDto

**Purpose:** Authentication response with JWT token

**Location:** `DTO/Auth/AuthResponseDto.cs`

**Properties:**
```csharp
public class AuthResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public List<string> Roles { get; set; } = new();
}
```

**Response Example:**
```json
{
  "userId": "abc123-def456-789ghi",
  "userName": "johndoe",
  "email": "john.doe@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-15T12:00:00Z",
  "roles": ["User"]
}
```

---

## Quiz DTOs

### CreateQuizDto

**Purpose:** Create a new quiz with questions

**Location:** `DTO/Quiz/CreateQuizDto.cs`

**Properties:**
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

**Usage Example:**
```json
{
  "title": "JavaScript Basics",
  "description": "Test your fundamental JavaScript knowledge",
  "questions": [
    {
      "text": "What is a closure?",
      "explanation": "A closure is a function with access to outer scope",
      "imageUrl": null,
      "choices": [
        { "text": "A loop structure", "isCorrect": false },
        { "text": "A function within a function", "isCorrect": true },
        { "text": "A variable type", "isCorrect": false }
      ]
    }
  ]
}
```

---

### UpdateQuizDto

**Purpose:** Update quiz metadata

**Location:** `DTO/Quiz/UpdateQuizDto.cs`

**Properties:**
```csharp
public class UpdateQuizDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3)]
    public required string Title { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public bool IsActive { get; set; }
}
```

**Usage Example:**
```json
{
  "title": "Updated Quiz Title",
  "description": "Updated description",
  "isActive": true
}
```

---

### QuizResponseDto

**Purpose:** Complete quiz details with all questions

**Location:** `DTO/Quiz/QuizResponseDto.cs`

**Properties:**
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

**Response Example:**
```json
{
  "id": 1,
  "title": "JavaScript Basics",
  "description": "Test your fundamental JavaScript knowledge",
  "createdAt": "2024-01-15T10:00:00Z",
  "createdById": "user-id-123",
  "createdByName": "johndoe",
  "isActive": true,
  "questionCount": 5,
  "questions": [...]
}
```

---

### QuizSummaryDto

**Purpose:** Quiz overview for list views

**Location:** `DTO/Quiz/QuizSummaryDto.cs`

**Properties:**
```csharp
public class QuizSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int QuestionCount { get; set; }
    public int AttemptCount { get; set; }
}
```

**Response Example:**
```json
{
  "id": 1,
  "title": "JavaScript Basics",
  "description": "Test your fundamental JavaScript knowledge",
  "createdAt": "2024-01-15T10:00:00Z",
  "createdByName": "johndoe",
  "isActive": true,
  "questionCount": 5,
  "attemptCount": 23
}
```

---

## Question DTOs

### CreateQuestionDto

**Purpose:** Add a question to a quiz

**Location:** `DTO/Question/CreateQuestionDto.cs`

**Properties:**
```csharp
public class CreateQuestionDto
{
    [Required(ErrorMessage = "Question text is required")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Question must be between 5 and 100 characters")]
    public required string Text { get; set; }

    [StringLength(300, ErrorMessage = "Explanation cannot exceed 300 characters")]
    public string? Explanation { get; set; }

    [Url(ErrorMessage = "Invalid image URL format")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Choices are required")]
    [MinLength(2, ErrorMessage = "Question must have at least 2 choices")]
    [MaxLength(6, ErrorMessage = "Question cannot have more than 6 choices")]
    public required List<CreateChoiceDto> Choices { get; set; }
}
```

**Validation Rules:**
- Text: 5-100 characters
- Explanation: Max 300 characters
- ImageUrl: Must be valid URL format
- Choices: 2-6 choices required
- Exactly one choice must have `isCorrect: true`

**Usage Example:**
```json
{
  "text": "What is the output of typeof null?",
  "explanation": "This is a known quirk in JavaScript",
  "imageUrl": "https://example.com/image.png",
  "choices": [
    { "text": "null", "isCorrect": false },
    { "text": "object", "isCorrect": true },
    { "text": "undefined", "isCorrect": false }
  ]
}
```

---

### QuestionResponseDto

**Purpose:** Question details with choices

**Location:** `DTO/Question/QuestionResponseDto.cs`

**Properties:**
```csharp
public class QuestionResponseDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string? ImageUrl { get; set; }
    public List<ChoiceResponseDto> Choices { get; set; } = new();
}
```

**Response Example:**
```json
{
  "id": 1,
  "text": "What is the output of typeof null?",
  "explanation": "This is a known quirk in JavaScript",
  "imageUrl": null,
  "choices": [
    { "id": 1, "text": "null", "isCorrect": false },
    { "id": 2, "text": "object", "isCorrect": true }
  ]
}
```

---

## Choice DTOs

### CreateChoiceDto

**Purpose:** Add a choice to a question

**Location:** `DTO/Choice/CreateChoiceDto.cs`

**Properties:**
```csharp
public class CreateChoiceDto
{
    [Required(ErrorMessage = "Choice text is required")]
    [StringLength(500, ErrorMessage = "Choice text cannot exceed 500 characters")]
    public required string Text { get; set; }

    [Required(ErrorMessage = "IsCorrect must be specified")]
    public bool IsCorrect { get; set; }
}
```

**Usage Example:**
```json
{
  "text": "object",
  "isCorrect": true
}
```

---

### ChoiceResponseDto

**Purpose:** Choice details in responses

**Location:** `DTO/Choice/ChoiceResponseDto.cs`

**Properties:**
```csharp
public class ChoiceResponseDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
```

**Response Example:**
```json
{
  "id": 1,
  "text": "object",
  "isCorrect": true
}
```

---

## Quiz Attempt DTOs

### StartQuizAttemptDto

**Purpose:** Start a new quiz attempt

**Location:** `DTO/Attempt/StartQuizAttemptDto.cs`

**Properties:**
```csharp
public class StartQuizAttemptDto
{
    [Required(ErrorMessage = "Quiz ID is required")]
    public int QuizId { get; set; }
}
```

**Usage Example:**
```json
{
  "quizId": 1
}
```

---

### SubmitQuizDto

**Purpose:** Submit answers for a quiz attempt

**Location:** `DTO/Attempt/SubmitQuizDto.cs`

**Properties:**
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

**Validation Rules:**
- Must provide answer for ALL questions in quiz
- QuizAttemptId must be valid and owned by user
- Attempt must not be already completed

**Usage Example:**
```json
{
  "quizAttemptId": 1,
  "answers": [
    { "questionId": 1, "choiceId": 2 },
    { "questionId": 2, "choiceId": 7 },
    { "questionId": 3, "choiceId": 11 }
  ]
}
```

---

### SubmitAnswerDto

**Purpose:** Single answer within quiz submission

**Location:** `DTO/Attempt/SubmitAnswerDto.cs`

**Properties:**
```csharp
public class SubmitAnswerDto
{
    [Required(ErrorMessage = "Question ID is required")]
    public int QuestionId { get; set; }

    [Required(ErrorMessage = "Choice ID is required")]
    public int ChoiceId { get; set; }
}
```

**Usage Example:**
```json
{
  "questionId": 1,
  "choiceId": 2
}
```

---

### QuizAttemptResponseDto

**Purpose:** Quiz attempt details

**Location:** `DTO/Attempt/QuizAttemptResponseDto.cs`

**Properties:**
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

**Response Example:**
```json
{
  "id": 1,
  "quizId": 1,
  "quizTitle": "JavaScript Basics",
  "userId": "user-id-123",
  "userName": "johndoe",
  "startedAt": "2024-01-15T14:30:00Z",
  "completedAt": "2024-01-15T14:45:00Z",
  "score": 80,
  "isCompleted": true
}
```

---

### QuizResultDto

**Purpose:** Detailed quiz results after submission

**Location:** `DTO/Attempt/QuizResultDto.cs`

**Properties:**
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

**Response Example:**
```json
{
  "quizAttemptId": 1,
  "quizTitle": "JavaScript Basics",
  "totalQuestions": 5,
  "correctAnswers": 4,
  "incorrectAnswers": 1,
  "score": 80,
  "percentage": 80.0,
  "startedAt": "2024-01-15T14:30:00Z",
  "completedAt": "2024-01-15T14:45:00Z",
  "duration": "00:15:00",
  "questionResults": [...]
}
```

---

### QuestionResultDto

**Purpose:** Individual question result details

**Location:** `DTO/Attempt/QuestionResultDto.cs`

**Properties:**
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

**Response Example:**
```json
{
  "questionId": 1,
  "questionText": "What is the output of typeof null?",
  "explanation": "This is a known quirk in JavaScript",
  "selectedChoiceId": 2,
  "selectedChoiceText": "object",
  "correctChoiceId": 2,
  "correctChoiceText": "object",
  "isCorrect": true
}
```

---

## DTO Mapping

### AutoMapper Profiles

**QuizProfile.cs**
```csharp
// Quiz -> QuizResponseDto
CreateMap<Quiz, QuizResponseDto>()
    .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy.UserName))
    .ForMember(dest => dest.QuestionCount, opt => opt.MapFrom(src => src.Questions.Count));

// Quiz -> QuizSummaryDto
CreateMap<Quiz, QuizSummaryDto>()
    .ForMember(dest => dest.AttemptCount, opt => opt.MapFrom(src => src.QuizAttempts.Count));

// CreateQuizDto -> Quiz
CreateMap<CreateQuizDto, Quiz>();
```

**QuestionProfile.cs**
```csharp
// Question -> QuestionResponseDto
CreateMap<Question, QuestionResponseDto>();

// CreateQuestionDto -> Question
CreateMap<CreateQuestionDto, Question>();
```

**ChoiceProfile.cs**
```csharp
// Choice -> ChoiceResponseDto
CreateMap<Choice, ChoiceResponseDto>();

// CreateChoiceDto -> Choice
CreateMap<CreateChoiceDto, Choice>();
```

**QuizAttemptProfile.cs**
```csharp
// QuizAttempt -> QuizAttemptResponseDto
CreateMap<QuizAttempt, QuizAttemptResponseDto>()
    .ForMember(dest => dest.QuizTitle, opt => opt.MapFrom(src => src.Quiz.Title))
    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
    .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => src.CompletedAt.HasValue));
```

---

## Validation Summary

### Common Validation Patterns

**Required Fields:**
```csharp
[Required(ErrorMessage = "Field is required")]
public required string FieldName { get; set; }
```

**String Length:**
```csharp
[StringLength(maxLength, MinimumLength = minLength, ErrorMessage = "...")]
public string FieldName { get; set; }
```

**Email:**
```csharp
[EmailAddress(ErrorMessage = "Invalid email format")]
public string Email { get; set; }
```

**URL:**
```csharp
[Url(ErrorMessage = "Invalid URL format")]
public string? ImageUrl { get; set; }
```

**Range:**
```csharp
[Range(1, 100, ErrorMessage = "Value must be between 1 and 100")]
public int Score { get; set; }
```

**List Length:**
```csharp
[MinLength(2, ErrorMessage = "At least 2 items required")]
[MaxLength(6, ErrorMessage = "Maximum 6 items allowed")]
public List<Item> Items { get; set; }
```

---

## Custom Validation

### Example: Question Validation

Beyond data annotations, business rules are enforced in the service layer:

```csharp
// In QuizService.CreateQuizAsync
foreach (var question in createDto.Questions)
{
    if (question.Choices.Count < 2 || question.Choices.Count > 6)
        return null; // Invalid

    var correctCount = question.Choices.Count(c => c.IsCorrect);
    if (correctCount != 1)
        return null; // Must have exactly one correct answer
}
```

---

## DTO Best Practices

### 1. Use Required Keyword
```csharp
public required string Title { get; set; }
```

### 2. Initialize Collections
```csharp
public List<QuestionDto> Questions { get; set; } = new();
```

### 3. Use Nullable Types Appropriately
```csharp
public string? Description { get; set; }  // Optional
public string Title { get; set; } = string.Empty;  // Required, default
```

### 4. Consistent Naming
- Request DTOs: `CreateXDto`, `UpdateXDto`
- Response DTOs: `XResponseDto`, `XSummaryDto`
- Result DTOs: `XResultDto`

### 5. Separate Request and Response DTOs
- Don't reuse DTOs for both input and output
- Prevents over-posting attacks
- Clear separation of concerns

---

## Error Response DTOs

### Validation Error Response
```json
{
  "errors": {
    "Title": [
      "The Title field is required.",
      "The Title field must be between 3 and 200 characters."
    ],
    "Questions": [
      "At least one question is required"
    ]
  },
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400
}
```

### Business Logic Error Response
```json
{
  "message": "Failed to create quiz. Each question must have exactly one correct answer."
}
```

---

**Last Updated**: 2024
**Version**: 1.0.0
