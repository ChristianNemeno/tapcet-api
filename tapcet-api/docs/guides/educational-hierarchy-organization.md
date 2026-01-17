# Educational Hierarchy Organization Plan

## Overview

This guide outlines the plan to transform the **Tapcet Quiz API** from a flat quiz structure into a hierarchical educational system following the standard organization pattern: **Subject ? Course ? Unit ? Quiz**.

---

## Current State

### Existing Structure
```
User
??? Quiz
    ??? Questions
    ?   ??? Choices
    ??? QuizAttempts
        ??? UserAnswers
```

**Current Features:**
- ? Users can create quizzes
- ? Users can take quizzes and get scored
- ? Quizzes have questions with multiple choices
- ? No organizational hierarchy
- ? No way to group quizzes by topic or difficulty
- ? No learning path or progression

---

## Proposed Hierarchy

### New Structure
```
Subject (e.g., "Science", "Mathematics", "Programming")
??? Course (e.g., "High School Physics", "Python for Beginners")
    ??? Unit (e.g., "Forces and Motion", "Variables and Data Types")
        ??? Quiz (e.g., "Newton's Laws Quiz", "Python Basics Test")
            ??? Questions
            ?   ??? Choices
            ??? QuizAttempts
                ??? UserAnswers
```

### Entity Relationships Diagram

```
Subject
??? Id (PK)
??? Name (string, 100)
??? Description (string?, 500)
??? Courses (Collection)

Course
??? Id (PK)
??? Title (string, 100)
??? Description (string?, 500)
??? SubjectId (FK)
??? Subject (Navigation)
??? Units (Collection)

Unit
??? Id (PK)
??? Title (string, 100)
??? OrderIndex (int)
??? CourseId (FK)
??? Course (Navigation)
??? Quizzes (Collection)

Quiz (Updated)
??? Id (PK)
??? Title (string, 200)
??? Description (string?, 2000)
??? UnitId (int?, FK) ? NEW: nullable for backward compatibility
??? OrderIndex (int) ? NEW: order within unit
??? Unit (Navigation) ? NEW
??? CreatedById (FK)
??? CreatedBy (User)
??? CreatedAt (DateTimeOffset)
??? IsActive (bool)
??? Questions (Collection)
??? QuizAttempts (Collection)
```

---

## Detailed Entity Definitions

### 1. Subject Model

**Purpose**: Top-level category for broad academic or professional domains.

**File Location**: `tapcet-api/Models/Subject.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace tapcet_api.Models
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; } // e.g., "Science"

        [MaxLength(500)]
        public string? Description { get; set; }

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
```

**Examples:**
- Science
- Mathematics  
- Programming
- History
- Language Arts
- Business
- Health & Fitness

---

### 2. Course Model

**Purpose**: Specific learning track within a subject (e.g., "High School Physics" under "Science").

**File Location**: `tapcet-api/Models/Course.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace tapcet_api.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Title { get; set; } // e.g., "High School Physics"

        [MaxLength(500)]
        public string? Description { get; set; }

        // Link to Parent (Subject)
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        public ICollection<Unit> Units { get; set; } = new List<Unit>();
    }
}
```

**Examples:**
- Subject: Science
  - High School Physics
  - College Chemistry
  - Biology 101
  - Astronomy Basics

---

### 3. Unit Model

**Purpose**: A module or chapter within a course (e.g., "Forces and Newton's Laws").

**File Location**: `tapcet-api/Models/Unit.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace tapcet_api.Models
{
    public class Unit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Title { get; set; } // e.g., "Forces and Newton's Laws"

        public int OrderIndex { get; set; } // 1, 2, 3... to maintain sequence

        // Link to Parent (Course)
        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }
}
```

**Examples:**
- Course: High School Physics
  - Unit 1: Forces and Newton's Laws (OrderIndex: 1)
  - Unit 2: Energy and Work (OrderIndex: 2)
  - Unit 3: Waves and Sound (OrderIndex: 3)
  - Unit 4: Electricity and Magnetism (OrderIndex: 4)

---

### 4. Quiz Model (Updated)

**Purpose**: Assessment within a unit (can also be standalone for backward compatibility).

**File Location**: `tapcet-api/Models/Quiz.cs`

**Changes to Apply:**

```csharp
using System.ComponentModel.DataAnnotations;

namespace tapcet_api.Models
{
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

        // --- NEW CHANGES START HERE ---
        
        // Link to Parent (Unit) - Make it nullable (?) if you want to allow "orphan" quizzes temporarily
        public int? UnitId { get; set; } 
        public Unit? Unit { get; set; }

        // To order quizzes inside the unit (Quiz 1, Quiz 2...)
        public int OrderIndex { get; set; }

        // --- NEW CHANGES END HERE ---

        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    }
}
```

**Key Points:**
- ? `UnitId` is **nullable** to allow standalone quizzes (backward compatibility)
- ? All existing quizzes will have `UnitId = null` after migration
- ? `OrderIndex` keeps quizzes in sequence within a unit (Quiz 1, Quiz 2, etc.)
- ? Quizzes can be retroactively assigned to units

---

## Database Configuration

### DbContext Updates

**File Location**: `tapcet-api/Data/ApplicationDbContext.cs`

Add these DbSet properties:

```csharp
public DbSet<Subject> Subjects { get; set; }
public DbSet<Course> Courses { get; set; }
public DbSet<Unit> Units { get; set; }
```

Add these configurations in `OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    
    // Existing configurations...
    
    // --- NEW CONFIGURATIONS ---
    
    // Subject ? Course (One-to-Many)
    builder.Entity<Course>()
        .HasOne(c => c.Subject)
        .WithMany(s => s.Courses)
        .HasForeignKey(c => c.SubjectId)
        .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Subject with Courses
    
    // Course ? Unit (One-to-Many)
    builder.Entity<Unit>()
        .HasOne(u => u.Course)
        .WithMany(c => c.Units)
        .HasForeignKey(u => u.CourseId)
        .OnDelete(DeleteBehavior.Cascade); // Delete Units when Course is deleted
    
    // Unit ? Quiz (One-to-Many, Optional)
    builder.Entity<Quiz>()
        .HasOne(q => q.Unit)
        .WithMany(u => u.Quizzes)
        .HasForeignKey(q => q.UnitId)
        .OnDelete(DeleteBehavior.SetNull); // Keep Quiz if Unit is deleted, but clear UnitId
}
```

---

## Migration Strategy

### Step 1: Create New Tables

**Run these commands:**

```bash
# Add migration
dotnet ef migrations add AddEducationalHierarchy

# Apply to database
dotnet ef database update
```

**Generated SQL (approximate):**

```sql
-- Create Subjects table
CREATE TABLE "Subjects" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500)
);

-- Create Courses table
CREATE TABLE "Courses" (
    "Id" SERIAL PRIMARY KEY,
    "Title" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500),
    "SubjectId" INT NOT NULL REFERENCES "Subjects"("Id") ON DELETE RESTRICT
);

-- Create Units table
CREATE TABLE "Units" (
    "Id" SERIAL PRIMARY KEY,
    "Title" VARCHAR(100) NOT NULL,
    "OrderIndex" INT NOT NULL,
    "CourseId" INT NOT NULL REFERENCES "Courses"("Id") ON DELETE CASCADE
);

-- Alter Quizzes table
ALTER TABLE "Quizzes"
ADD COLUMN "UnitId" INT NULL REFERENCES "Units"("Id") ON DELETE SET NULL,
ADD COLUMN "OrderIndex" INT NOT NULL DEFAULT 0;

-- Create indexes for performance
CREATE INDEX "IX_Courses_SubjectId" ON "Courses"("SubjectId");
CREATE INDEX "IX_Units_CourseId" ON "Units"("CourseId");
CREATE INDEX "IX_Quizzes_UnitId" ON "Quizzes"("UnitId");
```

### Step 2: Verify Migration

```bash
# Check current migration status
dotnet ef migrations list

# Rollback if needed (careful!)
dotnet ef database update PreviousMigrationName
```

---

## API Endpoints Design

### Subject Endpoints

**Controller**: `SubjectController.cs`

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/subject` | List all subjects | No |
| GET | `/api/subject/{id}` | Get subject with courses | No |
| POST | `/api/subject` | Create subject | Admin only |
| PUT | `/api/subject/{id}` | Update subject | Admin only |
| DELETE | `/api/subject/{id}` | Delete subject | Admin only |

**Example Request (POST):**

```json
{
  "name": "Science",
  "description": "Natural sciences including physics, chemistry, biology"
}
```

**Example Response:**

```json
{
  "id": 1,
  "name": "Science",
  "description": "Natural sciences including physics, chemistry, biology",
  "courseCount": 0
}
```

---

### Course Endpoints

**Controller**: `CourseController.cs`

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/course` | List all courses | No |
| GET | `/api/course/{id}` | Get course with units | No |
| GET | `/api/subject/{subjectId}/courses` | Courses by subject | No |
| POST | `/api/course` | Create course | Educator/Admin |
| PUT | `/api/course/{id}` | Update course | Owner/Admin |
| DELETE | `/api/course/{id}` | Delete course | Owner/Admin |

**Example Request (POST):**

```json
{
  "title": "High School Physics",
  "description": "Comprehensive introduction to physics concepts for high school students",
  "subjectId": 1
}
```

**Example Response:**

```json
{
  "id": 1,
  "title": "High School Physics",
  "description": "Comprehensive introduction to physics concepts for high school students",
  "subjectId": 1,
  "subjectName": "Science",
  "unitCount": 0
}
```

---

### Unit Endpoints

**Controller**: `UnitController.cs`

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/unit/{id}` | Get unit with quizzes | No |
| GET | `/api/course/{courseId}/units` | Units by course (ordered) | No |
| POST | `/api/unit` | Create unit | Educator/Admin |
| PUT | `/api/unit/{id}` | Update unit | Owner/Admin |
| PATCH | `/api/unit/{id}/reorder` | Update order index | Owner/Admin |
| DELETE | `/api/unit/{id}` | Delete unit | Owner/Admin |

**Example Request (POST):**

```json
{
  "title": "Forces and Newton's Laws",
  "orderIndex": 1,
  "courseId": 1
}
```

**Example Response:**

```json
{
  "id": 1,
  "title": "Forces and Newton's Laws",
  "orderIndex": 1,
  "courseId": 1,
  "courseTitle": "High School Physics",
  "quizCount": 0
}
```

---

### Quiz Endpoints (Updated)

**Existing endpoints remain functional with new optional parameters**

| Method | Endpoint | Description | Changes |
|--------|----------|-------------|---------|
| GET | `/api/quiz` | All quizzes | Add `?unitId=5` filter |
| GET | `/api/unit/{unitId}/quizzes` | Quizzes by unit (ordered) | **NEW** |
| GET | `/api/quiz/{id}` | Quiz details | Include unit info |
| POST | `/api/quiz` | Create quiz | Accept optional `unitId` |
| PUT | `/api/quiz/{id}` | Update quiz | Allow changing `unitId` |
| PATCH | `/api/quiz/{id}/assign-unit` | Assign quiz to unit | **NEW** |
| PATCH | `/api/quiz/{id}/reorder` | Update order in unit | **NEW** |

**Updated Create Quiz Request:**

```json
{
  "title": "Newton's Laws Quiz",
  "description": "Test your understanding of Newton's three laws of motion",
  "unitId": 1,
  "orderIndex": 1,
  "questions": [
    {
      "text": "What is Newton's First Law?",
      "choices": [
        { "text": "Force equals mass times acceleration", "isCorrect": false },
        { "text": "An object at rest stays at rest", "isCorrect": true }
      ]
    }
  ]
}
```

---

## DTOs (Data Transfer Objects)

### Subject DTOs

**CreateSubjectDto.cs**

```csharp
public class CreateSubjectDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string Name { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
}
```

**SubjectResponseDto.cs**

```csharp
public class SubjectResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CourseCount { get; set; } // Computed
}
```

**SubjectWithCoursesDto.cs**

```csharp
public class SubjectWithCoursesDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CourseResponseDto> Courses { get; set; } = new();
}
```

---

### Course DTOs

**CreateCourseDto.cs**

```csharp
public class CreateCourseDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public required string Title { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public int SubjectId { get; set; }
}
```

**CourseResponseDto.cs**

```csharp
public class CourseResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int UnitCount { get; set; }
}
```

**CourseWithUnitsDto.cs**

```csharp
public class CourseWithUnitsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public List<UnitResponseDto> Units { get; set; } = new();
}
```

---

### Unit DTOs

**CreateUnitDto.cs**

```csharp
public class CreateUnitDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public required string Title { get; set; }
    
    [Required]
    [Range(1, 999)]
    public int OrderIndex { get; set; }
    
    [Required]
    public int CourseId { get; set; }
}
```

**UnitResponseDto.cs**

```csharp
public class UnitResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int QuizCount { get; set; }
}
```

**UnitWithQuizzesDto.cs**

```csharp
public class UnitWithQuizzesDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public List<QuizSummaryDto> Quizzes { get; set; } = new();
}
```

---

### Updated Quiz DTOs

**CreateQuizDto.cs (Updated)**

```csharp
public class CreateQuizDto
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public required string Title { get; set; }
    
    [StringLength(2000)]
    public string? Description { get; set; }
    
    // NEW: Optional unit assignment
    public int? UnitId { get; set; }
    
    // NEW: Order within unit
    [Range(1, 999)]
    public int OrderIndex { get; set; } = 1;
    
    [Required]
    [MinLength(1)]
    public required List<CreateQuestionDto> Questions { get; set; }
}
```

**QuizSummaryDto.cs (Updated)**

```csharp
public class QuizSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // NEW fields
    public int? UnitId { get; set; }
    public string? UnitTitle { get; set; }
    public int OrderIndex { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int QuestionCount { get; set; }
    public int AttemptCount { get; set; }
}
```

---

## User Experience Flow

### Browse by Hierarchy

```
1. User lands on homepage
   ?
2. Sees list of Subjects
   - Science
   - Mathematics
   - Programming
   - History
   ?
3. Selects "Science"
   ?
4. Sees Courses in Science
   - High School Physics
   - College Chemistry
   - Biology 101
   ?
5. Selects "High School Physics"
   ?
6. Sees Units in order
   - Unit 1: Forces and Newton's Laws
   - Unit 2: Energy and Work
   - Unit 3: Waves and Sound
   ?
7. Selects "Unit 1: Forces"
   ?
8. Sees Quizzes in order
   - Quiz 1: Newton's Laws (10 questions)
   - Quiz 2: Friction and Gravity (8 questions)
   ?
9. Clicks "Start Quiz 1"
   ?
10. Takes quiz and gets results
```

### Breadcrumb Navigation Example

```
Home > Science > High School Physics > Unit 1: Forces > Quiz 1: Newton's Laws
```

---

## Business Rules

### Subject Rules
- ? Name must be unique (case-insensitive)
- ? Cannot delete if courses exist (referential integrity)
- ? Only admins can create/update/delete subjects
- ? Description is optional

### Course Rules
- ? Title must be unique within a subject
- ? Must belong to a valid subject
- ? Cannot delete if units exist
- ? Educators or admins can create courses
- ? Only owner or admin can update/delete

### Unit Rules
- ? Title should be descriptive (minimum 3 characters)
- ? OrderIndex must be positive integer (1, 2, 3...)
- ? OrderIndex should be unique within a course (enforced in service layer)
- ? Cannot delete if quizzes exist (optional: can reassign quizzes first)
- ? Reordering updates all affected OrderIndex values

### Quiz Rules
- ? Can be standalone (`UnitId = null`) or belong to a unit
- ? If part of unit, must respect `OrderIndex`
- ? Moving quiz between units updates both `OrderIndex` values
- ? Existing quizzes remain compatible (backward compatible)
- ? Only quiz owner can reassign to different unit

---

## Implementation Phases

### Phase 1: Core Hierarchy (MVP - Week 1)
**Goal:** Get the 4-level structure working

**Tasks:**
- [ ] Create `Subject.cs`, `Course.cs`, `Unit.cs` models
- [ ] Update `Quiz.cs` model with `UnitId` and `OrderIndex`
- [ ] Update `ApplicationDbContext.cs` with new DbSets and relationships
- [ ] Create migration: `dotnet ef migrations add AddEducationalHierarchy`
- [ ] Apply migration: `dotnet ef database update`
- [ ] Create DTOs for Subject, Course, Unit
- [ ] Update Quiz DTOs
- [ ] Create `ISubjectService`, `ICourseService`, `IUnitService` interfaces
- [ ] Implement services with basic CRUD operations
- [ ] Create `SubjectController`, `CourseController`, `UnitController`
- [ ] Update `QuizController` to accept optional `unitId`
- [ ] Test all endpoints via Swagger
- [ ] Add seed data (optional): 2-3 subjects, 4-6 courses, 10-15 units

**Estimated Time:** 3-4 days

**Success Criteria:**
- ? Can create Subject ? Course ? Unit ? Quiz hierarchy
- ? Can retrieve entities at each level
- ? Existing quizzes still work (backward compatible)

---

### Phase 2: Navigation & Breadcrumbs (Week 2)
**Goal:** Users can browse by hierarchy

**Tasks:**
- [ ] Create subject listing page
- [ ] Create course listing (filtered by subject)
- [ ] Create unit listing (filtered by course, ordered)
- [ ] Create quiz listing (filtered by unit, ordered)
- [ ] Implement breadcrumb navigation component
- [ ] Add "Back" navigation at each level
- [ ] Show hierarchy context on quiz detail page
- [ ] Add filtering to existing quiz endpoints (`?unitId=5`)

**Estimated Time:** 3-4 days

**Success Criteria:**
- ? Users can drill down: Subject ? Course ? Unit ? Quiz
- ? Breadcrumbs show full path
- ? Can navigate back up the hierarchy

---

### Phase 3: Content Management (Week 3)
**Goal:** Educators can organize content easily

**Tasks:**
- [ ] Implement drag-and-drop reordering for Units
- [ ] Implement drag-and-drop reordering for Quizzes within Unit
- [ ] Add "Move to Unit" feature for quizzes
- [ ] Add "Copy Quiz to Another Unit" feature
- [ ] Bulk operations: assign multiple quizzes to unit
- [ ] Auto-adjust OrderIndex when inserting/removing
- [ ] Show unit/quiz count at each level

**Estimated Time:** 4-5 days

**Success Criteria:**
- ? Can reorder units within course
- ? Can reorder quizzes within unit
- ? Can move/copy quizzes between units

---

### Phase 4: Progress Tracking (Week 4-5)
**Goal:** Users see their learning journey

**Tasks:**
- [ ] Create `UserProgress` model (tracks completion per quiz)
- [ ] Add migration for progress tracking
- [ ] Calculate quiz completion status
- [ ] Calculate unit completion percentage
- [ ] Calculate course completion percentage
- [ ] Show overall subject progress
- [ ] Add "Continue Learning" feature (resume from last unit)
- [ ] Display "Next Quiz" button after completion
- [ ] Show progress bars on course/unit pages
- [ ] Add "Completed" badge on finished quizzes

**Estimated Time:** 5-6 days

**Success Criteria:**
- ? Users see completion status for each level
- ? Progress bars reflect actual completion
- ? Can resume learning from last position

---

### Phase 5: Advanced Features (Future)

**Potential Enhancements:**
- ?? **Prerequisites**: Require completing Unit 1 before unlocking Unit 2
- ?? **Certifications**: Issue certificate PDF for course completion
- ?? **Tags & Search**: Add tags to quizzes, search across hierarchy
- ?? **Recommendations**: "You might like this course..." based on completion
- ?? **Learning Paths**: Curated sequence across multiple courses
- ?? **Difficulty Levels**: Beginner, Intermediate, Advanced per unit
- ?? **Time Estimates**: "This unit takes ~2 hours"
- ?? **Ratings & Reviews**: Users rate courses/units
- ?? **Discussion Forums**: Q&A per unit
- ?? **Instructor Profiles**: Show who created the course

---

## Backward Compatibility Strategy

### Handling Existing Quizzes

**After Migration:**
- All existing quizzes will have `UnitId = NULL`
- All existing quizzes will have `OrderIndex = 0`
- They remain accessible via `/api/quiz` (flat list)

**Options for Existing Quizzes:**

1. **Leave as Standalone**
   - Continue to show in flat quiz list
   - Don't require unit assignment

2. **Manual Assignment**
   - Admin dashboard to assign quizzes to units
   - Endpoint: `PATCH /api/quiz/{id}/assign-unit`

3. **Bulk Import/Migration**
   - Create default subject/course/unit structure
   - Assign all orphan quizzes to "General" unit

**Recommendation:** Keep option 1 (standalone quizzes allowed) for MVP, add option 2 later.

---

## Example Content Structure

### Sample Hierarchy

```
?? Subject: Programming
    ?? Course: Python for Beginners
        ?? Unit 1: Getting Started (OrderIndex: 1)
            ?? Quiz 1: Variables and Data Types (OrderIndex: 1)
            ?? Quiz 2: Basic Operations (OrderIndex: 2)
        ?? Unit 2: Control Flow (OrderIndex: 2)
            ?? Quiz 1: If Statements (OrderIndex: 1)
            ?? Quiz 2: Loops (OrderIndex: 2)
            ?? Quiz 3: Functions (OrderIndex: 3)
        ?? Unit 3: Data Structures (OrderIndex: 3)
            ?? Quiz 1: Lists and Tuples (OrderIndex: 1)
            ?? Quiz 2: Dictionaries (OrderIndex: 2)
    
    ?? Course: JavaScript Essentials
        ?? Unit 1: Basics
            ?? Quiz 1: Variables and Types
            ?? Quiz 2: Functions
        ?? Unit 2: DOM Manipulation
            ?? Quiz 1: Selecting Elements
            ?? Quiz 2: Event Handling

?? Subject: Science
    ?? Course: High School Physics
        ?? Unit 1: Forces and Motion (OrderIndex: 1)
            ?? Quiz 1: Newton's Laws (OrderIndex: 1)
            ?? Quiz 2: Friction and Gravity (OrderIndex: 2)
        ?? Unit 2: Energy (OrderIndex: 2)
            ?? Quiz 1: Kinetic and Potential Energy
            ?? Quiz 2: Conservation of Energy
        ?? Unit 3: Waves (OrderIndex: 3)
            ?? Quiz 1: Wave Properties
            ?? Quiz 2: Sound Waves

?? Subject: Mathematics
    ?? Course: Algebra 1
        ?? Unit 1: Linear Equations
        ?? Unit 2: Quadratic Equations
        ?? Unit 3: Polynomials
```

---

## Seed Data Script

### Example Data for Testing

**File Location**: `tapcet-api/Data/SeedEducationalHierarchy.cs`

```csharp
public static class EducationalHierarchySeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (context.Subjects.Any())
            return; // Already seeded

        // Create Subjects
        var science = new Subject 
        { 
            Name = "Science", 
            Description = "Natural sciences including physics, chemistry, biology" 
        };
        
        var programming = new Subject 
        { 
            Name = "Programming", 
            Description = "Computer programming and software development" 
        };
        
        context.Subjects.AddRange(science, programming);
        await context.SaveChangesAsync();

        // Create Courses
        var physics = new Course 
        { 
            Title = "High School Physics", 
            Description = "Introduction to physics concepts",
            SubjectId = science.Id 
        };
        
        var python = new Course 
        { 
            Title = "Python for Beginners", 
            Description = "Learn Python programming from scratch",
            SubjectId = programming.Id 
        };
        
        context.Courses.AddRange(physics, python);
        await context.SaveChangesAsync();

        // Create Units for Physics
        var forcesUnit = new Unit 
        { 
            Title = "Forces and Newton's Laws", 
            OrderIndex = 1,
            CourseId = physics.Id 
        };
        
        var energyUnit = new Unit 
        { 
            Title = "Energy and Work", 
            OrderIndex = 2,
            CourseId = physics.Id 
        };
        
        // Create Units for Python
        var pythonBasics = new Unit 
        { 
            Title = "Getting Started with Python", 
            OrderIndex = 1,
            CourseId = python.Id 
        };
        
        context.Units.AddRange(forcesUnit, energyUnit, pythonBasics);
        await context.SaveChangesAsync();

        Console.WriteLine("Educational hierarchy seeded successfully!");
    }
}
```

**Call from `Program.cs`:**

```csharp
// After app.Run()
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await EducationalHierarchySeeder.SeedAsync(context);
}
```

---

## Testing Strategy

### Unit Tests

**Test File**: `SubjectServiceTests.cs`

```csharp
[Fact]
public async Task CreateSubject_ValidData_ReturnsSubject()
{
    // Arrange
    var dto = new CreateSubjectDto { Name = "Science" };
    
    // Act
    var result = await _subjectService.CreateAsync(dto);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("Science", result.Name);
}

[Fact]
public async Task GetCoursesBySubject_ReturnsOrderedCourses()
{
    // Arrange
    var subjectId = 1;
    
    // Act
    var result = await _courseService.GetBySubjectAsync(subjectId);
    
    // Assert
    Assert.NotEmpty(result);
    Assert.True(result.All(c => c.SubjectId == subjectId));
}
```

### Integration Tests

**Test File**: `HierarchyIntegrationTests.cs`

```csharp
[Fact]
public async Task CreateFullHierarchy_Success()
{
    // Create Subject
    var subjectResponse = await _client.PostAsJsonAsync("/api/subject", 
        new { name = "Test Subject" });
    var subject = await subjectResponse.Content.ReadFromJsonAsync<SubjectResponseDto>();
    
    // Create Course
    var courseResponse = await _client.PostAsJsonAsync("/api/course", 
        new { title = "Test Course", subjectId = subject.Id });
    var course = await courseResponse.Content.ReadFromJsonAsync<CourseResponseDto>();
    
    // Create Unit
    var unitResponse = await _client.PostAsJsonAsync("/api/unit", 
        new { title = "Test Unit", orderIndex = 1, courseId = course.Id });
    var unit = await unitResponse.Content.ReadFromJsonAsync<UnitResponseDto>();
    
    // Create Quiz
    var quizResponse = await _client.PostAsJsonAsync("/api/quiz", 
        new { title = "Test Quiz", unitId = unit.Id, questions = [...] });
    
    Assert.True(quizResponse.IsSuccessStatusCode);
}
```

---

## Performance Considerations

### Database Indexes

```sql
-- Add indexes for foreign keys (auto-created by EF Core)
CREATE INDEX "IX_Courses_SubjectId" ON "Courses"("SubjectId");
CREATE INDEX "IX_Units_CourseId" ON "Units"("CourseId");
CREATE INDEX "IX_Quizzes_UnitId" ON "Quizzes"("UnitId");

-- Add composite indexes for common queries
CREATE INDEX "IX_Units_CourseId_OrderIndex" ON "Units"("CourseId", "OrderIndex");
CREATE INDEX "IX_Quizzes_UnitId_OrderIndex" ON "Quizzes"("UnitId", "OrderIndex");
```

### Caching Strategy

**Recommended Caching:**
- ? Cache subject list (changes rarely)
- ? Cache course list per subject
- ? Cache unit list per course
- ?? Don't cache quiz lists (frequently updated)

**Implementation:**

```csharp
// In SubjectService
private readonly IMemoryCache _cache;

public async Task<List<SubjectResponseDto>> GetAllAsync()
{
    if (!_cache.TryGetValue("all_subjects", out List<SubjectResponseDto> subjects))
    {
        subjects = await _context.Subjects
            .Include(s => s.Courses)
            .ToListAsync();
        
        _cache.Set("all_subjects", subjects, TimeSpan.FromHours(1));
    }
    
    return subjects;
}
```

### Eager Loading

**Avoid N+1 queries:**

```csharp
// ? Bad: N+1 query problem
var courses = await _context.Courses.ToListAsync();
foreach (var course in courses)
{
    var units = course.Units; // Triggers separate query per course
}

// ? Good: Single query with Include
var courses = await _context.Courses
    .Include(c => c.Units)
    .ToListAsync();
```

---

## Security Considerations

### Authorization Rules

```csharp
// Subject Management: Admin only
[Authorize(Roles = "Admin")]
public class SubjectController : ControllerBase { }

// Course Management: Educator or Admin
[Authorize(Roles = "Educator,Admin")]
public async Task<IActionResult> CreateCourse() { }

// Quiz Management: Owner or Admin
private async Task<bool> IsAuthorizedAsync(int quizId, string userId)
{
    var quiz = await _context.Quizzes.FindAsync(quizId);
    return quiz.CreatedById == userId || User.IsInRole("Admin");
}
```

### Input Validation

```csharp
// Prevent duplicate subjects
public async Task<bool> SubjectExistsAsync(string name)
{
    return await _context.Subjects
        .AnyAsync(s => s.Name.ToLower() == name.ToLower());
}

// Validate OrderIndex uniqueness within course
public async Task<bool> ValidateOrderIndexAsync(int courseId, int orderIndex, int? excludeUnitId = null)
{
    return !await _context.Units
        .AnyAsync(u => u.CourseId == courseId 
            && u.OrderIndex == orderIndex 
            && u.Id != excludeUnitId);
}
```

---

## Questions to Consider

### Before Implementation

1. **Permissions Model**
   - Who can create subjects? (Admin only, or open to educators?)
   - Who can create courses? (Educators, or require approval?)
   - Can users create units/quizzes in any course, or only their own?

2. **Standalone Quizzes**
   - Should all new quizzes require a unit assignment?
   - Or allow "General Quizzes" section for standalone?

3. **Quiz Reassignment**
   - Can a quiz move between units? (Recommendation: Yes)
   - Should moving a quiz clear attempt history? (Recommendation: No)

4. **Ordering Conflicts**
   - What happens if two units have same OrderIndex? (Recommendation: Auto-increment others)
   - Should OrderIndex be editable, or only via drag-and-drop?

5. **Deletion Behavior**
   - If Subject is deleted, what happens to courses? (Recommendation: Prevent deletion)
   - If Course is deleted, cascade to units and quizzes? (Recommendation: Yes, with warning)
   - If Unit is deleted, orphan quizzes or delete them? (Recommendation: Set UnitId to NULL)

6. **Multiple Attempts**
   - Current system allows unlimited attempts—keep this? (Recommendation: Yes for MVP)
   - Should progress track "best attempt" or "latest attempt"?

---

## Next Steps

### Immediate Actions (Phase 1)

1. ? **Review this plan** with stakeholders/team
2. ? **Decide on MVP scope** (Recommend: Phase 1 only for first release)
3. ? **Create model files**: `Subject.cs`, `Course.cs`, `Unit.cs`
4. ? **Update `Quiz.cs`** with new properties
5. ? **Update `ApplicationDbContext.cs`** with DbSets and relationships
6. ? **Create migration**: `dotnet ef migrations add AddEducationalHierarchy`
7. ? **Apply migration**: `dotnet ef database update`
8. ? **Verify migration** via database inspection
9. ? **Create DTOs** for new entities
10. ? **Implement services** (Subject, Course, Unit)
11. ? **Create controllers** with basic endpoints
12. ? **Update quiz controller** to support `unitId`
13. ? **Test via Swagger** (create full hierarchy)
14. ? **Optional: Add seed data** for demo
15. ? **Deploy to test environment**

### Documentation to Update

- ? API Reference (`docs/api-reference.md`)
- ? Architecture Documentation (`docs/architecture.md`)
- ? Testing Guide (`docs/testing-guide.md`)
- ? Business Rules (`docs/business-rules.md`)

---

## Summary

This hierarchical organization transforms **Tapcet** from a flat quiz app into a **structured learning platform** similar to Udemy, Coursera, or Khan Academy.

### Key Benefits

? **Organized Content**: Browse by domain ? track ? module ? assessment  
? **Scalability**: Supports hundreds of courses and thousands of quizzes  
? **User Experience**: Clear learning paths with logical progression  
? **Backward Compatible**: Existing quizzes continue to work  
? **Flexible**: Quizzes can be standalone or organized  
? **Extensible**: Easy to add features like prerequisites, certifications, progress tracking  

### Success Metrics

After Phase 1 implementation, you should be able to:

1. ? Create: Subject ? Course ? Unit ? Quiz
2. ? List: Courses by Subject, Units by Course, Quizzes by Unit
3. ? Navigate: Full breadcrumb path from Subject to Quiz
4. ? Maintain: Existing flat quizzes still work
5. ? Reorder: Change unit/quiz order via OrderIndex

---

## Need Help?

If you need assistance with:
- ? Creating the models and migrations
- ? Implementing services and controllers
- ? Writing DTOs and AutoMapper profiles
- ? Setting up seed data
- ? Testing the hierarchy
- ? Frontend integration

Let me know which phase/component you'd like to start with! ??

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**Status**: ? Ready for Implementation
