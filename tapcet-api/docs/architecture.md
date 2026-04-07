# Architecture

## Layer Overview

```
HTTP Request
     │
     ▼
┌─────────────────────────────────────────┐
│  Controllers/                           │  ← HTTP only: routing, auth attributes,
│  (AuthController, QuizController, ...)  │    model validation, status codes
└────────────────────┬────────────────────┘
                     │ calls
                     ▼
┌─────────────────────────────────────────┐
│  Services/                              │  ← All business logic, ownership checks,
│  (IAuthService, IQuizService, ...)      │    AutoMapper, EF Core queries
└────────────────────┬────────────────────┘
                     │ uses
                     ▼
┌─────────────────────────────────────────┐
│  Data/ApplicationDbContext              │  ← EF Core DbContext, entity config,
│  + Models/                             │    relationships, cascade rules
└────────────────────┬────────────────────┘
                     │
                     ▼
                 PostgreSQL
```

**Rule:** Business logic never lives in controllers. Controllers read the request, call one service method, and return a status code. Entities never leave the service layer — only DTOs cross the controller/service boundary.

---

## Directory Structure

```
tapcet-api/
├── Controllers/          # 6 API controllers (one per domain)
├── Services/
│   ├── Interfaces/       # IAuthService, IQuizService, IQuizAttemptService, ...
│   └── *.cs              # Concrete implementations
├── Models/               # 9 EF Core entity classes
├── DTO/
│   ├── Auth/
│   ├── Quiz/
│   ├── Question/
│   ├── Choice/
│   ├── Attempt/
│   ├── Subject/
│   ├── Course/
│   └── Unit/
├── Mappings/             # AutoMapper profiles (one per domain)
├── Data/
│   ├── ApplicationDbContext.cs   # DbContext + OnModelCreating config
│   └── DbSeeder.cs               # Seeds roles and admin user on startup
├── Extensions/           # DI registration helpers
│   ├── IdentityExtensions.cs
│   ├── SwaggerExtensions.cs
│   └── RateLimitingExtensions.cs
├── Migrations/           # EF Core code-first migration files
├── Program.cs            # App entry point, all DI and middleware wiring
└── appsettings.json      # Connection string, JWT, rate limiting config
```

---

## Startup Sequence (Program.cs)

1. Register infrastructure — controllers, DbContext (`ApplicationDbContext`)
2. Register extension modules — Identity, Swagger, rate limiting, AutoMapper
3. Register services — `IAuthService`, `IQuizService`, `IQuizAttemptService`, `ISubjectService`, `ICourseService`, `IUnitService`
4. Build middleware pipeline — HTTPS redirect → authentication → authorization → rate limiting → Swagger → controllers
5. Seed database — `DbSeeder.SeedAsync()` runs on every startup (idempotent; skips if data already exists)

---

## Service Pattern

Every service follows the same shape:

```csharp
// Services/Interfaces/IQuizService.cs
public interface IQuizService
{
    Task<QuizResponseDto?> CreateQuizAsync(CreateQuizDto dto, string userId);
    // ...
}

// Services/QuizService.cs
public class QuizService : IQuizService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<QuizService> _logger;
    // ...
}
```

**Error signaling convention:**
- Methods that find/modify resources return `null` when the resource doesn't exist or the caller lacks permission.
- Methods that perform delete/block operations return `bool` — `false` means "not found or blocked by a business rule."
- Controllers map `null` → `404` or `403`, `false` → `400` or `404`.

---

## AutoMapper

One profile per domain in `Mappings/`. Registered via:
```csharp
builder.Services.AddAutoMapper(typeof(Program).Assembly);
```

---

## Known Gaps (Not Yet Implemented)

- **Global exception handler** — each controller catches exceptions individually
- **CORS policy** — not configured; browser-based frontends will be blocked
- **Pagination** — all list endpoints return full result sets
- **Token refresh** — no refresh token endpoint; clients must re-login after 60 min
