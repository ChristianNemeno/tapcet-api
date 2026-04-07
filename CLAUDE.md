# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

All commands run from the `tapcet-api/` subdirectory (the actual .NET project).

```bash
# Build
dotnet build

# Run (development)
dotnet run

# Database migrations
dotnet ef migrations add <MigrationName>
dotnet ef database update
dotnet ef migrations remove   # undo last migration

# List migrations
dotnet ef migrations list
```

There are no test projects currently. Swagger UI is available at `/swagger` when running in development.

## Architecture

Layered ASP.NET Core 8 Web API with PostgreSQL.

```
tapcet-api/
├── Controllers/     # HTTP layer — thin, delegates to services
├── Services/        # Business logic (interface + implementation pairs)
├── Models/          # EF Core entities
├── DTO/             # Request/response shapes, organized by domain
├── Mappings/        # AutoMapper profiles
├── Data/            # ApplicationDbContext, DbSeeder
├── Extensions/      # DI registration helpers (IdentityExtensions, SwaggerExtensions, etc.)
└── Migrations/      # EF Core code-first migrations
```

**Request flow:** Controller → Service → DbContext → PostgreSQL. Controllers validate auth/authorization; services contain all business logic and use AutoMapper for DTO↔entity mapping.

## Key Technical Details

**Auth:** ASP.NET Core Identity + JWT Bearer. Two roles: `Admin` and `User`. Default admin seeded as `admin@tapcet.com / Admin@123`. Tokens expire in 60 minutes.

**Rate Limiting:** Three policy tiers configured in `appsettings.json` — Public (50/min), Authenticated (150/min), Auth endpoints (10/min).

**Database:** PostgreSQL on `127.0.0.1:6543`, database `TapcetDb`. Connection string in `appsettings.json`. DbSeeder runs at startup to ensure roles and admin user exist.

## Domain Model

Educational hierarchy: **Subject → Course → Unit → Quiz** (quiz-to-unit association is optional — quizzes can be standalone).

Core quiz flow: **Quiz** has **Questions**, each Question has 2–6 **Choices** (exactly one marked correct). Students create a **QuizAttempt**, submit **UserAnswers**, and get scored results.

## Business Rules

- Each question must have exactly 1 correct choice and 2–6 total choices
- Users can only modify their own quizzes
- Password: min 6 chars, must include uppercase, lowercase, digit
- Ownership enforced in service layer, not controller layer

## DTO / Service Conventions

- DTOs live in `DTO/<Domain>/` subdirectories
- Each service has a matching interface in `Services/Interfaces/`
- All services are registered via extension methods in `Extensions/`
- AutoMapper profiles in `Mappings/` — one profile per domain
