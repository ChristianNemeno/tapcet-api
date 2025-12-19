# FAQ

## Where is the entry point for the API?

`Program.cs`.

It configures:
- Database (PostgreSQL + EF Core)
- Identity
- JWT authentication
- Swagger
- Dependency injection
- Middleware pipeline

## Where do I find the API endpoints?

See `api-reference.md`. The controllers are in `Controllers/`.

## Which endpoints require authentication?

- Most endpoints require JWT authentication.
- Some quiz listing endpoints allow anonymous access.

See `api-reference.md` and `authentication.md`.

## How do I run the database locally?

Use Docker (recommended) as described in `getting-started.md` and `onboarding.md`.

## How do I apply migrations?

From `tapcet-api/`:

```bash
dotnet ef database update
```

## Why can I not start an attempt for a quiz?

The quiz attempt start requires:
- The quiz exists
- The quiz is active
- The quiz has at least one question

See `business-rules.md`.

## Why can I not submit answers?

Submission requires:
- You own the attempt
- The attempt has not been completed
- Answer count matches the quiz question count
- Each answer references a valid question and choice

See `business-rules.md`.

## What is the ranking logic for leaderboards?

Leaderboards are ordered by:
1. Score (descending)
2. Duration (ascending)

See `business-rules.md`.
