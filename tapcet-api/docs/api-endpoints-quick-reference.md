# API Endpoints Quick Reference

## Base URL
```
Development: https://localhost:7237
Production: TBD
```

## Authentication
All protected endpoints require:
```
Authorization: Bearer <your-jwt-token>
```

---

## ?? Complete Endpoint List

### ?? Authentication (2)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | No | Register new user |
| POST | `/api/auth/login` | No | Login and get JWT token |

### ?? Subjects (5)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/subject` | No | List all subjects |
| GET | `/api/subject/{id}` | No | Get subject with courses |
| POST | `/api/subject` | Admin | Create subject |
| PUT | `/api/subject/{id}` | Admin | Update subject |
| DELETE | `/api/subject/{id}` | Admin | Delete subject |

### ?? Courses (6)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/course` | No | List all courses |
| GET | `/api/course/{id}` | No | Get course with units |
| GET | `/api/course/subject/{subjectId}` | No | Courses by subject |
| POST | `/api/course` | Yes | Create course |
| PUT | `/api/course/{id}` | Owner | Update course |
| DELETE | `/api/course/{id}` | Owner | Delete course |

### ?? Units (7)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/unit/{id}` | No | Get unit with quizzes |
| GET | `/api/unit/course/{courseId}` | No | Units by course (ordered) |
| GET | `/api/unit/{unitId}/quizzes` | No | Quizzes by unit (ordered) |
| POST | `/api/unit` | Yes | Create unit |
| PUT | `/api/unit/{id}` | Owner | Update unit |
| PATCH | `/api/unit/{id}/reorder` | Owner | Reorder unit |
| DELETE | `/api/unit/{id}` | Owner | Delete unit |

### ? Quizzes (13)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/quiz/active` | No | Get active quizzes |
| GET | `/api/quiz/standalone` | No | Get standalone quizzes |
| GET | `/api/quiz/{id}` | No | Get quiz details |
| GET | `/api/quiz` | Yes | Get all quizzes |
| GET | `/api/quiz?unitId={id}` | Yes | Get quizzes by unit |
| GET | `/api/quiz/user/me` | Yes | Get my quizzes |
| POST | `/api/quiz` | Yes | Create quiz |
| PUT | `/api/quiz/{id}` | Owner | Update quiz |
| PATCH | `/api/quiz/{id}/assign-unit` | Owner | Assign to unit |
| PATCH | `/api/quiz/{id}/reorder` | Owner | Reorder in unit |
| PATCH | `/api/quiz/{id}/toggle` | Owner | Toggle active status |
| POST | `/api/quiz/{id}/questions` | Owner | Add question |
| DELETE | `/api/quiz/{id}` | Owner | Delete quiz |

### ?? Quiz Attempts (7)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/quiz-attempt/start` | Yes | Start quiz attempt |
| POST | `/api/quiz-attempt/submit` | Yes | Submit quiz answers |
| GET | `/api/quiz-attempt/{id}` | Owner | Get attempt by ID |
| GET | `/api/quiz-attempt/{id}/result` | Owner | Get attempt result |
| GET | `/api/quiz-attempt/user/me` | Yes | Get my attempts |
| GET | `/api/quiz-attempt/quiz/{quizId}` | Yes | Get quiz attempts |
| GET | `/api/quiz-attempt/quiz/{quizId}/leaderboard` | Yes | Get leaderboard |

**Total: 40 Endpoints**

---

## ?? Common Workflows

### 1. User Registration & Login
```http
# Register
POST /api/auth/register
Content-Type: application/json

{
  "userName": "johndoe",
  "email": "john@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!"
}

# Login
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "Password123!"
}

# Response
{
  "token": "eyJhbGciOiJIUzI1NiIsInR...",
  "email": "john@example.com",
  "userName": "johndoe",
  "expiresAt": "2024-01-15T15:30:00Z"
}
```

### 2. Browse Hierarchy
```http
# Get all subjects
GET /api/subject

# Get courses in a subject
GET /api/course/subject/1

# Get units in a course (ordered)
GET /api/unit/course/1

# Get quizzes in a unit (ordered)
GET /api/unit/1/quizzes
```

### 3. Create Complete Hierarchy
```http
# 1. Create Subject (Admin)
POST /api/subject
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "name": "Science",
  "description": "Natural sciences"
}

# 2. Create Course
POST /api/course
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "High School Physics",
  "description": "Introduction to physics",
  "subjectId": 1
}

# 3. Create Unit
POST /api/unit
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Forces and Newton's Laws",
  "orderIndex": 1,
  "courseId": 1
}

# 4. Create Quiz
POST /api/quiz
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Newton's Laws Quiz",
  "description": "Test your understanding",
  "unitId": 1,
  "orderIndex": 1,
  "questions": [
    {
      "text": "What is Newton's First Law?",
      "explanation": "An object at rest stays at rest",
      "choices": [
        { "text": "F = ma", "isCorrect": false },
        { "text": "An object at rest stays at rest", "isCorrect": true },
        { "text": "Action and reaction", "isCorrect": false }
      ]
    }
  ]
}
```

### 4. Take a Quiz
```http
# 1. Start quiz attempt
POST /api/quiz-attempt/start
Authorization: Bearer {token}
Content-Type: application/json

{
  "quizId": 1
}

# Response
{
  "id": 42,
  "quizId": 1,
  "quizTitle": "Newton's Laws Quiz",
  "startedAt": "2024-01-15T14:30:00Z",
  ...
}

# 2. Submit answers
POST /api/quiz-attempt/submit
Authorization: Bearer {token}
Content-Type: application/json

{
  "quizAttemptId": 42,
  "answers": [
    { "questionId": 1, "choiceId": 2 },
    { "questionId": 2, "choiceId": 5 }
  ]
}

# Response includes score and detailed results
{
  "quizAttemptId": 42,
  "score": 80,
  "percentage": 80.0,
  "correctAnswers": 4,
  "incorrectAnswers": 1,
  "questionResults": [...]
}
```

### 5. Manage Quiz in Unit
```http
# Assign existing quiz to unit
PATCH /api/quiz/5/assign-unit
Authorization: Bearer {token}
Content-Type: application/json

{
  "unitId": 1,
  "orderIndex": 3
}

# Reorder quiz within unit
PATCH /api/quiz/5/reorder
Authorization: Bearer {token}
Content-Type: application/json

{
  "orderIndex": 1
}

# Update quiz (change unit)
PUT /api/quiz/5
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Updated Title",
  "description": "Updated description",
  "unitId": 2,
  "orderIndex": 1,
  "isActive": true
}
```

---

## ?? Response Status Codes

| Code | Meaning | When Used |
|------|---------|-----------|
| 200 | OK | Successful GET, PUT, PATCH |
| 201 | Created | Successful POST (resource created) |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Validation error, business rule violation |
| 401 | Unauthorized | Missing/invalid token |
| 403 | Forbidden | Not authorized (e.g., not Admin) |
| 404 | Not Found | Resource doesn't exist |
| 500 | Server Error | Unexpected error |

---

## ?? Navigation Patterns

### Breadcrumb Path Queries
```
Quiz ID ? Unit ID ? Course ID ? Subject ID

GET /api/quiz/42        ? unitId: 1
GET /api/unit/1         ? courseId: 1
GET /api/course/1       ? subjectId: 1
GET /api/subject/1      ? name: "Science"

Result: Home > Science > High School Physics > Forces > Newton's Laws Quiz
```

### List by Parent
```
GET /api/subject                     # All subjects
GET /api/course/subject/1            # Courses in subject 1
GET /api/unit/course/1               # Units in course 1
GET /api/unit/1/quizzes              # Quizzes in unit 1
```

### Filter Options
```
GET /api/quiz                        # All quizzes (authenticated)
GET /api/quiz?unitId=1              # Quizzes in unit 1
GET /api/quiz/standalone            # Quizzes without unit
GET /api/quiz/active                # Only active quizzes
GET /api/quiz/user/me               # My created quizzes
```

---

## ?? Frontend Route Mapping

```typescript
// Suggested React Router routes
{
  path: '/',                                    // Home page
  path: '/subjects',                            // Subject list
  path: '/subjects/:subjectId',                 // Course list
  path: '/courses/:courseId',                   // Unit list
  path: '/units/:unitId',                       // Quiz list
  path: '/quiz/:quizId',                        // Quiz detail
  path: '/quiz/:quizId/take',                   // Take quiz
  path: '/quiz-results/:attemptId',             // View results
  path: '/my-quizzes',                          // Created quizzes
  path: '/my-attempts',                         // My attempts
  path: '/leaderboard/:quizId',                 // Quiz leaderboard
}
```

---

## ?? Tips

### Efficient Data Loading
```typescript
// Load subject with courses in one call
const subject = await fetch(`/api/subject/${id}`);
// subject.courses already included

// Load course with units in one call
const course = await fetch(`/api/course/${id}`);
// course.units already included

// Load unit with quizzes in one call
const unit = await fetch(`/api/unit/${id}`);
// unit.quizzes already included
```

### Handling OrderIndex
```typescript
// When creating:
// - Always check existing orderIndex values
// - Use next available number

// When reordering:
// - Update one item at a time
// - Or implement drag-and-drop batch update

// When deleting:
// - Optionally reassign orderIndex of remaining items
```

### Error Handling
```typescript
try {
  const response = await fetch('/api/endpoint', options);
  
  if (!response.ok) {
    const error = await response.json();
    console.error('API Error:', error.message);
    // Handle specific error codes
    if (response.status === 401) {
      // Redirect to login
    }
  }
  
  return await response.json();
} catch (error) {
  console.error('Network Error:', error);
}
```

---

## ?? See Also

- **Full API Reference**: `api-reference.md`
- **Frontend Guide**: `frontend-guide.md`
- **TypeScript Types**: `frontend-guide.md#typescript-types`
- **Hierarchy Plan**: `guides/educational-hierarchy-organization.md`

---

**Last Updated**: January 2025  
**API Version**: 1.0