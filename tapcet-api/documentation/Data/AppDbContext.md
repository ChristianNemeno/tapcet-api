# ApplicationDbContext Documentation

## Overview

**File Location**: `tapcet-api/Data/ApplicationDbContext.cs`

`ApplicationDbContext` is the main database context class for the Tapcet Quiz API. It inherits from `IdentityDbContext<User>` to provide authentication and authorization features while managing all quiz-related entities and the educational hierarchy system.

---

## Class Declaration

```csharp
public class ApplicationDbContext : IdentityDbContext<User>
```

### Inheritance
- **Base Class**: `IdentityDbContext<User>`
- **Purpose**: Provides ASP.NET Core Identity features (user authentication, roles, claims) with a custom `User` model
- **Benefit**: Automatically includes tables for users, roles, claims, and authentication tokens

---

## Constructor

```csharp
public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
{
}
```

### Purpose
- Accepts configuration options (connection string, database provider, etc.)
- Passes options to the base `IdentityDbContext` constructor
- Called by dependency injection when the application starts

### Typical Usage
Configured in `Program.cs`:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
```

---

## DbSet Properties

DbSets represent tables in the database. Each property maps to a database table and allows CRUD operations.

### Quiz System Entities

```csharp
public DbSet<Quiz> Quizzes { get; set; }
```
- **Table**: `Quizzes`
- **Purpose**: Stores quiz information (title, description, creator, active status)
- **Key Relationships**: Created by User, contains Questions, belongs to Unit (optional)

```csharp
public DbSet<Question> Questions { get; set; }
```
- **Table**: `Questions`
- **Purpose**: Stores individual quiz questions with text, explanation, and optional image
- **Key Relationships**: Belongs to Quiz, has multiple Choices

```csharp
public DbSet<Choice> Choices { get; set; }
```
- **Table**: `Choices`
- **Purpose**: Stores answer options for questions (text and correct/incorrect flag)
- **Key Relationships**: Belongs to Question

```csharp
public DbSet<QuizAttempt> QuizAttempts { get; set; }
```
- **Table**: `QuizAttempts`
- **Purpose**: Tracks when users take quizzes (start time, completion time, score)
- **Key Relationships**: Belongs to User and Quiz, contains UserAnswers

```csharp
public DbSet<UserAnswer> UserAnswers { get; set; }
```
- **Table**: `UserAnswers`
- **Purpose**: Records user's selected answers during quiz attempts
- **Key Relationships**: Belongs to QuizAttempt, references Question and Choice

### Educational Hierarchy Entities

```csharp
public DbSet<Subject> Subjects { get; set; }
```
- **Table**: `Subjects`
- **Purpose**: Top-level categories (e.g., "Science", "Mathematics", "Programming")
- **Key Relationships**: Contains multiple Courses

```csharp
public DbSet<Course> Courses { get; set; }
```
- **Table**: `Courses`
- **Purpose**: Learning tracks within subjects (e.g., "High School Physics")
- **Key Relationships**: Belongs to Subject, contains multiple Units

```csharp
public DbSet<Unit> Units { get; set; }
```
- **Table**: `Units`
- **Purpose**: Modules or chapters within courses (e.g., "Forces and Newton's Laws")
- **Key Relationships**: Belongs to Course, contains multiple Quizzes

---

## OnModelCreating Method

This method configures entity relationships, constraints, and database behavior using Fluent API.

```csharp
protected override void OnModelCreating(ModelBuilder builder)
```

### Purpose
- Define relationships between entities (foreign keys)
- Configure cascade delete behavior
- Set up indexes and constraints
- Called automatically when EF Core builds the database model

---

## Relationship Configurations

### 1. Quiz System Relationships

#### Quiz → User (Creator)

```csharp
builder.Entity<Quiz>()
    .HasOne(q => q.CreatedBy)           // One User
    .WithMany(u => u.CreatedQuizzes)    // Many Quizzes
    .HasForeignKey(q => q.CreatedById)
    .OnDelete(DeleteBehavior.Restrict); // ⚠️ Cannot delete user if they have quizzes
```

**Relationship Type**: One-to-Many  
**Delete Behavior**: `Restrict` - Prevents deleting a user who has created quizzes  
**Reason**: Preserves quiz ownership and creator history

---

#### Question → Quiz

```csharp
builder.Entity<Question>()
    .HasOne(q => q.Quiz)              // One Quiz
    .WithMany(qz => qz.Questions)     // Many Questions
    .HasForeignKey(q => q.QuizId)
    .OnDelete(DeleteBehavior.Cascade); // ✅ Deleting quiz deletes all questions
```

**Relationship Type**: One-to-Many  
**Delete Behavior**: `Cascade` - Deleting a quiz automatically deletes all its questions  
**Reason**: Questions don't make sense without their parent quiz

---

#### Choice → Question

```csharp
builder.Entity<Choice>()
   .HasOne(c => c.Question)           // One Question
   .WithMany(q => q.Choices)          // Many Choices
   .HasForeignKey(c => c.QuestionId)
   .OnDelete(DeleteBehavior.Cascade); // ✅ Deleting question deletes all choices
```

**Relationship Type**: One-to-Many  
**Delete Behavior**: `Cascade` - Deleting a question automatically deletes all answer choices  
**Reason**: Choices are meaningless without their question

---

#### QuizAttempt → User

```csharp
builder.Entity<QuizAttempt>()
    .HasOne(qa => qa.User)             // One User
    .WithMany(u => u.QuizAttempts)     // Many QuizAttempts
    .HasForeignKey(qa => qa.UserId)
    .OnDelete(DeleteBehavior.Cascade); // ✅ Deleting user deletes their attempts
```

**Relationship Type**: One-to-Many  
**Delete Behavior**: `Cascade` - Deleting a user removes all their quiz attempts  
**Reason**: Attempts belong to the user; no user, no attempts

---

#### QuizAttempt → Quiz

```csharp
builder.Entity<QuizAttempt>()
    .HasOne(qa => qa.Quiz)             // One Quiz
    .WithMany(q => q.QuizAttempts)     // Many QuizAttempts
    .HasForeignKey(qa => qa.QuizId)
    .OnDelete(DeleteBehavior.Restrict); // ⚠️ Cannot delete quiz with attempts
```

**Relationship Type**: One-to-Many  
**Delete Behavior**: `Restrict` - Prevents deleting quizzes that have been attempted  
**Reason**: Preserves historical data and user progress

---

#### UserAnswer → QuizAttempt

```csharp
builder.Entity<UserAnswer>()
    .HasOne(ua => ua.QuizAttempt)        // One QuizAttempt
    .WithMany(qa => qa.UserAnswers)      // Many UserAnswers
    .HasForeignKey(ua => ua.QuizAttemptId)
    .OnDelete(DeleteBehavior.Cascade);   // ✅ Deleting attempt deletes answers
```

**Relationship Type**: One-to-Many  
**Delete Behavior**: `Cascade` - Deleting a quiz attempt removes all associated answers  
**Reason**: Answers are part of the attempt; no attempt, no answers

---

#### UserAnswer → Question

```csharp
builder.Entity<UserAnswer>()
    .HasOne(ua => ua.Question)           // One Question
    .WithMany(q => q.UserAnswers)        // Many UserAnswers
    .HasForeignKey(ua => ua.QuestionId)
    .OnDelete(DeleteBehavior.Restrict);  // ⚠️ Cannot delete question with answers
```

**Relationship Type**: One-to-Many  
**Delete Behavior**: `Restrict` - Prevents deleting questions that have been answered  
**Reason**: Preserves quiz attempt integrity and historical data

---

#### UserAnswer → Choice

```csharp
builder.Entity<UserAnswer>()
    .HasOne(ua => ua.Choice)             // One Choice
    .WithMany(c => c.UserAnswers)        // Many UserAnswers
    .HasForeignKey(ua => ua.ChoiceId)
    .OnDelete(DeleteBehavior.Restrict);  // ⚠️ Cannot delete choice with answers
```

**Relationship Type**: One-to-Many  
**Delete Behavior**: `Restrict` - Prevents deleting choices that users have selected  
**Reason**: Maintains answer integrity in quiz history

---

### 2. Educational Hierarchy Relationships

#### Subject → Course

```csharp
builder.Entity<Course>()
    .HasOne(c => c.Subject)              // One Subject
    .WithMany(s => s.Courses)            // Many Courses
    .HasForeignKey(c => c.SubjectId)
    .OnDelete(DeleteBehavior.Restrict);  // ⚠️ Cannot delete subject with courses
```

**Relationship Type**: One-to-Many  
**Delete Behavior**: `Restrict` - Prevents deleting subjects that have courses  
**Reason**: Must explicitly remove or reassign courses before deleting subject  
**Example**: Cannot delete "Science" subject if it has "Physics" course

---

#### Course → Unit

```csharp
builder.Entity<Unit>()
    .HasOne(u => u.Course)               // One Course
    .WithMany(c => c.Units)              // Many Units
    .HasForeignKey(u => u.CourseId)
    .OnDelete(DeleteBehavior.Cascade);   // ✅ Deleting course deletes all units
```

**Relationship Type**: One-to-Many  
**Delete Behavior**: `Cascade` - Deleting a course automatically removes all units  
**Reason**: Units are organizational components of the course  
**Example**: Deleting "Physics" course removes "Unit 1: Forces", "Unit 2: Energy", etc.

---

#### Unit → Quiz (Optional)

```csharp
builder.Entity<Quiz>()
    .HasOne(q => q.Unit)                 // One Unit (nullable)
    .WithMany(u => u.Quizzes)            // Many Quizzes
    .HasForeignKey(q => q.UnitId)
    .OnDelete(DeleteBehavior.SetNull);   // 🔄 Deleting unit makes quizzes standalone
```

**Relationship Type**: One-to-Many (Optional)  
**Delete Behavior**: `SetNull` - Deleting a unit sets `UnitId` to `null` for quizzes  
**Reason**: Preserves quizzes as standalone when unit is removed  
**Example**: Deleting "Unit 1: Forces" doesn't delete quizzes, just orphans them

---

## Delete Behavior Summary

| Behavior | Symbol | Meaning | Used For |
|----------|--------|---------|----------|
| **Cascade** | ✅ | Delete child when parent is deleted | Question→Quiz, Choice→Question, UserAnswer→Attempt, Unit→Course |
| **Restrict** | ⚠️ | Prevent parent deletion if children exist | Quiz→User, Attempt→Quiz, Subject→Course |
| **SetNull** | 🔄 | Set foreign key to null when parent deleted | Quiz→Unit |

---

## Entity Relationship Diagram

```
User (ASP.NET Identity)
 ├─ Creates → Quiz (Restrict)
 └─ Takes → QuizAttempt (Cascade)
            └─ Contains → UserAnswer (Cascade)

Subject
 └─ Contains → Course (Restrict)
               └─ Contains → Unit (Cascade)
                            └─ Contains → Quiz (SetNull, optional)

Quiz
 ├─ Contains → Question (Cascade)
 │             └─ Contains → Choice (Cascade)
 └─ Has → QuizAttempt (Restrict)
          └─ Contains → UserAnswer (Cascade)
                       ├─ References → Question (Restrict)
                       └─ References → Choice (Restrict)
```

---

## Key Design Decisions

### 1. **Restrict on Quiz → User**
- **Why**: Prevents accidental deletion of users who have created content
- **Alternative**: Could use `Cascade` to delete all quizzes when user is deleted, but this loses educational content

### 2. **Restrict on QuizAttempt → Quiz**
- **Why**: Preserves quiz history and user progress
- **Alternative**: Could use `Cascade` to allow quiz deletion, but loses valuable analytics

### 3. **SetNull on Quiz → Unit**
- **Why**: Allows flexible quiz management (standalone or organized)
- **Alternative**: `Cascade` would delete quizzes when unit is removed, losing content

### 4. **Cascade on Course → Unit**
- **Why**: Units are organizational structure; deleting course should remove structure
- **Alternative**: `Restrict` would require manual cleanup before course deletion

---

## Common Operations

### Adding Data

```csharp
// Create a new quiz
var quiz = new Quiz { Title = "Newton's Laws", CreatedById = userId };
context.Quizzes.Add(quiz);
await context.SaveChangesAsync();
```

### Querying with Relationships

```csharp
// Get quiz with questions and choices
var quiz = await context.Quizzes
    .Include(q => q.Questions)
        .ThenInclude(q => q.Choices)
    .FirstOrDefaultAsync(q => q.Id == quizId);

// Get subject with courses and units
var subject = await context.Subjects
    .Include(s => s.Courses)
        .ThenInclude(c => c.Units)
    .FirstOrDefaultAsync(s => s.Id == subjectId);
```

### Handling Deletes

```csharp
// ✅ Safe delete - cascades to questions and choices
context.Quizzes.Remove(quiz);
await context.SaveChangesAsync();

// ⚠️ Will throw exception if quiz has attempts
// Must handle QuizAttempts first

// 🔄 Deleting unit orphans quizzes
context.Units.Remove(unit);
await context.SaveChangesAsync();
// Quizzes in this unit now have UnitId = null
```

---

## Migrations

When this context is modified, create and apply migrations:

```bash
# Create migration
dotnet ef migrations add MigrationName

# Apply to database
dotnet ef database update

# View SQL that will be executed
dotnet ef migrations script
```

---

## Best Practices

### 1. **Always Use Include for Related Data**
```csharp
// ❌ Bad - causes N+1 queries
var quizzes = await context.Quizzes.ToListAsync();
foreach (var quiz in quizzes) {
    var questions = quiz.Questions; // Separate query for each quiz
}

// ✅ Good - single query
var quizzes = await context.Quizzes
    .Include(q => q.Questions)
    .ToListAsync();
```

### 2. **Use AsNoTracking for Read-Only Queries**
```csharp
// ✅ Better performance for read-only data
var subjects = await context.Subjects
    .AsNoTracking()
    .ToListAsync();
```

### 3. **Handle Restrict Deletions Gracefully**
```csharp
// Check before deleting
var hasAttempts = await context.QuizAttempts.AnyAsync(a => a.QuizId == quizId);
if (hasAttempts) {
    return BadRequest("Cannot delete quiz with existing attempts");
}
context.Quizzes.Remove(quiz);
await context.SaveChangesAsync();
```

---

## Related Files

- **Models**: `tapcet-api/Models/*.cs` - Entity definitions
- **Services**: `tapcet-api/Services/Implementations/*.cs` - Business logic using DbContext
- **Controllers**: `tapcet-api/Controllers/*.cs` - API endpoints that use services
- **Migrations**: `tapcet-api/Migrations/*.cs` - Database schema versions

---

## Database Tables Created

This context creates the following tables:

### ASP.NET Identity Tables (from base class)
- `AspNetUsers` - User accounts
- `AspNetRoles` - User roles
- `AspNetUserRoles` - User-role mappings
- `AspNetUserClaims` - User claims
- `AspNetUserLogins` - External login providers
- `AspNetUserTokens` - Authentication tokens

### Quiz System Tables
- `Quizzes` - Quiz information
- `Questions` - Quiz questions
- `Choices` - Answer choices
- `QuizAttempts` - Quiz taking sessions
- `UserAnswers` - User's selected answers

### Educational Hierarchy Tables
- `Subjects` - Top-level categories
- `Courses` - Learning tracks
- `Units` - Course modules

---

## Performance Considerations

### Indexes
Entity Framework automatically creates indexes on:
- Primary keys (all `Id` columns)
- Foreign keys (all relationship columns)

### Additional Indexes (if needed)
```csharp
builder.Entity<Quiz>()
    .HasIndex(q => q.IsActive);

builder.Entity<Unit>()
    .HasIndex(u => new { u.CourseId, u.OrderIndex })
    .IsUnique(); // Ensure unique order within course
```

---
[Source: ApplicationDbContext.cs](../../Data/ApplicationDbContext.cs)

**Last Updated**: January 2025  
**Version**: 1.0  
**Status**: Production Ready
