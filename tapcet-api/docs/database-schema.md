# Database Schema

## Entity Relationship Diagram

```
???????????????????
?     Users       ?
?  (Identity)     ?
???????????????????
? Id (PK)         ????????????
? UserName        ?          ?
? Email           ?          ?
? PasswordHash    ?          ?
? CreatedDate     ?          ?
? TotalQuizAttempts?         ?
? AverageScore    ?          ?
???????????????????          ?
         ?                   ?
         ? 1                 ?
         ?                   ?
         ? *                 ?
         ?                   ?
???????????????????          ?
?    Quizzes      ?          ?
???????????????????          ?
? Id (PK)         ?          ?
? Title           ?          ?
? Description     ?          ?
? CreatedAt       ?          ?
? CreatedById (FK)????????????
? IsActive        ?
???????????????????
         ? 1
         ?
         ? *
???????????????????
?   Questions     ?
???????????????????
? Id (PK)         ?
? QuizId (FK)     ?
? Text            ?
? Explanation     ?
? ImageUrl        ?
???????????????????
         ? 1
         ?
         ? 2-6
???????????????????
?    Choices      ?
???????????????????
? Id (PK)         ?
? QuestionId (FK) ?
? Text            ?
? IsCorrect       ?
???????????????????

???????????????????          ????????????????????
?  QuizAttempts   ?          ?   UserAnswers    ?
???????????????????          ????????????????????
? Id (PK)         ???????????? Id (PK)          ?
? QuizId (FK)     ?     ?    ? QuizAttemptId(FK)?
? UserId (FK)     ?     ?????? QuestionId (FK)  ?
? StartedAt       ?          ? ChoiceId (FK)    ?
? CompletedAt     ?          ? AnsweredAt       ?
? Score           ?          ????????????????????
???????????????????
```

## Entity Definitions

### Users

Extends ASP.NET Core Identity `IdentityUser` with additional properties.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | string | No | Primary key (GUID) |
| UserName | string | No | Unique username |
| Email | string | No | Unique email address |
| PasswordHash | string | No | Hashed password |
| CreatedDate | DateTime | No | Account creation timestamp |
| TotalQuizAttempts | int | No | Count of completed attempts |
| AverageScore | double | No | Average score across all attempts |

**Relationships**:
- One-to-Many with Quizzes (as creator)
- One-to-Many with QuizAttempts (as participant)

**Indexes**:
- Unique index on Email
- Unique index on UserName

### Quizzes

Represents a quiz with metadata.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | int | No | Primary key (auto-increment) |
| Title | string(200) | No | Quiz title |
| Description | string(2000) | Yes | Quiz description |
| CreatedAt | DateTimeOffset | No | Creation timestamp (UTC) |
| CreatedById | string | No | Foreign key to Users |
| IsActive | bool | No | Whether quiz is available for attempts |

**Relationships**:
- Many-to-One with Users (Creator)
- One-to-Many with Questions
- One-to-Many with QuizAttempts

**Default Values**:
- IsActive: true
- CreatedAt: DateTimeOffset.UtcNow

**Business Rules**:
- Title is required and must be 3-200 characters
- Description is optional, max 2000 characters
- Must have at least 1 question to be startable

### Questions

Represents a question within a quiz.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | int | No | Primary key (auto-increment) |
| QuizId | int | No | Foreign key to Quizzes |
| Text | string(2000) | No | Question text |
| Explanation | string(2000) | Yes | Explanation of correct answer |
| ImageUrl | string(500) | Yes | Optional question image URL |

**Relationships**:
- Many-to-One with Quizzes
- One-to-Many with Choices
- One-to-Many with UserAnswers

**Business Rules**:
- Text is required, max 2000 characters
- Explanation is optional, max 2000 characters
- Must have between 2-6 choices
- Must have exactly 1 correct choice

**Cascade Behavior**:
- Deleting a Quiz deletes all Questions
- Deleting a Question deletes all Choices

### Choices

Represents answer options for a question.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | int | No | Primary key (auto-increment) |
| QuestionId | int | No | Foreign key to Questions |
| Text | string(500) | No | Choice text |
| IsCorrect | bool | No | Whether this is the correct answer |

**Relationships**:
- Many-to-One with Questions
- One-to-Many with UserAnswers

**Business Rules**:
- Text is required, max 500 characters
- Each question must have exactly 1 IsCorrect = true
- Each question must have 2-6 total choices

**Default Values**:
- IsCorrect: false

**Cascade Behavior**:
- Deleting a Question deletes all Choices

### QuizAttempts

Tracks user attempts at quizzes.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | int | No | Primary key (auto-increment) |
| QuizId | int | No | Foreign key to Quizzes |
| UserId | string | No | Foreign key to Users |
| StartedAt | DateTimeOffset | No | Attempt start time (UTC) |
| CompletedAt | DateTimeOffset | Yes | Attempt completion time (UTC) |
| Score | int | No | Percentage score (0-100) |

**Relationships**:
- Many-to-One with Quizzes
- Many-to-One with Users
- One-to-Many with UserAnswers

**Computed Properties**:
- IsCompleted: CompletedAt != null
- Duration: CompletedAt - StartedAt

**Business Rules**:
- User can have multiple attempts per quiz
- Score is calculated as (CorrectAnswers / TotalQuestions) * 100
- CompletedAt is null until quiz is submitted

**Default Values**:
- StartedAt: DateTimeOffset.UtcNow
- Score: 0
- CompletedAt: null

**Cascade Behavior**:
- Deleting a Quiz may fail if QuizAttempts exist (referential integrity)
- Deleting a User may fail if QuizAttempts exist (referential integrity)

### UserAnswers

Records individual question answers within an attempt.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | int | No | Primary key (auto-increment) |
| QuizAttemptId | int | No | Foreign key to QuizAttempts |
| QuestionId | int | No | Foreign key to Questions |
| ChoiceId | int | No | Foreign key to Choices |
| AnsweredAt | DateTimeOffset | No | Answer submission time (UTC) |

**Relationships**:
- Many-to-One with QuizAttempts
- Many-to-One with Questions
- Many-to-One with Choices

**Business Rules**:
- One answer per question per attempt
- Cannot answer questions not in the quiz
- Cannot submit after attempt is completed

**Default Values**:
- AnsweredAt: DateTimeOffset.UtcNow

**Cascade Behavior**:
- Deleting a QuizAttempt deletes all UserAnswers
- Deleting a Question does not cascade (referential integrity)
- Deleting a Choice does not cascade (referential integrity)

## Database Constraints

### Primary Keys

All tables use auto-incrementing integers except Users (GUID string).

### Foreign Keys

All foreign keys have referential integrity constraints:
- ON DELETE RESTRICT for Users, Quizzes, Questions, Choices
- ON DELETE CASCADE for dependent entities (Questions?Choices, QuizAttempts?UserAnswers)

### Unique Constraints

- Users.Email (unique)
- Users.UserName (unique)

### Check Constraints

Enforced at application level:
- Quiz.Title length: 3-200 characters
- Question.Text: 1-2000 characters
- Choice.Text: 1-500 characters
- Score: 0-100
- QuizAttempt: Must answer all questions

## Indexes

### Automatically Created

- Primary key indexes on all tables
- Foreign key indexes on all relationships

### Recommended Additional Indexes

For performance optimization:

```sql
-- Frequently queried columns
CREATE INDEX IX_Quizzes_IsActive ON Quizzes(IsActive);
CREATE INDEX IX_QuizAttempts_UserId ON QuizAttempts(UserId);
CREATE INDEX IX_QuizAttempts_QuizId ON QuizAttempts(QuizId);
CREATE INDEX IX_QuizAttempts_Score ON QuizAttempts(Score DESC);
CREATE INDEX IX_QuizAttempts_CompletedAt ON QuizAttempts(CompletedAt);

-- Composite indexes for common queries
CREATE INDEX IX_QuizAttempts_Quiz_Score 
    ON QuizAttempts(QuizId, Score DESC, CompletedAt);
```

## Migration History

### Initial Migration (20251109063440_InitialCreate)

Creates all base tables with Identity integration.

### Models Update (20251110134430_models)

Updates to entity models and relationships.

## Database Initialization

### Seed Data

The application seeds the following data on first run:

**Roles**:
- Admin
- User

**Default Admin User**:
- Email: admin@tapcet.com
- UserName: admin
- Password: Admin@123
- Role: Admin

## Query Patterns

### Common EF Core Queries

**Get Quiz with Questions and Choices**:
```csharp
var quiz = await _context.Quizzes
    .Include(q => q.Questions)
        .ThenInclude(q => q.Choices)
    .FirstOrDefaultAsync(q => q.Id == quizId);
```

**Get User Attempts with Quiz Info**:
```csharp
var attempts = await _context.QuizAttempts
    .Include(a => a.Quiz)
    .Include(a => a.User)
    .Where(a => a.UserId == userId)
    .OrderByDescending(a => a.StartedAt)
    .ToListAsync();
```

**Get Leaderboard**:
```csharp
var leaderboard = await _context.QuizAttempts
    .Include(a => a.User)
    .Where(a => a.QuizId == quizId && a.CompletedAt.HasValue)
    .OrderByDescending(a => a.Score)
    .ThenBy(a => a.CompletedAt.Value - a.StartedAt)
    .Take(topCount)
    .ToListAsync();
```

## Database Maintenance

### Backup Strategy

Recommended backup procedures:

```bash
# Full database backup
docker exec tapcet-pg pg_dump -U tapcet TapcetDb > backup.sql

# Restore from backup
docker exec -i tapcet-pg psql -U tapcet TapcetDb < backup.sql
```

### Performance Monitoring

Monitor these metrics:
- Query execution time
- Connection pool usage
- Index usage statistics
- Table scan frequency

### Database Growth

Expected growth rates:
- Users: Low (user registrations)
- Quizzes: Low (content creation)
- Questions: Medium (proportional to quizzes)
- Choices: Medium (proportional to questions)
- QuizAttempts: High (user activity)
- UserAnswers: High (proportional to attempts × questions)

Plan for archiving UserAnswers and QuizAttempts after 1-2 years.
