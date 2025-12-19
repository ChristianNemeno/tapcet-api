# API Reference

## Base URL

**Development**: https://localhost:7237  
**Production**: TBD

## Authentication

All protected endpoints require a JWT Bearer token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

## Endpoints Summary

### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | /api/auth/register | Register new user | No |
| POST | /api/auth/login | User login | No |

### Quiz Management Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | /api/quiz | Create quiz | Yes |
| GET | /api/quiz/{id} | Get quiz by ID | No |
| GET | /api/quiz | Get all quizzes | No |
| GET | /api/quiz/active | Get active quizzes | No |
| PUT | /api/quiz/{id} | Update quiz | Yes (Owner) |
| DELETE | /api/quiz/{id} | Delete quiz | Yes (Owner) |
| PATCH | /api/quiz/{id}/toggle | Toggle quiz status | Yes (Owner) |
| POST | /api/quiz/{id}/questions | Add question | Yes (Owner) |

### Quiz Attempt Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | /api/quiz-attempt/start | Start quiz attempt | Yes |
| POST | /api/quiz-attempt/submit | Submit quiz answers | Yes |
| GET | /api/quiz-attempt/{id} | Get attempt by ID | Yes (Owner) |
| GET | /api/quiz-attempt/{id}/result | Get attempt results | Yes (Owner) |
| GET | /api/quiz-attempt/user/me | Get user's attempts | Yes |
| GET | /api/quiz-attempt/quiz/{quizId} | Get quiz attempts | Yes |
| GET | /api/quiz-attempt/quiz/{quizId}/leaderboard | Get leaderboard | Yes |

---

## Authentication API

### Register User

Register a new user account.

**Endpoint**: `POST /api/auth/register`

**Request Body**:
```json
{
  "userName": "johndoe",
  "email": "john@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!"
}
```

**Validation Rules**:
- UserName: Required, 3-50 characters
- Email: Required, valid email format, unique
- Password: Required, min 6 characters, must include uppercase, lowercase, and digit
- ConfirmPassword: Must match Password

**Success Response (200 OK)**:
```json
{
  "message": "Registration successful! Please check your email to verify your account."
}
```

**Error Response (400 Bad Request)**:
```json
{
  "message": "Registration failed",
  "errors": [
    "Email is already registered",
    "Password must contain at least one uppercase letter"
  ]
}
```

### Login

Authenticate user and receive JWT token.

**Endpoint**: `POST /api/auth/login`

**Request Body**:
```json
{
  "email": "john@example.com",
  "password": "Password123!"
}
```

**Success Response (200 OK)**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "john@example.com",
  "userName": "johndoe",
  "expiresAt": "2024-01-15T15:30:00Z"
}
```

**Error Response (401 Unauthorized)**:
```json
{
  "message": "Invalid email or password"
}
```

---

## Quiz Management API

### Create Quiz

Create a new quiz with questions and answer choices.

**Endpoint**: `POST /api/quiz`

**Authorization**: Required

**Request Body**:
```json
{
  "title": "JavaScript Basics",
  "description": "Test your JavaScript knowledge",
  "questions": [
    {
      "text": "What is a closure?",
      "explanation": "A closure is a function with access to outer scope",
      "choices": [
        {
          "text": "A loop structure",
          "isCorrect": false
        },
        {
          "text": "A function within a function",
          "isCorrect": true
        },
        {
          "text": "A variable type",
          "isCorrect": false
        }
      ]
    }
  ]
}
```

**Validation Rules**:
- Title: Required, 3-200 characters
- Description: Optional, max 2000 characters
- Questions: Required, at least 1 question
- Each question: 2-6 choices, exactly 1 correct

**Success Response (201 Created)**:
```json
{
  "id": 1,
  "title": "JavaScript Basics",
  "description": "Test your JavaScript knowledge",
  "createdAt": "2024-01-15T10:00:00Z",
  "createdById": "user-id-123",
  "createdByName": "johndoe",
  "isActive": true,
  "questionCount": 1,
  "questions": [
    {
      "id": 1,
      "text": "What is a closure?",
      "explanation": "A closure is a function with access to outer scope",
      "choices": [
        {
          "id": 1,
          "text": "A loop structure",
          "isCorrect": false
        },
        {
          "id": 2,
          "text": "A function within a function",
          "isCorrect": true
        },
        {
          "id": 3,
          "text": "A variable type",
          "isCorrect": false
        }
      ]
    }
  ]
}
```

**Response Headers**:
```
Location: /api/quiz/1
```

### Get Quiz by ID

Retrieve quiz details with all questions and choices.

**Endpoint**: `GET /api/quiz/{id}`

**Authorization**: Not required

**Path Parameters**:
- `id` (integer): Quiz ID

**Success Response (200 OK)**:
```json
{
  "id": 1,
  "title": "JavaScript Basics",
  "description": "Test your JavaScript knowledge",
  "createdAt": "2024-01-15T10:00:00Z",
  "createdById": "user-id-123",
  "createdByName": "johndoe",
  "isActive": true,
  "questionCount": 1,
  "questions": [...]
}
```

**Error Response (404 Not Found)**:
```json
{
  "message": "Quiz with ID 1 not found"
}
```

### Get All Quizzes

Retrieve all quizzes (summary view).

**Endpoint**: `GET /api/quiz`

**Authorization**: Not required

**Success Response (200 OK)**:
```json
[
  {
    "id": 1,
    "title": "JavaScript Basics",
    "description": "Test your JavaScript knowledge",
    "createdAt": "2024-01-15T10:00:00Z",
    "createdByName": "johndoe",
    "isActive": true,
    "questionCount": 5
  },
  {
    "id": 2,
    "title": "Python Fundamentals",
    "description": "Python basics quiz",
    "createdAt": "2024-01-14T09:00:00Z",
    "createdByName": "janedoe",
    "isActive": true,
    "questionCount": 10
  }
]
```

### Get Active Quizzes

Retrieve only active quizzes available for attempts.

**Endpoint**: `GET /api/quiz/active`

**Authorization**: Not required

**Success Response (200 OK)**:
```json
[
  {
    "id": 1,
    "title": "JavaScript Basics",
    "description": "Test your JavaScript knowledge",
    "questionCount": 5
  }
]
```

### Update Quiz

Update quiz metadata (title, description, status).

**Endpoint**: `PUT /api/quiz/{id}`

**Authorization**: Required (Owner only)

**Path Parameters**:
- `id` (integer): Quiz ID

**Request Body**:
```json
{
  "title": "JavaScript Advanced",
  "description": "Updated description",
  "isActive": true
}
```

**Success Response (200 OK)**:
```json
{
  "id": 1,
  "title": "JavaScript Advanced",
  "description": "Updated description",
  "isActive": true,
  "questionCount": 5
}
```

**Error Response (404 Not Found)**:
```json
{
  "message": "Quiz not found"
}
```

### Delete Quiz

Permanently delete a quiz.

**Endpoint**: `DELETE /api/quiz/{id}`

**Authorization**: Required (Owner only)

**Path Parameters**:
- `id` (integer): Quiz ID

**Success Response (204 No Content)**: No body

**Error Response (404 Not Found)**:
```json
{
  "message": "Quiz with ID 1 not found or you don't have permission to delete it"
}
```

### Toggle Quiz Status

Toggle quiz between active and inactive.

**Endpoint**: `PATCH /api/quiz/{id}/toggle`

**Authorization**: Required (Owner only)

**Path Parameters**:
- `id` (integer): Quiz ID

**Success Response (200 OK)**:
```json
{
  "message": "Status toggled"
}
```

### Add Question to Quiz

Add a new question to an existing quiz.

**Endpoint**: `POST /api/quiz/{id}/questions`

**Authorization**: Required (Owner only)

**Path Parameters**:
- `id` (integer): Quiz ID

**Request Body**:
```json
{
  "text": "What is hoisting?",
  "explanation": "Variable declarations are moved to top",
  "choices": [
    {
      "text": "A loop structure",
      "isCorrect": false
    },
    {
      "text": "Variable declaration behavior",
      "isCorrect": true
    }
  ]
}
```

**Success Response (200 OK)**: Returns updated quiz with new question

---

## Quiz Attempt API

### Start Quiz Attempt

Start a new attempt at a quiz.

**Endpoint**: `POST /api/quiz-attempt/start`

**Authorization**: Required

**Request Body**:
```json
{
  "quizId": 1
}
```

**Validation Rules**:
- Quiz must exist
- Quiz must be active
- Quiz must have questions

**Success Response (201 Created)**:
```json
{
  "id": 1,
  "quizId": 1,
  "quizTitle": "JavaScript Basics",
  "userId": "user-id-123",
  "userName": "johndoe",
  "startedAt": "2024-01-15T14:30:00Z",
  "completedAt": null,
  "score": 0,
  "isCompleted": false
}
```

**Response Headers**:
```
Location: /api/quiz-attempt/1
```

**Error Response (400 Bad Request)**:
```json
{
  "message": "Failed to start quiz. Quiz may be inactive, not found, or has no questions."
}
```

### Submit Quiz Answers

Submit all answers and receive scored results.

**Endpoint**: `POST /api/quiz-attempt/submit`

**Authorization**: Required

**Request Body**:
```json
{
  "quizAttemptId": 1,
  "answers": [
    {
      "questionId": 1,
      "choiceId": 2
    },
    {
      "questionId": 2,
      "choiceId": 6
    }
  ]
}
```

**Validation Rules**:
- Must answer all questions
- Attempt must not be already completed
- User must own the attempt

**Success Response (200 OK)**:
```json
{
  "quizAttemptId": 1,
  "quizTitle": "JavaScript Basics",
  "totalQuestions": 2,
  "correctAnswers": 1,
  "incorrectAnswers": 1,
  "score": 50,
  "percentage": 50.0,
  "startedAt": "2024-01-15T14:30:00Z",
  "completedAt": "2024-01-15T14:35:00Z",
  "duration": "00:05:00",
  "questionResults": [
    {
      "questionId": 1,
      "questionText": "What is a closure?",
      "explanation": "A closure is a function with access to outer scope",
      "selectedChoiceId": 2,
      "selectedChoiceText": "A function within a function",
      "correctChoiceId": 2,
      "correctChoiceText": "A function within a function",
      "isCorrect": true
    },
    {
      "questionId": 2,
      "questionText": "What is hoisting?",
      "explanation": "Variable declarations are moved to top",
      "selectedChoiceId": 6,
      "selectedChoiceText": "A loop structure",
      "correctChoiceId": 5,
      "correctChoiceText": "Variable declaration behavior",
      "isCorrect": false
    }
  ]
}
```

**Error Response (400 Bad Request)**:
```json
{
  "message": "Failed to submit quiz. Ensure you've answered all questions and haven't already submitted this attempt."
}
```

### Get Attempt by ID

Retrieve basic attempt information.

**Endpoint**: `GET /api/quiz-attempt/{id}`

**Authorization**: Required (Owner only)

**Path Parameters**:
- `id` (integer): Attempt ID

**Success Response (200 OK)**:
```json
{
  "id": 1,
  "quizId": 1,
  "quizTitle": "JavaScript Basics",
  "userId": "user-id-123",
  "userName": "johndoe",
  "startedAt": "2024-01-15T14:30:00Z",
  "completedAt": "2024-01-15T14:35:00Z",
  "score": 50,
  "isCompleted": true
}
```

### Get Attempt Result

Retrieve detailed results for a completed attempt.

**Endpoint**: `GET /api/quiz-attempt/{id}/result`

**Authorization**: Required (Owner only)

**Path Parameters**:
- `id` (integer): Attempt ID

**Success Response (200 OK)**: Same as Submit Quiz response

**Error Response (400 Bad Request)**:
```json
{
  "message": "Result not found. The attempt may not be completed, doesn't exist, or you don't have permission to view it."
}
```

### Get User's Attempts

Retrieve all attempts by the current user.

**Endpoint**: `GET /api/quiz-attempt/user/me`

**Authorization**: Required

**Success Response (200 OK)**:
```json
[
  {
    "id": 5,
    "quizId": 1,
    "quizTitle": "JavaScript Basics",
    "startedAt": "2024-01-15T14:30:00Z",
    "completedAt": "2024-01-15T14:35:00Z",
    "score": 80,
    "isCompleted": true
  },
  {
    "id": 3,
    "quizId": 2,
    "quizTitle": "Python Fundamentals",
    "startedAt": "2024-01-14T10:00:00Z",
    "completedAt": null,
    "score": 0,
    "isCompleted": false
  }
]
```

### Get Quiz Attempts

Retrieve all completed attempts for a specific quiz.

**Endpoint**: `GET /api/quiz-attempt/quiz/{quizId}`

**Authorization**: Required

**Path Parameters**:
- `quizId` (integer): Quiz ID

**Success Response (200 OK)**:
```json
[
  {
    "id": 42,
    "quizId": 1,
    "quizTitle": "JavaScript Basics",
    "userName": "speedster",
    "startedAt": "2024-01-15T10:00:00Z",
    "completedAt": "2024-01-15T10:05:00Z",
    "score": 100,
    "isCompleted": true
  }
]
```

### Get Leaderboard

Retrieve top performers for a quiz.

**Endpoint**: `GET /api/quiz-attempt/quiz/{quizId}/leaderboard`

**Authorization**: Required

**Path Parameters**:
- `quizId` (integer): Quiz ID

**Query Parameters**:
- `topCount` (integer, optional): Number of results (1-100, default: 10)

**Success Response (200 OK)**:
```json
[
  {
    "id": 42,
    "userName": "speedster",
    "score": 100,
    "completedAt": "2024-01-15T10:05:00Z"
  },
  {
    "id": 38,
    "userName": "genius",
    "score": 100,
    "completedAt": "2024-01-15T09:08:00Z"
  }
]
```

**Error Response (400 Bad Request)**:
```json
{
  "message": "topCount must be between 1 and 100"
}
```

---

## HTTP Status Codes

| Code | Description | When Used |
|------|-------------|-----------|
| 200 | OK | Successful GET, PUT, PATCH, POST (non-creation) |
| 201 | Created | Successful POST that creates a resource |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Validation errors, business rule violations |
| 401 | Unauthorized | Missing or invalid authentication |
| 403 | Forbidden | Authenticated but not authorized |
| 404 | Not Found | Resource doesn't exist |
| 500 | Internal Server Error | Unexpected server errors |

## Error Response Format

All error responses follow this structure:

```json
{
  "message": "Human-readable error message",
  "errors": ["Additional error details (optional)"]
}
```

## Rate Limiting

Currently not implemented. Recommended for production:
- 100 requests per minute per IP for unauthenticated endpoints
- 300 requests per minute per user for authenticated endpoints

## Pagination

Currently not implemented. All list endpoints return all results.

Recommended implementation for large datasets:
```
GET /api/quiz?page=1&pageSize=20
```

## API Versioning

Current version: v1 (implicit)

Future versions can be implemented via:
- URL path: `/api/v2/quiz`
- Header: `API-Version: 2`
- Query parameter: `/api/quiz?api-version=2`
