# TAPCET Quiz API Documentation

## Purpose

This directory contains the formal documentation for the TAPCET Quiz API. It is intended to onboard new developers, document API behavior for consumers, and describe system design decisions.

## Start Here

- **New to the project**: read `onboarding.md`.
- **Need endpoints and payloads**: read `api-reference.md`.
- **Frontend developer**: read `frontend-guide.md`.
- **Need business constraints**: read `business-rules.md`.
- **Understanding the hierarchy**: read `guides/educational-hierarchy-organization.md`.

## Documentation Index

### Quick Start Guides

- `onboarding.md` - Developer onboarding (run locally, project tour, common tasks)
- `getting-started.md` - Setup instructions and prerequisites
- `frontend-guide.md` - **Frontend development guide with TypeScript examples** ?
- `api-reference.md` - **Complete API reference with all endpoints** ??

### System Design

- `architecture.md` - Architecture overview and design patterns
- `database-schema.md` - Database schema and entity relationships
- `service-layer.md` - Service-layer responsibilities and methods
- `dtos.md` - DTO definitions and validation
- `authentication.md` - Authentication and authorization
- `business-rules.md` - Business rules and validation

### Educational Hierarchy System

- `guides/educational-hierarchy-organization.md` - **Complete hierarchy implementation plan** ??
  - Subject ? Course ? Unit ? Quiz structure
  - Implementation phases and status
  - API endpoints for each level
  - Frontend integration examples

### Development & Testing

- `testing-guide.md` - Manual and automated testing guide
- `CONTRIBUTING.md` - Contribution guidelines
- `faq.md` - Common questions and troubleshooting

### Deployment & Maintenance

- `deployment.md` - Deployment strategies and environment configuration
- `CHANGELOG.md` - Release notes
- `SECURITY.md` - Security policy and production hardening recommendations

---

## Quick Links for Frontend Development

### ?? Get Started Quickly

1. **Authentication Flow**: `api-reference.md#authentication-api`
2. **Browse Subjects**: `GET /api/subject` - `api-reference.md#get-all-subjects`
3. **Course Listing**: `GET /api/course/subject/{id}` - `api-reference.md#get-courses-by-subject`
4. **Unit Listing**: `GET /api/unit/course/{id}` - `api-reference.md#get-units-by-course`
5. **Quiz Listing**: `GET /api/unit/{unitId}/quizzes` - `api-reference.md#get-quizzes-by-unit`
6. **Take Quiz**: `api-reference.md#quiz-attempt-api`

### ?? TypeScript Integration

See `frontend-guide.md` for:
- Complete TypeScript types
- API client setup
- React hooks and components
- State management examples
- Error handling patterns
- UI component examples (Breadcrumb, Quiz Taker, etc.)

### ?? Hierarchy Navigation

```
Home
  ?? Subjects (GET /api/subject)
      ?? Courses (GET /api/course/subject/{subjectId})
          ?? Units (GET /api/unit/course/{courseId})
              ?? Quizzes (GET /api/unit/{unitId}/quizzes)
                  ?? Quiz Detail (GET /api/quiz/{id})
                      ?? Take Quiz (POST /api/quiz-attempt/start)
```

### ?? Data Flow Examples

**Browse by Hierarchy**:
1. Load all subjects ? Display grid
2. Click subject ? Load courses in subject
3. Click course ? Load units in course (ordered)
4. Click unit ? Load quizzes in unit (ordered)
5. Click quiz ? Start quiz attempt

**Quiz Taking**:
1. Start attempt: `POST /api/quiz-attempt/start`
2. Display questions and collect answers
3. Submit answers: `POST /api/quiz-attempt/submit`
4. Show results with score and explanations

**Breadcrumb Navigation**:
```typescript
// Example: Science > High School Physics > Unit 1: Forces > Newton's Laws Quiz
const breadcrumbs = await getBreadcrumbs(quizId);
// Returns: [Home, Science, High School Physics, Forces and Newton's Laws, Newton's Laws Quiz]
```

---

## API Endpoints Summary

### Educational Hierarchy

| Entity | Endpoints | Purpose |
|--------|-----------|---------|
| **Subject** | 5 endpoints | Top-level categories (Science, Math, Programming) |
| **Course** | 6 endpoints | Learning tracks within subjects |
| **Unit** | 7 endpoints | Modules/chapters within courses (ordered) |
| **Quiz** | 13 endpoints | Assessments within units (ordered) |

### Quiz System

| Feature | Endpoints | Purpose |
|---------|-----------|---------|
| **Authentication** | 2 endpoints | Register, Login |
| **Quiz Management** | 13 endpoints | Create, Read, Update, Delete quizzes |
| **Quiz Attempts** | 7 endpoints | Start quiz, Submit answers, View results |

**Total**: 40+ API endpoints fully documented

---

## Status

### Phase 1: Core Hierarchy ? **COMPLETED**
- ? All models created (Subject, Course, Unit, Quiz)
- ? All services implemented
- ? All controllers created
- ? Migration applied
- ? Full API documentation
- ? Frontend integration guide

### Phase 2: Navigation & Breadcrumbs ?? **IN PROGRESS**
- Frontend UI development
- Breadcrumb navigation
- Hierarchy browsing

---

## Technologies

- **.NET 8** - Backend framework
- **PostgreSQL** - Database
- **Entity Framework Core** - ORM
- **ASP.NET Core Identity** - Authentication
- **JWT Bearer** - Token-based auth
- **AutoMapper** - Object mapping
- **Swagger/OpenAPI** - API documentation

---

## Getting Help

- **API Questions**: See `api-reference.md` for complete endpoint documentation
- **Frontend Integration**: See `frontend-guide.md` for TypeScript examples
- **Business Logic**: See `business-rules.md` for validation rules
- **Setup Issues**: See `getting-started.md` and `faq.md`
- **GitHub**: https://github.com/ChristianNemeno/tapcet-api

---

## Repository

- GitHub: https://github.com/ChristianNemeno/tapcet-api

## License

See the repository root for license information.
