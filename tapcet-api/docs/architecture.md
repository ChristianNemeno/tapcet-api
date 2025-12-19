# Architecture

## Overview

TAPCET Quiz API follows a layered architecture pattern with clear separation of concerns. The application is structured into distinct layers, each with specific responsibilities.

## Architecture Layers

```
+-----------------------------------------+
|           Presentation Layer            |
|     (Controllers / HTTP Endpoints)      |
+---------------------+-------------------+
                      |
+---------------------v-------------------+
|              Service Layer              |
|    (Business Logic & Orchestration)     |
+---------------------+-------------------+
                      |
+---------------------v-------------------+
|           Data Access Layer             |
| (Entity Framework Core / DbContext)     |
+---------------------+-------------------+
                      |
+---------------------v-------------------+
|              Database Layer             |
|               (PostgreSQL)              |
+-----------------------------------------+
```

## Layer Responsibilities

### 1. Presentation Layer (Controllers)

**Location**: `Controllers/`

**Responsibilities**:
- Handle HTTP requests and responses
- Route requests to appropriate service methods
- Validate request data using ModelState
- Extract user identity from JWT tokens
- Return appropriate HTTP status codes
- Log controller-level actions

**Key Components**:
- `AuthController`: User registration and login
- `QuizController`: Quiz CRUD operations and question management
- `QuizAttemptController`: Quiz attempt lifecycle and results

**Pattern**:
```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ExampleController : ControllerBase
{
    private readonly IExampleService _service;
    private readonly ILogger<ExampleController> _logger;

    // Constructor injection
    // Validation
    // Service calls
    // Response mapping
}
```

### 2. Service Layer

**Location**: `Services/`

**Responsibilities**:
- Implement business logic and rules
- Orchestrate data access operations
- Perform data validation
- Map entities to DTOs using AutoMapper
- Handle exceptions and logging

**Key Components**:
- `AuthService`: Authentication and JWT generation
- `QuizService`: Quiz management and question validation
- `QuizAttemptService`: Attempt tracking, scoring, and statistics

**Pattern**:
```csharp
public class ExampleService : IExampleService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ExampleService> _logger;

    // Business logic implementation
    // Database queries with EF Core
    // Data transformation
    // Error handling
}
```

### 3. Data Access Layer

**Location**: `Data/`, `Models/`

**Responsibilities**:
- Define database schema through entity models
- Configure entity relationships
- Manage database context
- Execute database migrations

**Key Components**:
- `ApplicationDbContext`: EF Core database context
- Entity models: `User`, `Quiz`, `Question`, `Choice`, `QuizAttempt`, `UserAnswer`

**Pattern**:
```csharp
public class ApplicationDbContext : IdentityDbContext<User>
{
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
}
```

### 4. Data Transfer Objects (DTOs)

**Location**: `DTO/`

**Responsibilities**:
- Define API request and response contracts
- Implement data validation attributes
- Prevent over-posting and under-posting
- Decouple internal models from external API

## Design Patterns

### Dependency Injection

All services and dependencies are registered in `Program.cs` and injected through constructors.

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IQuizAttemptService, QuizAttemptService>();
```

## Authentication and Authorization

### JWT Token Flow

```
Client -> AuthController -> Identity/UserManager -> JWT token
Client -> API endpoint (Authorization: Bearer <token>) -> Controller/Service
```

### Authorization Levels

1. Public endpoints
   - `GET /api/quiz`
   - `GET /api/quiz/active`

2. Authenticated endpoints
   - All POST/PUT/PATCH/DELETE operations
   - User-specific attempt endpoints

3. Owner authorization
   - Update/delete quizzes only by creator
   - View attempt results only by attempt owner

## Data Flow

### Quiz Creation Flow

```
Client (CreateQuizDto)
  -> QuizController.CreateQuiz
  -> QuizService.CreateQuizAsync
  -> EF Core SaveChanges
  -> return 201 Created (QuizResponseDto)
```

### Quiz Attempt Flow

```
Client starts attempt
  -> validate quiz active + has questions
  -> create QuizAttempt
Client submits answers
  -> validate attempt ownership + completeness
  -> calculate score
  -> persist UserAnswers
  -> update user statistics
  -> return QuizResultDto
```

## Error Handling

- Service layer logs exceptions and returns null/empty results on recoverable failures.
- Controller layer translates null/failed results into HTTP status codes (400/404/401).

## Configuration

Configuration is stored in `appsettings.json` / `appsettings.Development.json` and may be overridden by environment variables.

Relevant sections:
- `ConnectionStrings:DefaultConnection`
- `JwtSettings:*`

## Notes for MVP

For an MVP, this structure is sufficient. For production hardening, consider:
- Global exception handling middleware
- Rate limiting
- Pagination for list endpoints
- Secret management via environment variables or a secret vault
