# Project Status Journal

Date: 2025-11-09

## Overview
Tapcet Quiz API currently runs on .NET 8 (C# 12). Core authentication (registration, login, JWT issuance, role seeding) is functional. Database layer migrated from SQL Server to PostgreSQL running in Docker.

## Runtime & Hosting
- Framework: .NET 8
- API Ports (Development):
  - HTTP: http://localhost:5080
  - HTTPS: https://localhost:7237
- Launch profile updated to avoid port conflict (previously 5052 occupied).

## Database
- Engine: PostgreSQL 17 (Docker container)
- Container Name: tapcet-pg
- Host: 127.0.0.1
- Mapped Port: 6543 (host) -> 5432 (container)
- Database Name: TapcetDb
- User: tapcet
- Connection String (excluding password): `Host=127.0.0.1;Port=6543;Database=TapcetDb;Username=tapcet;Password=***`
- Migration: `InitialCreate` applied successfully (Identity tables present)
- Tooling: Access verified via `psql` and can be browsed with pgAdmin configuration (Host 127.0.0.1, Port 6543)

## Authentication & Authorization
- Identity: ASP.NET Core Identity integrated with custom `User` model (extra fields: CreatedDate, TotalQuizAttempts, AverageScore)
- Roles auto-seeded: `Admin`, `User`
- Default admin user created:
  - Email: `admin@tapcet.com`
  - Password (not stored in journal) set during seeding
- JWT Configuration:
  - Issuer: TapcetAPI
  - Audience: TapcetClient
  - Expiry: 60 minutes
  - Secret Key sourced from `appsettings.json` (should be moved to secrets / env for production)
- Claims embedded: `sub (NameIdentifier)`, `name`, `email`, `jti`, roles.

## Security Notes
- Secret key currently hard-coded in `appsettings.json` (development only). Needs migration to User Secrets / environment variables for production.
- No refresh token implementation yet.
- HTTPS enabled in development profile.

## Recent Issues Resolved
| Issue | Resolution |
|-------|------------|
| SQL Server build errors after package removal | Removed old migrations and switched to Npgsql provider |
| Password auth failures (PostgreSQL) | Port conflict & mismatched password resolved; new container with distinct port 6543 |
| Local PostgreSQL service intercepting connections | Moved Docker to high port (6543) to avoid OS service listeners |
| Binding failure on port 5052 | Changed launchSettings to use 5080/7237 |
| JWT / role seeding validation | Confirmed roles create and admin user provisioned on startup |

## Pending / Next Tasks
- Implement quiz domain entities (Quiz, Question, Choice, Attempt, etc.)
- Add DTO validation around scoring and attempt tracking.
- Introduce refresh tokens & revoke logic (optional enhancement).
- Move JWT secret and DB credentials to environment variables / secrets.
- Add unit/integration tests (AuthService, Controllers, role seeding).
- Implement global exception handling middleware.
- Add rate limiting on auth endpoints.
- Prepare Dockerfile + compose for production (API + Postgres) with health checks.

## Technical Debt / Improvements
- Logging: Expand structured logging (correlation IDs) for auth flows.
- Metrics: Add basic health/metrics endpoint (e.g. `/health` with ASP.NET Core HealthChecks).
- Validation: Ensure consistent use of `ProblemDetails` responses for error states.
- Security: Enforce password complexity rules in configuration and consider email confirmation.

## Current File Highlights
- `Program.cs`: Configures DI, EF Core (Npgsql), Identity, JWT, Swagger, and role/user seeding.
- `ApplicationDbContextFactory`: Provides design-time context for migrations via Npgsql.
- `launchSettings.json`: Updated ports to avoid conflicts.

## Verification Checklist (Completed)
- [x] PostgreSQL container reachable on port 6543
- [x] EF Core migration applied
- [x] Identity tables present
- [x] Registration endpoint functional
- [x] Login endpoint functional
- [x] JWT returned and decodable with expected claims
- [x] Admin user seeded
- [x] Roles assigned & retrievable

## Recommended Immediate Actions
1. Move secrets (DB password, JWT key) to User Secrets or environment variables.
2. Add migration scripts or documentation for production deployment.
3. Begin implementing quiz domain model classes & related migrations.
4. Add basic integration tests before expanding features.

---
*End of status snapshot.*