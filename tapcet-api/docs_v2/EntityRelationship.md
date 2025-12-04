# Entity Relationship Documentation

## Database Schema Overview

The TAPCET Quiz API uses a relational database schema with 6 main entities plus ASP.NET Core Identity tables. The schema supports user authentication, quiz creation, quiz attempts, and detailed answer tracking.

---

## Entity Relationship Diagram

```
????????????????????????
?   AspNetUsers        ?
?   (Identity User)    ?
????????????????????????
? Id (PK)              ?
? UserName             ?
? Email                ?
? PasswordHash         ?
? CreatedDate          ?
? TotalQuizAttempts    ?
? AverageScore         ?
????????????????????????
           ?
           ? 1:N (CreatedBy)
           ?
           ?
????????????????????????         ????????????????????????
?       Quiz           ?         ?    QuizAttempt       ?
????????????????????????         ????????????????????????
? Id (PK)              ??????????? Id (PK)              ?
? Title                ?  1:N    ? QuizId (FK)          ?
? Description          ?         ? UserId (FK)          ?
? CreatedAt            ?         ? Score                ?
? CreatedById (FK)     ?         ? StartedAt            ?
? IsActive             ?         ? CompletedAt          ?
????????????????????????         ????????????????????????
           ?                                 ?
           ? 1:N                             ? 1:N
           ?                                 ?
           ?                                 ?
????????????????????????         ????????????????????????
?      Question        ?         ?    UserAnswer        ?
????????????????????????         ????????????????????????
? Id (PK)              ??????????? Id (PK)              ?
? Text                 ?  1:N    ? QuizAttemptId (FK)   ?
? Explanation          ?         ? QuestionId (FK)      ?
? ImageUrl             ?         ? ChoiceId (FK)        ?
? QuizId (FK)          ?         ? AnsweredAt           ?
????????????????????????         ????????????????????????
           ?                                 ?
           ? 1:N                             ?
           ?                                 ?
           ?                                 ?
????????????????????????                    ?
?       Choice         ??????????????????????
????????????????????????          1:N
? Id (PK)              ?
? Text                 ?
? IsCorrect            ?
? QuestionId (FK)      ?
????????????????????????
```

---

## Entity Definitions

### 1. User (AspNetUsers)
Extends ASP.NET Core Identity User with custom properties for quiz tracking.

```csharp
public class User : IdentityUser
{
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
    public int TotalQuizAttempts { get; set; } = 0;
    public double AverageScore { get; set; } = 0.0;
    
    // Navigation Properties
    public ICollection<Quiz> CreatedQuizzes { get; set; } = new List<Quiz>();
    public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}
```

**Columns:**
- `Id` (PK, string): Primary key from Identity
- `UserName` (string, unique): Username for login
- `Email` (string, unique): User's email address
- `PasswordHash` (string): Hashed password
- `CreatedDate` (DateTimeOffset): Account creation timestamp
- `TotalQuizAttempts` (int): Total number of completed quiz attempts
- `AverageScore` (double): Average score across all attempts

**Relationships:**
- One-to-Many with Quiz (as creator)
- One-to-Many with QuizAttempt (as participant)

**Identity Integration:**
- Includes standard Identity fields (PhoneNumber, EmailConfirmed, etc.)
- Supports roles through AspNetUserRoles table

---

### 2. Quiz

Represents a quiz created by a user.

```csharp
public class Quiz
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    [Required]
    public required string CreatedById { get; set; }
    public User? CreatedBy { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}
```

**Columns:**
- `Id` (PK, int): Auto-incrementing primary key
- `Title` (string, required, max 200): Quiz title
- `Description` (string, nullable, max 2000): Quiz description
- `CreatedAt` (DateTimeOffset, default now): Creation timestamp
- `CreatedById` (FK, string, required): References User.Id
- `IsActive` (bool, default true): Whether quiz is available for attempts

**Relationships:**
- Many-to-One with User (CreatedBy)
- One-to-Many with Question
- One-to-Many with QuizAttempt

**Constraints:**
- Title is required and cannot exceed 200 characters
- CreatedById must reference a valid user
- Cannot be deleted if user is deleted (Restrict)

---

### 3. Question

Represents a single question within a quiz.

```csharp
public class Question
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string Text { get; set; }
    
    [MaxLength(300)]
    public string? Explanation { get; set; }
    
    public string? ImageUrl { get; set; }
    
    [Required]
    public int QuizId { get; set; }
    
    [ForeignKey("QuizId")]
    public Quiz? Quiz { get; set; }
    
    // Navigation Properties
    public ICollection<Choice> Choices { get; set; } = new List<Choice>();
    public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
```

**Columns:**
- `Id` (PK, int): Auto-incrementing primary key
- `Text` (string, required, max 100): Question text
- `Explanation` (string, nullable, max 300): Answer explanation
- `ImageUrl` (string, nullable): Optional image URL for visual questions
- `QuizId` (FK, int, required): References Quiz.Id

**Relationships:**
- Many-to-One with Quiz
- One-to-Many with Choice
- One-to-Many with UserAnswer

**Constraints:**
- Text is required (5-100 characters via DTO validation)
- QuizId must reference a valid quiz
- Cascading delete when quiz is deleted

---

### 4. Choice

Represents a possible answer choice for a question.

```csharp
public class Choice
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public required string Text { get; set; }
    
    public bool IsCorrect { get; set; } = false;
    
    [Required]
    public int QuestionId { get; set; }
    
    [ForeignKey("QuestionId")]
    public Question? Question { get; set; }
    
    // Navigation Properties
    public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
```

**Columns:**
- `Id` (PK, int): Auto-incrementing primary key
- `Text` (string, required, max 500): Choice text
- `IsCorrect` (bool, default false): Whether this is the correct answer
- `QuestionId` (FK, int, required): References Question.Id

**Relationships:**
- Many-to-One with Question
- One-to-Many with UserAnswer

**Constraints:**
- Text is required and cannot exceed 500 characters
- QuestionId must reference a valid question
- Cascading delete when question is deleted
- Exactly one choice per question must have IsCorrect = true (business rule)

---

### 5. QuizAttempt

Represents a user's attempt at taking a quiz.

```csharp
public class QuizAttempt
{
    [Key]
    public int Id { get; set; }
    
    public int Score { get; set; }
    
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public DateTimeOffset? CompletedAt { get; set; }
    
    [Required]
    public required string UserId { get; set; }
    
    [Required]
    public int QuizId { get; set; }
    
    [ForeignKey("UserId")]
    public User? User { get; set; }
    
    [ForeignKey("QuizId")]
    public Quiz? Quiz { get; set; }
    
    // Navigation Properties
    public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
```

**Columns:**
- `Id` (PK, int): Auto-incrementing primary key
- `Score` (int): Calculated score (0-100 percentage)
- `StartedAt` (DateTimeOffset, default now): When attempt started
- `CompletedAt` (DateTimeOffset, nullable): When attempt was submitted
- `UserId` (FK, string, required): References User.Id
- `QuizId` (FK, int, required): References Quiz.Id

**Relationships:**
- Many-to-One with User
- Many-to-One with Quiz
- One-to-Many with UserAnswer

**Constraints:**
- UserId must reference a valid user
- QuizId must reference a valid quiz
- Cascading delete when user is deleted
- Restrict delete when quiz is deleted (preserve attempt history)
- CompletedAt is null until quiz is submitted

**Business Logic:**
- Score is calculated as (CorrectAnswers / TotalQuestions) * 100
- Duration = CompletedAt - StartedAt

---

### 6. UserAnswer

Represents a user's answer to a specific question in a quiz attempt.

```csharp
public class UserAnswer
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int QuizAttemptId { get; set; }
    
    [Required]
    public int QuestionId { get; set; }
    
    [Required]
    public int ChoiceId { get; set; }
    
    public DateTimeOffset AnsweredAt { get; set; } = DateTimeOffset.UtcNow;
    
    [ForeignKey("QuizAttemptId")]
    public QuizAttempt? QuizAttempt { get; set; }
    
    [ForeignKey("QuestionId")]
    public Question? Question { get; set; }
    
    [ForeignKey("ChoiceId")]
    public Choice? Choice { get; set; }
}
```

**Columns:**
- `Id` (PK, int): Auto-incrementing primary key
- `QuizAttemptId` (FK, int, required): References QuizAttempt.Id
- `QuestionId` (FK, int, required): References Question.Id
- `ChoiceId` (FK, int, required): References Choice.Id
- `AnsweredAt` (DateTimeOffset, default now): Answer timestamp

**Relationships:**
- Many-to-One with QuizAttempt
- Many-to-One with Question
- Many-to-One with Choice

**Constraints:**
- QuizAttemptId must reference a valid quiz attempt
- QuestionId must reference a valid question
- ChoiceId must reference a valid choice
- Cascading delete when quiz attempt is deleted
- Restrict delete when question or choice is deleted
- One answer per question per attempt (business rule)

---

## Relationship Details

### Delete Behaviors

| Parent Entity | Child Entity | Delete Behavior | Rationale |
|--------------|--------------|-----------------|-----------|
| User | Quiz | Restrict | Preserve quiz even if creator account is deleted |
| User | QuizAttempt | Cascade | User's attempts belong to them |
| Quiz | Question | Cascade | Questions are part of quiz structure |
| Quiz | QuizAttempt | Restrict | Preserve attempt history even if quiz is deleted |
| Question | Choice | Cascade | Choices are part of question structure |
| Question | UserAnswer | Restrict | Preserve answer history even if question is modified |
| QuizAttempt | UserAnswer | Cascade | Answers belong to the attempt |
| Choice | UserAnswer | Restrict | Preserve answer history even if choice is modified |

### Foreign Key Constraints

```sql
-- Quiz to User
ALTER TABLE Quiz
ADD CONSTRAINT FK_Quiz_User_CreatedById
FOREIGN KEY (CreatedById) REFERENCES AspNetUsers(Id)
ON DELETE RESTRICT;

-- Question to Quiz
ALTER TABLE Question
ADD CONSTRAINT FK_Question_Quiz_QuizId
FOREIGN KEY (QuizId) REFERENCES Quiz(Id)
ON DELETE CASCADE;

-- Choice to Question
ALTER TABLE Choice
ADD CONSTRAINT FK_Choice_Question_QuestionId
FOREIGN KEY (QuestionId) REFERENCES Question(Id)
ON DELETE CASCADE;

-- QuizAttempt to User
ALTER TABLE QuizAttempt
ADD CONSTRAINT FK_QuizAttempt_User_UserId
FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
ON DELETE CASCADE;

-- QuizAttempt to Quiz
ALTER TABLE QuizAttempt
ADD CONSTRAINT FK_QuizAttempt_Quiz_QuizId
FOREIGN KEY (QuizId) REFERENCES Quiz(Id)
ON DELETE RESTRICT;

-- UserAnswer to QuizAttempt
ALTER TABLE UserAnswer
ADD CONSTRAINT FK_UserAnswer_QuizAttempt_QuizAttemptId
FOREIGN KEY (QuizAttemptId) REFERENCES QuizAttempt(Id)
ON DELETE CASCADE;

-- UserAnswer to Question
ALTER TABLE UserAnswer
ADD CONSTRAINT FK_UserAnswer_Question_QuestionId
FOREIGN KEY (QuestionId) REFERENCES Question(Id)
ON DELETE RESTRICT;

-- UserAnswer to Choice
ALTER TABLE UserAnswer
ADD CONSTRAINT FK_UserAnswer_Choice_ChoiceId
FOREIGN KEY (ChoiceId) REFERENCES Choice(Id)
ON DELETE RESTRICT;
```

---

## Indexes

### Recommended Indexes for Performance

```sql
-- Quiz indexes
CREATE INDEX IX_Quiz_CreatedById ON Quiz(CreatedById);
CREATE INDEX IX_Quiz_IsActive ON Quiz(IsActive);
CREATE INDEX IX_Quiz_CreatedAt ON Quiz(CreatedAt DESC);

-- Question indexes
CREATE INDEX IX_Question_QuizId ON Question(QuizId);

-- Choice indexes
CREATE INDEX IX_Choice_QuestionId ON Choice(QuestionId);

-- QuizAttempt indexes
CREATE INDEX IX_QuizAttempt_UserId ON QuizAttempt(UserId);
CREATE INDEX IX_QuizAttempt_QuizId ON QuizAttempt(QuizId);
CREATE INDEX IX_QuizAttempt_Score ON QuizAttempt(Score DESC);
CREATE INDEX IX_QuizAttempt_CompletedAt ON QuizAttempt(CompletedAt);

-- UserAnswer indexes
CREATE INDEX IX_UserAnswer_QuizAttemptId ON UserAnswer(QuizAttemptId);
CREATE INDEX IX_UserAnswer_QuestionId ON UserAnswer(QuestionId);
CREATE INDEX IX_UserAnswer_ChoiceId ON UserAnswer(ChoiceId);

-- Composite index for leaderboard queries
CREATE INDEX IX_QuizAttempt_QuizId_Score_CompletedAt 
ON QuizAttempt(QuizId, Score DESC, CompletedAt);
```

---

## Data Integrity Rules

### Application-Level Constraints

1. **Quiz Creation**
   - Must have at least 1 question
   - Each question must have 2-6 choices
   - Each question must have exactly 1 correct choice

2. **Quiz Attempts**
   - Can only start attempt for active quizzes
   - Cannot submit same attempt twice
   - Must answer all questions in quiz

3. **Scoring**
   - Score = (Correct Answers / Total Questions) * 100
   - Rounded to nearest integer
   - Stored as 0-100 value

4. **User Statistics**
   - TotalQuizAttempts incremented on completion
   - AverageScore recalculated after each completion
   - Only completed attempts count toward statistics

---

## Sample Data Queries

### Get Quiz with All Questions and Choices
```csharp
var quiz = await _context.Quizzes
    .Include(q => q.CreatedBy)
    .Include(q => q.Questions)
        .ThenInclude(q => q.Choices)
    .FirstOrDefaultAsync(q => q.Id == quizId);
```

### Get Complete Quiz Attempt with Answers
```csharp
var attempt = await _context.QuizAttempts
    .Include(a => a.Quiz)
        .ThenInclude(q => q.Questions)
            .ThenInclude(q => q.Choices)
    .Include(a => a.UserAnswers)
        .ThenInclude(ua => ua.Choice)
    .Include(a => a.User)
    .FirstOrDefaultAsync(a => a.Id == attemptId);
```

### Get Quiz Leaderboard
```csharp
var leaderboard = await _context.QuizAttempts
    .Include(a => a.User)
    .Where(a => a.QuizId == quizId && a.CompletedAt.HasValue)
    .OrderByDescending(a => a.Score)
    .ThenBy(a => a.CompletedAt!.Value - a.StartedAt)
    .Take(10)
    .ToListAsync();
```

---

## Migration History

The database schema is managed through Entity Framework Core migrations:

1. **20251109063440_InitialCreate** - Initial Identity tables
2. **20251110134430_models** - Quiz domain entities

To apply migrations:
```bash
dotnet ef database update
```

To create new migration:
```bash
dotnet ef migrations add MigrationName
```

---

## Database Maintenance

### Backup Recommendations
- Daily backups of TapcetDb
- Backup before major migrations
- Keep at least 30 days of backup history

### Data Retention
Consider implementing:
- Archive old quiz attempts (> 1 year)
- Soft delete for quizzes (mark as deleted instead of removing)
- Audit trail for user actions

---

**Last Updated**: 2024
**Schema Version**: 1.0.0
