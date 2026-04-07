# Database Schema

**Database:** PostgreSQL
**Connection (dev):** `Host=127.0.0.1;Port=6543;Database=TapcetDb;Username=tapcet`
**ORM:** EF Core 8, code-first migrations

---

## Entity Relationship Diagram

```
Subject (1) ──────< Course (1) ──────< Unit (1) ──────< Quiz
                                                          │
User >──── created ──────────────────────────────────────┘
 │
 └──< QuizAttempt >────────────── Quiz
           │
           └──< UserAnswer >───── Question
                          └────── Choice

Quiz (1) ──────< Question (1) ──────< Choice
```

---

## Tables

### AspNetUsers (User extends IdentityUser)

| Column | Type | Constraint |
|--------|------|-----------|
| `Id` | `nvarchar` (GUID) | PK (Identity) |
| `UserName` | `nvarchar(256)` | Unique, required |
| `Email` | `nvarchar(256)` | Unique, required |
| `CreatedDate` | `timestamptz` | Default: `UtcNow` |
| `TotalQuizAttempts` | `int` | Default: 0 |
| `AverageScore` | `double` | Default: 0.0 |
| + standard Identity columns | | (PasswordHash, etc.) |

---

### Subjects

| Column | Type | Constraint |
|--------|------|-----------|
| `Id` | `int` | PK, auto-increment |
| `Name` | `nvarchar(100)` | Required |
| `Description` | `nvarchar(500)` | Nullable |

---

### Courses

| Column | Type | Constraint |
|--------|------|-----------|
| `Id` | `int` | PK |
| `Title` | `nvarchar(100)` | Required |
| `Description` | `nvarchar(500)` | Nullable |
| `SubjectId` | `int` | FK → Subjects |

---

### Units

| Column | Type | Constraint |
|--------|------|-----------|
| `Id` | `int` | PK |
| `Title` | `nvarchar(100)` | Required |
| `OrderIndex` | `int` | Position within course |
| `CourseId` | `int` | FK → Courses |

---

### Quizzes

| Column | Type | Constraint |
|--------|------|-----------|
| `Id` | `int` | PK |
| `Title` | `nvarchar(200)` | Required |
| `Description` | `nvarchar(2000)` | Nullable |
| `CreatedAt` | `timestamptz` | Default: `UtcNow` |
| `IsActive` | `bool` | Default: `true` |
| `OrderIndex` | `int` | Position within unit (0 if standalone) |
| `CreatedById` | `nvarchar` (GUID) | FK → AspNetUsers, **RESTRICT** |
| `UnitId` | `int?` | FK → Units, nullable, **SET NULL** on unit delete |

---

### Questions

| Column | Type | Constraint |
|--------|------|-----------|
| `Id` | `int` | PK |
| `Text` | `nvarchar(1000)` | Required |
| `Explanation` | `nvarchar(1000)` | Nullable |
| `ImageUrl` | `nvarchar` | Nullable |
| `QuizId` | `int` | FK → Quizzes, **CASCADE** |

---

### Choices

| Column | Type | Constraint |
|--------|------|-----------|
| `Id` | `int` | PK |
| `Text` | `nvarchar(500)` | Required |
| `IsCorrect` | `bool` | Default: `false` — exactly one per question must be `true` |
| `QuestionId` | `int` | FK → Questions, **CASCADE** |

---

### QuizAttempts

| Column | Type | Constraint |
|--------|------|-----------|
| `Id` | `int` | PK |
| `Score` | `int` | 0–100, calculated on submission |
| `StartedAt` | `timestamptz` | Default: `UtcNow` |
| `CompletedAt` | `timestamptz?` | Nullable — null means in progress |
| `UserId` | `nvarchar` (GUID) | FK → AspNetUsers, **RESTRICT** |
| `QuizId` | `int` | FK → Quizzes, **RESTRICT** |

---

### UserAnswers

| Column | Type | Constraint |
|--------|------|-----------|
| `Id` | `int` | PK |
| `AnsweredAt` | `timestamptz` | Default: `UtcNow` |
| `QuizAttemptId` | `int` | FK → QuizAttempts, **CASCADE** |
| `QuestionId` | `int` | FK → Questions, **RESTRICT** |
| `ChoiceId` | `int` | FK → Choices, **RESTRICT** |

---

## Cascade / Delete Behavior Summary

| Relationship | On parent delete |
|---|---|
| Subject → Course | **RESTRICT** — cannot delete a Subject with Courses |
| Course → Unit | **RESTRICT** — cannot delete a Course with Units |
| Unit → Quiz | **SET NULL** — Quiz becomes standalone (`UnitId = null`) |
| Quiz → Question | **CASCADE** — Questions deleted with Quiz |
| Question → Choice | **CASCADE** — Choices deleted with Question |
| User → Quiz (creator) | **RESTRICT** — cannot delete User who has Quizzes |
| User → QuizAttempt | **RESTRICT** |
| Quiz → QuizAttempt | **RESTRICT** — cannot delete Quiz that has been attempted |
| QuizAttempt → UserAnswer | **CASCADE** |
| Question → UserAnswer | **RESTRICT** |
| Choice → UserAnswer | **RESTRICT** |

---

## Migration Commands

```bash
# Apply all pending migrations
dotnet ef database update

# Create a new migration after a model change
dotnet ef migrations add <DescriptiveName>

# Roll back to a specific migration
dotnet ef database update <MigrationName>

# List all migrations and their status
dotnet ef migrations list

# Remove the last migration (if not yet applied)
dotnet ef migrations remove
```
