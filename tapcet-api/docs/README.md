# TAPCET API — Documentation

> ASP.NET Core 8 REST API for a quiz-based learning platform.

---

## Table of Contents

| # | Document | What it covers |
|---|----------|----------------|
| 1 | [Getting Started](getting-started.md) | Prerequisites, database setup, first run |
| 2 | [Architecture](architecture.md) | Project structure, layers, request flow |
| 3 | [Authentication](authentication.md) | JWT, roles, registration, login |
| 4 | [API Reference](api-reference.md) | Every endpoint — method, path, request body, response |
| 5 | [Database Schema](database-schema.md) | All tables, columns, constraints, cascade rules |
| 6 | [Business Rules](business-rules.md) | Validation rules, ownership, scoring, deletion constraints |
| 7 | [Security](security.md) | Rate limiting, known gaps, production checklist |
| 8 | [Deployment](deployment.md) | Environment variables, Docker, production config |

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 8 |
| Database | PostgreSQL |
| ORM | Entity Framework Core 8 (code-first) |
| Auth | ASP.NET Core Identity + JWT Bearer |
| Object Mapping | AutoMapper 12 |
| API Docs | Swagger / OpenAPI (Swashbuckle) |

---

## Domain in One Sentence

Students take quizzes. Quizzes live inside an educational hierarchy: **Subject → Course → Unit → Quiz**. Each quiz has multiple-choice questions with exactly one correct answer. After submission, students get a score and can compare on a leaderboard.

---

## Default Dev Credentials

Seeded automatically on first run by `Data/DbSeeder.cs`.

| | Value |
|-|-------|
| Email | `admin@tapcet.com` |
| Password | `Admin@123` |
| Role | `Admin` |
