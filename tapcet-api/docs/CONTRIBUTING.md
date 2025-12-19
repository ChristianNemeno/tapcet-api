# Contributing

## Overview

This document describes how to contribute to the TAPCET Quiz API project.

## Requirements

- .NET 8 SDK
- PostgreSQL 17 (or Docker)
- A code editor (Visual Studio 2022 recommended)

## Branching Strategy

- `master`: Stable branch
- Feature branches: `feature/<short-description>`
- Bugfix branches: `fix/<short-description>`

## Development Workflow

1. Create a feature branch from `master`
2. Make changes in small, focused commits
3. Ensure the project builds and tests pass
4. Update documentation if behavior changes
5. Open a pull request

## Coding Standards

- Use constructor injection for dependencies
- Prefer interfaces (`IQuizService`) over concrete classes in controllers
- Keep controllers thin; business logic belongs in the service layer
- Use async/await for all EF Core operations
- Use structured logging (message templates with parameters)

## Documentation

If you change behavior or add endpoints, update:
- `docs/api-reference.md`
- `docs/business-rules.md`
- `docs/dtos.md` (if DTOs change)

## Testing

Before submitting a pull request:

```bash
dotnet restore
dotnet build
dotnet test
```

If no tests exist yet, at minimum ensure `dotnet build` succeeds and manually verify relevant endpoints in Swagger.

## Commit Messages

Use clear commit messages:

- `feat: add leaderboard endpoint`
- `fix: validate answer count before scoring`
- `docs: update api reference for quiz-attempt submit`

## Security

Do not commit:
- JWT secret keys
- Database passwords
- Production connection strings
- Any personal data

Use environment variables or User Secrets for local development.
