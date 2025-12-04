# API Endpoints Documentation

## Base URL

```
Development: https://localhost:{port}/api
```

All endpoints except `/auth/register` and `/auth/login` require JWT authentication.

---

## Authentication

Include JWT token in request headers:
```
Authorization: Bearer {your-jwt-token}
```

---

## Endpoints Overview

| Category | Endpoint | Method | Auth Required | Role |
|----------|----------|--------|---------------|------|
| **Authentication** |
| Register | `/auth/register` | POST | No | - |
| Login | `/auth/login` | POST | No | - |
| Check Email | `/auth/check-email` | POST | No | - |
| **Quiz Management** (To Be Implemented) |
| Create Quiz | `/quiz` | POST | Yes | User/Admin |
| Get Quiz | `/quiz/{id}` | GET | Yes | User/Admin |
| Get All Quizzes | `/quiz` | GET | Yes | User/Admin |
| Get Active Quizzes | `/quiz/active` | GET | Yes | User/Admin |
| Update Quiz | `/quiz/{id}` | PUT | Yes | Owner/Admin |
| Delete Quiz | `/quiz/{id}` | DELETE | Yes | Owner/Admin |
| Toggle Status | `/quiz/{id}/toggle` | PATCH | Yes | Owner/Admin |
| Add Question | `/quiz/{id}/questions` | POST | Yes | Owner/Admin |
| **Quiz Attempts** (To Be Implemented) |
| Start Attempt | `/attempt/start` | POST | Yes | User/Admin |
| Submit Attempt | `/attempt/submit` | POST | Yes | Owner |
| Get Attempt | `/attempt/{id}` | GET | Yes | Owner/Admin |
| Get User Attempts | `/attempt/user` | GET | Yes | User/Admin |
| Get Quiz Attempts | `/attempt/quiz/{quizId}` | GET | Yes | User/Admin |
| Get Attempt Result | `/attempt/{id}/result` | GET | Yes | Owner/Admin |
| Get Leaderboard | `/attempt/quiz/{quizId}/leaderboard` | GET | Yes | User/Admin |

---

## 1. Authentication Endpoints

### 1.1 Register New User

**Endpoint:** `POST /api/auth/register`

**Description:** Register a new user account.

**Authentication:** Not required

**Request Body:**
```json
{
  "username": "johndoe",
  "email": "john.doe@example.com",
  "password": "Password123"
}
```

**Validation:**
- Username: Required, 3-20 characters
- Email: Required, valid email format, unique
- Password: Required, min 6 characters, must contain uppercase, lowercase, and digit

**Success Response (200 OK):**
```json
{
  "userId": "abc123-def456-...",
  "userName": "johndoe",
  "email": "john.doe@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-15T12:00:00Z",
  "roles": ["User"]
}
```

**Error Responses:**

400 Bad Request:
```json
{
  "message": "Registration failed. Email may already be in use."
}
```

```json
{
  "errors": {
    "Email": ["The Email field is not a valid e-mail address."],
    "Password": ["Password must be at least 6 characters."]
  }
}
```

---

### 1.2 Login

**Endpoint:** `POST /api/auth/login`

**Description:** Authenticate user and receive JWT token.

**Authentication:** Not required

**Request Body:**
```json
{
  "email": "john.doe@example.com",
  "password": "Password123"
}
```

**Success Response (200 OK):**
```json
{
  "userId": "abc123-def456-...",
  "userName": "johndoe",
  "email": "john.doe@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-15T12:00:00Z",
  "roles": ["User"]
}
```

**Error Response (401 Unauthorized):**
```json
{
  "message": "Invalid email or password."
}
```

---

### 1.3 Check Email Exists

**Endpoint:** `POST /api/auth/check-email?email={email}`

**Description:** Check if an email is already registered.

**Authentication:** Not required

**Query Parameters:**
- `email` (string, required): Email to check

**Success Response (200 OK):**
```json
{
  "exists": true
}
```

---

## 2. Quiz Management Endpoints

### 2.1 Create Quiz

**Endpoint:** `POST /api/quiz`

**Description:** Create a new quiz with questions and choices.

**Authentication:** Required (User/Admin)

**Request Body:**
```json
{
  "title": "JavaScript Fundamentals Quiz",
  "description": "Test your knowledge of JavaScript basics",
  "questions": [
    {
      "text": "What is the output of typeof null?",
      "explanation": "This is a known quirk in JavaScript. typeof null returns 'object' due to a bug in the original JavaScript implementation.",
      "imageUrl": null,
      "choices": [
        {
          "text": "null",
          "isCorrect": false
        },
        {
          "text": "object",
          "isCorrect": true
        },
        {
          "text": "undefined",
          "isCorrect": false
        },
        {
          "text": "number",
          "isCorrect": false
        }
      ]
    },
    {
      "text": "Which keyword is used to declare a constant?",
      "explanation": "The const keyword creates a read-only reference to a value.",
      "imageUrl": null,
      "choices": [
        {
          "text": "var",
          "isCorrect": false
        },
        {
          "text": "let",
          "isCorrect": false
        },
        {
          "text": "const",
          "isCorrect": true
        }
      ]
    }
  ]
}
```

**Success Response (200 OK):**
```json
{
  "id": 1,
  "title": "JavaScript Fundamentals Quiz",
  "description": "Test your knowledge of JavaScript basics",
  "createdAt": "2024-01-15T10:00:00Z",
  "createdById": "abc123-def456-...",
  "createdByName": "johndoe",
  "isActive": true,
  "questionCount": 2,
  "questions": [
    {
      "id": 1,
      "text": "What is the output of typeof null?",
      "explanation": "This is a known quirk in JavaScript...",
      "imageUrl": null,
      "choices": [
        {
          "id": 1,
          "text": "null",
          "isCorrect": false
        },
        {
          "id": 2,
          "text": "object",
          "isCorrect": true
        }
        // ... more choices
      ]
    }
    // ... more questions
  ]
}
```

**Error Responses:**

400 Bad Request (No questions):
```json
{
  "message": "Quiz must have at least one question."
}
```

400 Bad Request (Invalid question):
```json
{
  "message": "Each question must have at least 2 choices and exactly one correct answer."
}
```

---

### 2.2 Get Quiz by ID

**Endpoint:** `GET /api/quiz/{id}`

**Description:** Get detailed information about a specific quiz.

**Authentication:** Required

**Path Parameters:**
- `id` (int, required): Quiz ID

**Success Response (200 OK):**
Same as Create Quiz response

**Error Response (404 Not Found):**
```json
{
  "message": "Quiz not found."
}
```

---

### 2.3 Get All Quizzes

**Endpoint:** `GET /api/quiz`

**Description:** Get summary of all quizzes (including inactive).

**Authentication:** Required

**Success Response (200 OK):**
```json
[
  {
    "id": 1,
    "title": "JavaScript Fundamentals Quiz",
    "description": "Test your knowledge of JavaScript basics",
    "createdAt": "2024-01-15T10:00:00Z",
    "createdByName": "johndoe",
    "isActive": true,
    "questionCount": 5,
    "attemptCount": 23
  },
  {
    "id": 2,
    "title": "Python Basics",
    "description": "Python programming fundamentals",
    "createdAt": "2024-01-14T15:30:00Z",
    "createdByName": "janedoe",
    "isActive": true,
    "questionCount": 10,
    "attemptCount": 45
  }
]
```

---

### 2.4 Get Active Quizzes

**Endpoint:** `GET /api/quiz/active`

**Description:** Get summary of only active quizzes.

**Authentication:** Required

**Success Response (200 OK):**
Same format as Get All Quizzes, but filtered to `isActive: true`

---

### 2.5 Update Quiz

**Endpoint:** `PUT /api/quiz/{id}`

**Description:** Update quiz metadata (title, description, status).

**Authentication:** Required (Owner/Admin only)

**Path Parameters:**
- `id` (int, required): Quiz ID

**Request Body:**
```json
{
  "title": "Updated Quiz Title",
  "description": "Updated description",
  "isActive": false
}
```

**Success Response (200 OK):**
Returns updated quiz (same format as Get Quiz)

**Error Responses:**

404 Not Found:
```json
{
  "message": "Quiz not found."
}
```

403 Forbidden:
```json
{
  "message": "You do not have permission to update this quiz."
}
```

---

### 2.6 Delete Quiz

**Endpoint:** `DELETE /api/quiz/{id}`

**Description:** Permanently delete a quiz.

**Authentication:** Required (Owner/Admin only)

**Path Parameters:**
- `id` (int, required): Quiz ID

**Success Response (204 No Content)**

**Error Responses:**

404 Not Found:
```json
{
  "message": "Quiz not found."
}
```

403 Forbidden:
```json
{
  "message": "You do not have permission to delete this quiz."
}
```

400 Bad Request (Has attempts):
```json
{
  "message": "Cannot delete quiz with existing attempts."
}
```

---

### 2.7 Toggle Quiz Status

**Endpoint:** `PATCH /api/quiz/{id}/toggle`

**Description:** Toggle quiz active/inactive status.

**Authentication:** Required (Owner/Admin only)

**Path Parameters:**
- `id` (int, required): Quiz ID

**Success Response (200 OK):**
```json
{
  "message": "Quiz status updated successfully.",
  "isActive": false
}
```

**Error Responses:** Same as Update Quiz

---

### 2.8 Add Question to Quiz

**Endpoint:** `POST /api/quiz/{id}/questions`

**Description:** Add a new question to existing quiz.

**Authentication:** Required (Owner only)

**Path Parameters:**
- `id` (int, required): Quiz ID

**Request Body:**
```json
{
  "text": "What is a closure in JavaScript?",
  "explanation": "A closure is the combination of a function and the lexical environment...",
  "imageUrl": null,
  "choices": [
    {
      "text": "A function inside another function",
      "isCorrect": false
    },
    {
      "text": "A function that has access to outer function's variables",
      "isCorrect": true
    },
    {
      "text": "A way to close browser windows",
      "isCorrect": false
    }
  ]
}
```

**Success Response (200 OK):**
Returns updated quiz with all questions

**Error Responses:** Same as Create Quiz validation errors

---

## 3. Quiz Attempt Endpoints

### 3.1 Start Quiz Attempt

**Endpoint:** `POST /api/attempt/start`

**Description:** Start a new quiz attempt.

**Authentication:** Required

**Request Body:**
```json
{
  "quizId": 1
}
```

**Success Response (200 OK):**
```json
{
  "id": 1,
  "quizId": 1,
  "quizTitle": "JavaScript Fundamentals Quiz",
  "userId": "abc123-def456-...",
  "userName": "johndoe",
  "startedAt": "2024-01-15T14:30:00Z",
  "completedAt": null,
  "score": 0,
  "isCompleted": false
}
```

**Error Responses:**

404 Not Found:
```json
{
  "message": "Quiz not found or is inactive."
}
```

400 Bad Request:
```json
{
  "message": "Quiz has no questions."
}
```

---

### 3.2 Submit Quiz Attempt

**Endpoint:** `POST /api/attempt/submit`

**Description:** Submit answers and get results.

**Authentication:** Required (Attempt owner only)

**Request Body:**
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
      "choiceId": 7
    },
    {
      "questionId": 3,
      "choiceId": 11
    }
  ]
}
```

**Success Response (200 OK):**
```json
{
  "quizAttemptId": 1,
  "quizTitle": "JavaScript Fundamentals Quiz",
  "totalQuestions": 3,
  "correctAnswers": 2,
  "incorrectAnswers": 1,
  "score": 67,
  "percentage": 67.0,
  "startedAt": "2024-01-15T14:30:00Z",
  "completedAt": "2024-01-15T14:45:00Z",
  "duration": "00:15:00",
  "questionResults": [
    {
      "questionId": 1,
      "questionText": "What is the output of typeof null?",
      "explanation": "This is a known quirk in JavaScript...",
      "selectedChoiceId": 2,
      "selectedChoiceText": "object",
      "correctChoiceId": 2,
      "correctChoiceText": "object",
      "isCorrect": true
    },
    {
      "questionId": 2,
      "questionText": "Which keyword is used to declare a constant?",
      "explanation": "The const keyword creates a read-only reference...",
      "selectedChoiceId": 7,
      "selectedChoiceText": "let",
      "correctChoiceId": 8,
      "correctChoiceText": "const",
      "isCorrect": false
    }
    // ... more results
  ]
}
```

**Error Responses:**

404 Not Found:
```json
{
  "message": "Quiz attempt not found."
}
```

400 Bad Request (Already completed):
```json
{
  "message": "Quiz attempt already completed."
}
```

400 Bad Request (Answer count mismatch):
```json
{
  "message": "You must answer all questions."
}
```

---

### 3.3 Get Attempt by ID

**Endpoint:** `GET /api/attempt/{id}`

**Description:** Get basic attempt information.

**Authentication:** Required (Owner/Admin only)

**Path Parameters:**
- `id` (int, required): Attempt ID

**Success Response (200 OK):**
Same as Start Attempt response

---

### 3.4 Get User's Attempts

**Endpoint:** `GET /api/attempt/user`

**Description:** Get all attempts for the authenticated user.

**Authentication:** Required

**Success Response (200 OK):**
```json
[
  {
    "id": 1,
    "quizId": 1,
    "quizTitle": "JavaScript Fundamentals Quiz",
    "userId": "abc123-def456-...",
    "userName": "johndoe",
    "startedAt": "2024-01-15T14:30:00Z",
    "completedAt": "2024-01-15T14:45:00Z",
    "score": 67,
    "isCompleted": true
  },
  {
    "id": 2,
    "quizId": 2,
    "quizTitle": "Python Basics",
    "userId": "abc123-def456-...",
    "userName": "johndoe",
    "startedAt": "2024-01-16T10:00:00Z",
    "completedAt": null,
    "score": 0,
    "isCompleted": false
  }
]
```

---

### 3.5 Get Quiz Attempts

**Endpoint:** `GET /api/attempt/quiz/{quizId}`

**Description:** Get all completed attempts for a specific quiz.

**Authentication:** Required

**Path Parameters:**
- `quizId` (int, required): Quiz ID

**Success Response (200 OK):**
Array of attempt objects (same format as User Attempts), filtered to completed only

---

### 3.6 Get Attempt Result

**Endpoint:** `GET /api/attempt/{id}/result`

**Description:** Get detailed results for a completed attempt.

**Authentication:** Required (Owner/Admin only)

**Path Parameters:**
- `id` (int, required): Attempt ID

**Success Response (200 OK):**
Same format as Submit Attempt response

**Error Responses:**

404 Not Found:
```json
{
  "message": "Attempt not found or not completed."
}
```

---

### 3.7 Get Quiz Leaderboard

**Endpoint:** `GET /api/attempt/quiz/{quizId}/leaderboard?topCount=10`

**Description:** Get top performers for a quiz.

**Authentication:** Required

**Path Parameters:**
- `quizId` (int, required): Quiz ID

**Query Parameters:**
- `topCount` (int, optional, default 10): Number of top results

**Success Response (200 OK):**
```json
[
  {
    "id": 15,
    "quizId": 1,
    "quizTitle": "JavaScript Fundamentals Quiz",
    "userId": "user1-id",
    "userName": "alice",
    "startedAt": "2024-01-15T09:00:00Z",
    "completedAt": "2024-01-15T09:12:00Z",
    "score": 100,
    "isCompleted": true
  },
  {
    "id": 23,
    "quizId": 1,
    "quizTitle": "JavaScript Fundamentals Quiz",
    "userId": "user2-id",
    "userName": "bob",
    "startedAt": "2024-01-15T10:00:00Z",
    "completedAt": "2024-01-15T10:10:00Z",
    "score": 95,
    "isCompleted": true
  }
  // ... more results
]
```

---

## 4. Error Response Format

All error responses follow a consistent format:

**Validation Errors (400):**
```json
{
  "errors": {
    "FieldName": [
      "Error message 1",
      "Error message 2"
    ]
  }
}
```

**Business Logic Errors (400):**
```json
{
  "message": "Descriptive error message"
}
```

**Authentication Errors (401):**
```json
{
  "message": "Invalid credentials or token expired"
}
```

**Authorization Errors (403):**
```json
{
  "message": "You do not have permission to perform this action"
}
```

**Not Found Errors (404):**
```json
{
  "message": "Resource not found"
}
```

**Server Errors (500):**
```json
{
  "message": "An unexpected error occurred"
}
```

---

## 5. Using the API

### 5.1 Swagger UI

Access interactive API documentation at:
```
https://localhost:{port}/swagger
```

Features:
- Try out endpoints directly
- See request/response schemas
- JWT authentication support

### 5.2 Authentication Flow

1. Register or login to get JWT token
2. Copy the token from response
3. Click "Authorize" in Swagger (or add header manually)
4. Enter: `Bearer {your-token}`
5. All subsequent requests will include the token

### 5.3 Example: Complete Quiz Flow

```javascript
// 1. Login
POST /api/auth/login
{
  "email": "john@example.com",
  "password": "Password123"
}
// Save token from response

// 2. Get active quizzes
GET /api/quiz/active
// Authorization: Bearer {token}

// 3. Start quiz attempt
POST /api/attempt/start
{
  "quizId": 1
}
// Save attemptId from response

// 4. Submit answers
POST /api/attempt/submit
{
  "quizAttemptId": 1,
  "answers": [
    { "questionId": 1, "choiceId": 2 },
    { "questionId": 2, "choiceId": 7 }
  ]
}
// Get results in response

// 5. View leaderboard
GET /api/attempt/quiz/1/leaderboard?topCount=10
```

---

## 6. Rate Limiting

**Current Status:** Not implemented

**Recommendations:**
- 100 requests per minute per user
- 1000 requests per hour per IP
- Stricter limits for authentication endpoints

---

## 7. API Versioning

**Current Version:** v1 (implicit)

**Future Consideration:**
- URL versioning: `/api/v1/quiz`
- Header versioning: `Accept: application/vnd.tapcet.v1+json`

---

**Last Updated**: 2024
**API Version**: 1.0.0 (In Development)
