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

### Educational Hierarchy Endpoints

#### Subject Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | /api/subject | List all subjects | No |
| GET | /api/subject/{id} | Get subject with courses | No |
| POST | /api/subject | Create subject | Yes (Admin) |
| PUT | /api/subject/{id} | Update subject | Yes (Admin) |
| DELETE | /api/subject/{id} | Delete subject | Yes (Admin) |

#### Course Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | /api/course | List all courses | No |
| GET | /api/course/{id} | Get course with units | No |
| GET | /api/course/subject/{subjectId} | Courses by subject | No |
| POST | /api/course | Create course | Yes |
| PUT | /api/course/{id} | Update course | Yes (Owner) |
| DELETE | /api/course/{id} | Delete course | Yes (Owner) |

#### Unit Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | /api/unit/{id} | Get unit with quizzes | No |
| GET | /api/unit/course/{courseId} | Units by course (ordered) | No |
| GET | /api/unit/{unitId}/quizzes | Quizzes by unit (ordered) | No |
| POST | /api/unit | Create unit | Yes |
| PUT | /api/unit/{id} | Update unit | Yes (Owner) |
| PATCH | /api/unit/{id}/reorder | Reorder unit | Yes (Owner) |
| DELETE | /api/unit/{id} | Delete unit | Yes (Owner) |

### Quiz Management Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | /api/quiz | Create quiz | Yes |
| GET | /api/quiz/{id} | Get quiz by ID | No |
| GET | /api/quiz | Get all quizzes | Yes |
| GET | /api/quiz?unitId={id} | Get quizzes by unit | Yes |
| GET | /api/quiz/standalone | Get standalone quizzes | No |
| GET | /api/quiz/active | Get active quizzes | No |
| GET | /api/quiz/user/me | Get my quizzes | Yes |
| PUT | /api/quiz/{id} | Update quiz | Yes (Owner) |
| PATCH | /api/quiz/{id}/assign-unit | Assign quiz to unit | Yes (Owner) |
| PATCH | /api/quiz/{id}/reorder | Reorder quiz in unit | Yes (Owner) |
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

## Educational Hierarchy API

### Subject API

#### Create Subject

Create a new subject (admin only).

**Endpoint**: `POST /api/subject`

**Authorization**: Required (Admin role)

**Request Body**:
```json
{
  "name": "Science",
  "description": "Natural sciences including physics, chemistry, biology"
}
```

**Validation Rules**:
- Name: Required, 2-100 characters, must be unique (case-insensitive)
- Description: Optional, max 500 characters

**Success Response (201 Created)**:
```json
{
  "id": 1,
  "name": "Science",
  "description": "Natural sciences including physics, chemistry, biology",
  "courseCount": 0
}
```

**Response Headers**:
```
Location: /api/subject/1
```

**Error Response (400 Bad Request)**:
```json
{
  "message": "Failed to create subject. A subject with this name may already exist."
}
```

#### Get All Subjects

Retrieve all subjects with course counts.

**Endpoint**: `GET /api/subject`

**Authorization**: Not required

**Success Response (200 OK)**:
```json
[
  {
    "id": 1,
    "name": "Science",
    "description": "Natural sciences including physics, chemistry, biology",
    "courseCount": 3
  },
  {
    "id": 2,
    "name": "Programming",
    "description": "Computer programming and software development",
    "courseCount": 5
  }
]
```

#### Get Subject by ID

Retrieve subject with all courses.

**Endpoint**: `GET /api/subject/{id}`

**Authorization**: Not required

**Path Parameters**:
- `id` (integer): Subject ID

**Success Response (200 OK)**:
```json
{
  "id": 1,
  "name": "Science",
  "description": "Natural sciences including physics, chemistry, biology",
  "courses": [
    {
      "id": 1,
      "title": "High School Physics",
      "description": "Introduction to physics concepts",
      "subjectId": 1,
      "subjectName": "Science",
      "unitCount": 4
    },
    {
      "id": 2,
      "title": "College Chemistry",
      "description": "Advanced chemistry topics",
      "subjectId": 1,
      "subjectName": "Science",
      "unitCount": 6
    }
  ]
}
```

**Error Response (404 Not Found)**:
```json
{
  "message": "Subject with ID 1 not found"
}
```

#### Update Subject

Update subject details (admin only).

**Endpoint**: `PUT /api/subject/{id}`

**Authorization**: Required (Admin role)

**Path Parameters**:
- `id` (integer): Subject ID

**Request Body**:
```json
{
  "name": "Natural Sciences",
  "description": "Updated description"
}
```

**Success Response (200 OK)**:
```json
{
  "id": 1,
  "name": "Natural Sciences",
  "description": "Updated description",
  "courseCount": 3
}
```

**Error Response (400 Bad Request)**:
```json
{
  "message": "Failed to update subject. Subject not found or name already exists."
}
```

#### Delete Subject

Delete a subject (admin only). Subject must have no courses.

**Endpoint**: `DELETE /api/subject/{id}`

**Authorization**: Required (Admin role)

**Path Parameters**:
- `id` (integer): Subject ID

**Success Response (204 No Content)**: No body

**Error Response (400 Bad Request)**:
```json
{
  "message": "Cannot delete subject. Subject not found or has existing courses."
}
```

---

### Course API

#### Create Course

Create a new course within a subject.

**Endpoint**: `POST /api/course`

**Authorization**: Required

**Request Body**:
```json
{
  "title": "High School Physics",
  "description": "Comprehensive introduction to physics concepts",
  "subjectId": 1
}
```

**Validation Rules**:
- Title: Required, 3-100 characters
- Description: Optional, max 500 characters
- SubjectId: Required, must exist

**Success Response (201 Created)**:
```json
{
  "id": 1,
  "title": "High School Physics",
  "description": "Comprehensive introduction to physics concepts",
  "subjectId": 1,
  "subjectName": "Science",
  "unitCount": 0
}
```

**Response Headers**:
```
Location: /api/course/1
```

**Error Response (400 Bad Request)**:
```json
{
  "message": "Failed to create course. Subject may not exist."
}
```

#### Get All Courses

Retrieve all courses.

**Endpoint**: `GET /api/course`

**Authorization**: Not required

**Success Response (200 OK)**:
```json
[
  {
    "id": 1,
    "title": "High School Physics",
    "description": "Introduction to physics",
    "subjectId": 1,
    "subjectName": "Science",
    "unitCount": 4
  },
  {
    "id": 2,
    "title": "Python for Beginners",
    "description": "Learn Python from scratch",
    "subjectId": 2,
    "subjectName": "Programming",
    "unitCount": 3
  }
]
```

#### Get Course by ID

Retrieve course with all units.

**Endpoint**: `GET /api/course/{id}`

**Authorization**: Not required

**Path Parameters**:
- `id` (integer): Course ID

**Success Response (200 OK)**:
```json
{
  "id": 1,
  "title": "High School Physics",
  "description": "Introduction to physics concepts",
  "subjectId": 1,
  "subjectName": "Science",
  "units": [
    {
      "id": 1,
      "title": "Forces and Newton's Laws",
      "orderIndex": 1,
      "courseId": 1,
      "courseTitle": "High School Physics",
      "quizCount": 2
    },
    {
      "id": 2,
      "title": "Energy and Work",
      "orderIndex": 2,
      "courseId": 1,
      "courseTitle": "High School Physics",
      "quizCount": 3
    }
  ]
}
```

**Error Response (404 Not Found)**:
```json
{
  "message": "Course with ID 1 not found"
}
```

#### Get Courses by Subject

Retrieve all courses in a subject.

**Endpoint**: `GET /api/course/subject/{subjectId}`

**Authorization**: Not required

**Path Parameters**:
- `subjectId` (integer): Subject ID

**Success Response (200 OK)**:
```json
[
  {
    "id": 1,
    "title": "High School Physics",
    "description": "Introduction to physics",
    "subjectId": 1,
    "subjectName": "Science",
    "unitCount": 4
  },
  {
    "id": 2,
    "title": "College Chemistry",
    "description": "Advanced chemistry",
    "subjectId": 1,
    "subjectName": "Science",
    "unitCount": 6
  }
]
```

#### Update Course

Update course details.

**Endpoint**: `PUT /api/course/{id}`

**Authorization**: Required (Owner)

**Path Parameters**:
- `id` (integer): Course ID

**Request Body**:
```json
{
  "title": "Advanced Physics",
  "description": "Updated description",
  "subjectId": 1
}
```

**Success Response (200 OK)**:
```json
{
  "id": 1,
  "title": "Advanced Physics",
  "description": "Updated description",
  "subjectId": 1,
  "subjectName": "Science",
  "unitCount": 4
}
```

**Error Response (400 Bad Request)**:
```json
{
  "message": "Failed to update course. Course or subject may not exist."
}
```

#### Delete Course

Delete a course. Course must have no units.

**Endpoint**: `DELETE /api/course/{id}`

**Authorization**: Required (Owner)

**Path Parameters**:
- `id` (integer): Course ID

**Success Response (204 No Content)**: No body

**Error Response (400 Bad Request)**:
```json
{
  "message": "Cannot delete course. Course not found or has existing units."
}
```

---

### Unit API

#### Create Unit

Create a new unit within a course.

**Endpoint**: `POST /api/unit`

**Authorization**: Required

**Request Body**:
```json
{
  "title": "Forces and Newton's Laws",
  "orderIndex": 1,
  "courseId": 1
}
```

**Validation Rules**:
- Title: Required, 3-100 characters
- OrderIndex: Required, 1-999, must be unique within course
- CourseId: Required, must exist

**Success Response (201 Created)**:
```json
{
  "id": 1,
  "title": "Forces and Newton's Laws",
  "orderIndex": 1,
  "courseId": 1,
  "courseTitle": "High School Physics",
  "quizCount": 0
}
```

**Response Headers**:
```
Location: /api/unit/1
```

**Error Response (400 Bad Request)**:
```json
{
  "message": "Failed to create unit. Course may not exist or order index is already in use."
}
```

#### Get Unit by ID

Retrieve unit with all quizzes.

**Endpoint**: `GET /api/unit/{id}`

**Authorization**: Not required

**Path Parameters**:
- `id` (integer): Unit ID

**Success Response (200 OK)**:
```json
{
  "id": 1,
  "title": "Forces and Newton's Laws",
  "orderIndex": 1,
  "courseId": 1,
  "courseTitle": "High School Physics",
  "quizzes": [
    {
      "id": 1,
      "title": "Newton's Laws Quiz",
      "description": "Test your understanding",
      "unitId": 1,
      "unitTitle": "Forces and Newton's Laws",
      "orderIndex": 1,
      "createdAt": "2024-01-15T10:00:00Z",
      "createdByName": "johndoe",
      "isActive": true,
      "questionCount": 5,
      "attemptCount": 12
    }
  ]
}
```

**Error Response (404 Not Found)**:
```json
{
  "message": "Unit with ID 1 not found"
}
```

#### Get Units by Course

Retrieve all units in a course (ordered by orderIndex).

**Endpoint**: `GET /api/unit/course/{courseId}`

**Authorization**: Not required

**Path Parameters**:
- `courseId` (integer): Course ID

**Success Response (200 OK)**:
```json
[
  {
    "id": 1,
    "title": "Forces and Newton's Laws",
    "orderIndex": 1,
    "courseId": 1,
    "courseTitle": "High School Physics",
    "quizCount": 2
  },
  {
    "id": 2,
    "title": "Energy and Work",
    "orderIndex": 2,
    "courseId": 1,
    "courseTitle": "High School Physics",
    "quizCount": 3
  }
]
```

#### Get Quizzes by Unit

Retrieve all quizzes in a unit (ordered by orderIndex).

**Endpoint**: `GET /api/unit/{unitId}/quizzes`

**Authorization**: Not required

**Path Parameters**:
- `unitId` (integer): Unit ID

**Success Response (200 OK)**:
```json
[
  {
    "id": 1,
    "title": "Newton's Laws Quiz",
    "description": "Test your understanding",
    "unitId": 1,
    "unitTitle": "Forces and Newton's Laws",
    "orderIndex": 1,
    "createdAt": "2024-01-15T10:00:00Z",
    "createdByName": "johndoe",
    "isActive": true,
    "questionCount": 5,
    "attemptCount": 12
  },
  {
    "id": 2,
    "title": "Friction and Gravity",
    "description": "Advanced topics",
    "unitId": 1,
    "unitTitle": "Forces and Newton's Laws",
    "orderIndex": 2,
    "createdAt": "2024-01-16T09:00:00Z",
    "createdByName": "teacher123",
    "isActive": true,
    "questionCount": 8,
    "attemptCount": 5
  }
]
```

#### Update Unit

Update unit details.

**Endpoint**: `PUT /api/unit/{id}`

**Authorization**: Required (Owner)

**Path Parameters**:
- `id` (integer): Unit ID

**Request Body**:
```json
{
  "title": "Forces and Motion",
  "orderIndex": 1,
  "courseId": 1
}
```

**Success Response (200 OK)**:
```json
{
  "id": 1,
  "title": "Forces and Motion",
  "orderIndex": 1,
  "courseId": 1,
  "courseTitle": "High School Physics",
  "quizCount": 2
}
```

**Error Response (400 Bad Request)**:
```json
{
  "message": "Failed to update unit. Unit or course may not exist, or order index is already in use."
}
```

#### Reorder Unit

Change unit's order within its course.

**Endpoint**: `PATCH /api/unit/{id}/reorder`

**Authorization**: Required (Owner)

**Path Parameters**:
- `id` (integer): Unit ID

**Request Body**:
```json
{
  "orderIndex": 3
}
```

**Validation Rules**:
- OrderIndex: Required, 1-999

**Success Response (200 OK)**:
```json
{
  "id": 1,
  "title": "Forces and Newton's Laws",
  "orderIndex": 3,
  "courseId": 1,
  "courseTitle": "High School Physics",
  "quizCount": 2
}
```

**Error Response (400 Bad Request)**:
```json
{
  "message": "Failed to reorder unit. Unit not found or order index is already in use."
}
```

#### Delete Unit

Delete a unit. Quizzes in the unit will be orphaned (UnitId set to null).

**Endpoint**: `DELETE /api/unit/{id}`

**Authorization**: Required (Owner)

**Path Parameters**:
- `id` (integer): Unit ID

**Success Response (204 No Content)**: No body

**Error Response (400 Bad Request)**:
```json
{
  "message": "Cannot delete unit. Unit not found."
}
```

---

## Quiz Management API

### Create Quiz

Create a new quiz with optional unit assignment.

**Endpoint**: `POST /api/quiz`

**Authorization**: Required

**Request Body**:
```json
{
  "title": "Newton's Laws Quiz",
  "description": "Test your understanding of Newton's three laws",
  "unitId": 1,
  "orderIndex": 1,
  "questions": [
    {
      "text": "What is Newton's First Law?",
      "explanation": "An object at rest stays at rest unless acted upon by a force",
      "choices": [
        {
          "text": "Force equals mass times acceleration",
          "isCorrect": false
        },
        {
          "text": "An object at rest stays at rest",
          "isCorrect": true
        },
        {
          "text": "For every action there is an equal and opposite reaction",
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
- UnitId: Optional (null for standalone quizzes)
- OrderIndex: Optional, 1-999, defaults to 1
- Questions: Required, at least 1 question
- Each question: 2-6 choices, exactly 1 correct

**Success Response (201 Created)**:
```json
{
  "id": 1,
  "title": "Newton's Laws Quiz",
  "description": "Test your understanding of Newton's three laws",
  "unitId": 1,
  "unitTitle": "Forces and Newton's Laws",
  "orderIndex": 1,
  "createdAt": "2024-01-15T10:00:00Z",
  "createdById": "user-id-123",
  "createdByName": "johndoe",
  "isActive": true,
  "questionCount": 1,
  "questions": [...]
}
```

**Response Headers**:
```
Location: /api/quiz/1
```

### Get Quiz by ID

Retrieve quiz details with all questions, choices, and unit information.

**Endpoint**: `GET /api/quiz/{id}`

**Authorization**: Not required

**Path Parameters**:
- `id` (integer): Quiz ID

**Success Response (200 OK)**:
```json
{
  "id": 1,
  "title": "Newton's Laws Quiz",
  "description": "Test your understanding",
  "unitId": 1,
  "unitTitle": "Forces and Newton's Laws",
  "orderIndex": 1,
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

Retrieve all quizzes with optional unit filtering.

**Endpoint**: `GET /api/quiz?unitId={unitId}`

**Authorization**: Required

**Query Parameters**:
- `unitId` (integer, optional): Filter by unit ID

**Success Response (200 OK)**:
```json
[
  {
    "id": 1,
    "title": "Newton's Laws Quiz",
    "description": "Test your understanding",
    "unitId": 1,
    "unitTitle": "Forces and Newton's Laws",
    "orderIndex": 1,
    "createdAt": "2024-01-15T10:00:00Z",
    "createdByName": "johndoe",
    "isActive": true,
    "questionCount": 5,
    "attemptCount": 12
  }
]
```

### Get Standalone Quizzes

Retrieve quizzes not assigned to any unit.

**Endpoint**: `GET /api/quiz/standalone`

**Authorization**: Not required

**Success Response (200 OK)**:
```json
[
  {
    "id": 42,
    "title": "General Knowledge Quiz",
    "description": "Mixed topics",
    "unitId": null,
    "unitTitle": null,
    "orderIndex": 0,
    "createdAt": "2024-01-10T14:00:00Z",
    "createdByName": "admin",
    "isActive": true,
    "questionCount": 10,
    "attemptCount": 25
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
    "title": "Newton's Laws Quiz",
    "description": "Test your understanding",
    "unitId": 1,
    "unitTitle": "Forces and Newton's Laws",
    "orderIndex": 1,
    "questionCount": 5
  }
]
```

### Get My Quizzes

Retrieve all quizzes created by the current user.

**Endpoint**: `GET /api/quiz/user/me`

**Authorization**: Required

**Success Response (200 OK)**:
```json
[
  {
    "id": 1,
    "title": "Newton's Laws Quiz",
    "description": "Test your understanding",
    "unitId": 1,
    "unitTitle": "Forces and Newton's Laws",
    "orderIndex": 1,
    "createdAt": "2024-01-15T10:00:00Z",
    "createdByName": "johndoe",
    "isActive": true,
    "questionCount": 5,
    "attemptCount": 12
  }
]
```

### Update Quiz

Update quiz metadata including unit assignment.

**Endpoint**: `PUT /api/quiz/{id}`

**Authorization**: Required (Owner only)

**Path Parameters**:
- `id` (integer): Quiz ID

**Request Body**:
```json
{
  "title": "Newton's Laws - Advanced",
  "description": "Updated description",
  "unitId": 2,
  "orderIndex": 3,
  "isActive": true
}
```

**Success Response (200 OK)**:
```json
{
  "id": 1,
  "title": "Newton's Laws - Advanced",
  "description": "Updated description",
  "unitId": 2,
  "unitTitle": "Energy and Work",
  "orderIndex": 3,
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

### Assign Quiz to Unit

Assign an existing quiz to a unit.

**Endpoint**: `PATCH /api/quiz/{id}/assign-unit`

**Authorization**: Required (Owner only)

**Path Parameters**:
- `id` (integer): Quiz ID

**Request Body**:
```json
{
  "unitId": 1,
  "orderIndex": 2
}
```

**Validation Rules**:
- UnitId: Required, must exist
- OrderIndex: Required, 1-999

**Success Response (200 OK)**:
```json
{
  "id": 1,
  "title": "Newton's Laws Quiz",
  "unitId": 1,
  "unitTitle": "Forces and Newton's Laws",
  "orderIndex": 2,
  "questionCount": 5
}
```

**Error Response (400 Bad Request)**:
```json
{
  "message": "Failed to assign quiz to unit. Quiz or unit may not exist, or you don't have permission."
}
```

### Reorder Quiz

Change quiz order within its unit.

**Endpoint**: `PATCH /api/quiz/{id}/reorder`

**Authorization**: Required (Owner only)

**Path Parameters**:
- `id` (integer): Quiz ID

**Request Body**:
```json
{
  "orderIndex": 5
}
```

**Validation Rules**:
- OrderIndex: Required, 1-999

**Success Response (200 OK)**:
```json
{
  "id": 1,
  "title": "Newton's Laws Quiz",
  "unitId": 1,
  "unitTitle": "Forces and Newton's Laws",
  "orderIndex": 5,
  "questionCount": 5
}
```

**Error Response (400 Bad Request)**:
```json
{
  "message": "Failed to reorder quiz. Quiz may not exist or you don't have permission."
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
  "text": "What is Newton's Second Law?",
  "explanation": "Force equals mass times acceleration (F=ma)",
  "choices": [
    {
      "text": "An object at rest stays at rest",
      "isCorrect": false
    },
    {
      "text": "F = ma",
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
  "quizTitle": "Newton's Laws Quiz",
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
  "quizTitle": "Newton's Laws Quiz",
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
      "questionText": "What is Newton's First Law?",
      "explanation": "An object at rest stays at rest",
      "selectedChoiceId": 2,
      "selectedChoiceText": "An object at rest stays at rest",
      "correctChoiceId": 2,
      "correctChoiceText": "An object at rest stays at rest",
      "isCorrect": true
    },
    {
      "questionId": 2,
      "questionText": "What is Newton's Second Law?",
      "explanation": "F = ma",
      "selectedChoiceId": 6,
      "selectedChoiceText": "An object at rest stays at rest",
      "correctChoiceId": 5,
      "correctChoiceText": "F = ma",
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
  "quizTitle": "Newton's Laws Quiz",
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
    "quizTitle": "Newton's Laws Quiz",
    "startedAt": "2024-01-15T14:30:00Z",
    "completedAt": "2024-01-15T14:35:00Z",
    "score": 80,
    "isCompleted": true
  },
  {
    "id": 3,
    "quizId": 2,
    "quizTitle": "Energy and Work Quiz",
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
    "quizTitle": "Newton's Laws Quiz",
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

## Data Models

### Educational Hierarchy Data Models

#### Subject Model
```typescript
{
  id: number;
  name: string;              // Max 100 characters, unique
  description?: string;      // Max 500 characters
  courseCount: number;       // Computed
  courses?: Course[];        // Only in detailed view
}
```

#### Course Model
```typescript
{
  id: number;
  title: string;             // Max 100 characters
  description?: string;      // Max 500 characters
  subjectId: number;
  subjectName: string;       // From Subject
  unitCount: number;         // Computed
  units?: Unit[];            // Only in detailed view
}
```

#### Unit Model
```typescript
{
  id: number;
  title: string;             // Max 100 characters
  orderIndex: number;        // 1-999, unique within course
  courseId: number;
  courseTitle: string;       // From Course
  quizCount: number;         // Computed
  quizzes?: Quiz[];          // Only in detailed view
}
```

### Quiz Data Models

#### Quiz Summary Model
```typescript
{
  id: number;
  title: string;             // Max 200 characters
  description?: string;      // Max 2000 characters
  unitId?: number;           // null for standalone quizzes
  unitTitle?: string;        // From Unit
  orderIndex: number;        // Position within unit
  createdAt: string;         // ISO 8601 datetime
  createdById: string;
  createdByName: string;
  isActive: boolean;
  questionCount: number;     // Computed
  attemptCount: number;      // Computed
}
```

#### Quiz Detail Model
```typescript
{
  id: number;
  title: string;
  description?: string;
  unitId?: number;
  unitTitle?: string;
  orderIndex: number;
  createdAt: string;
  createdById: string;
  createdByName: string;
  isActive: boolean;
  questionCount: number;
  questions: Question[];
}
```

#### Question Model
```typescript
{
  id: number;
  text: string;              // Max 1000 characters
  explanation?: string;      // Max 1000 characters
  imageUrl?: string;
  choices: Choice[];
}
```

#### Choice Model
```typescript
{
  id: number;
  text: string;              // Max 500 characters
  isCorrect: boolean;
}
```

### Quiz Attempt Data Models

#### Quiz Attempt Model
```typescript
{
  id: number;
  quizId: number;
  quizTitle: string;
  userId: string;
  userName: string;
  startedAt: string;         // ISO 8601 datetime
  completedAt?: string;      // ISO 8601 datetime
  score: number;             // 0-100
  isCompleted: boolean;
}
```

#### Quiz Result Model
```typescript
{
  quizAttemptId: number;
  quizTitle: string;
  totalQuestions: number;
  correctAnswers: number;
  incorrectAnswers: number;
  score: number;             // 0-100
  percentage: number;        // 0.0-100.0
  startedAt: string;
  completedAt: string;
  duration: string;          // HH:MM:SS format
  questionResults: QuestionResult[];
}
```

#### Question Result Model
```typescript
{
  questionId: number;
  questionText: string;
  explanation?: string;
  selectedChoiceId: number;
  selectedChoiceText: string;
  correctChoiceId: number;
  correctChoiceText: string;
  isCorrect: boolean;
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
| 403 | Forbidden | Authenticated but not authorized (e.g., not Admin) |
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
GET /api/course?page=2&pageSize=50
```

## CORS Configuration

CORS is configured to allow requests from:
- Development: `http://localhost:3000`, `http://localhost:5173` (React/Vite)
- Production: TBD

Allowed headers: `Content-Type`, `Authorization`

## API Versioning

Current version: v1 (implicit)

Future versions can be implemented via:
- URL path: `/api/v2/quiz`
- Header: `API-Version: 2`
- Query parameter: `/api/quiz?api-version=2`

---

## Frontend Integration Guide

### Authentication Flow

1. **Registration**:
   ```typescript
   const response = await fetch('/api/auth/register', {
     method: 'POST',
     headers: { 'Content-Type': 'application/json' },
     body: JSON.stringify({
       userName: 'johndoe',
       email: 'john@example.com',
       password: 'Password123!',
       confirmPassword: 'Password123!'
     })
   });
   ```

2. **Login**:
   ```typescript
   const response = await fetch('/api/auth/login', {
     method: 'POST',
     headers: { 'Content-Type': 'application/json' },
     body: JSON.stringify({
       email: 'john@example.com',
       password: 'Password123!'
     })
   });
   const { token } = await response.json();
   localStorage.setItem('token', token);
   ```

3. **Authenticated Requests**:
   ```typescript
   const token = localStorage.getItem('token');
   const response = await fetch('/api/quiz', {
     method: 'POST',
     headers: {
       'Content-Type': 'application/json',
       'Authorization': `Bearer ${token}`
     },
     body: JSON.stringify(quizData)
   });
   ```

### Hierarchy Navigation Flow

1. **Browse Subjects**:
   ```typescript
   const subjects = await fetch('/api/subject').then(r => r.json());
   // Display subjects in a grid or list
   ```

2. **View Courses in Subject**:
   ```typescript
   const subjectId = 1;
   const subject = await fetch(`/api/subject/${subjectId}`).then(r => r.json());
   // Display subject.courses
   ```

3. **View Units in Course**:
   ```typescript
   const courseId = 1;
   const course = await fetch(`/api/course/${courseId}`).then(r => r.json());
   // Display course.units ordered by orderIndex
   ```

4. **View Quizzes in Unit**:
   ```typescript
   const unitId = 1;
   const quizzes = await fetch(`/api/unit/${unitId}/quizzes`).then(r => r.json());
   // Display quizzes ordered by orderIndex
   ```

### Breadcrumb Implementation

```typescript
// Fetch full hierarchy for breadcrumbs
async function getBreadcrumbs(quizId: number) {
  const quiz = await fetch(`/api/quiz/${quizId}`).then(r => r.json());
  
  if (!quiz.unitId) {
    return [{ name: 'Home', path: '/' }, { name: quiz.title }];
  }
  
  const unit = await fetch(`/api/unit/${quiz.unitId}`).then(r => r.json());
  const course = await fetch(`/api/course/${unit.courseId}`).then(r => r.json());
  const subject = await fetch(`/api/subject/${course.subjectId}`).then(r => r.json());
  
  return [
    { name: 'Home', path: '/' },
    { name: subject.name, path: `/subjects/${subject.id}` },
    { name: course.title, path: `/courses/${course.id}` },
    { name: unit.title, path: `/units/${unit.id}` },
    { name: quiz.title }
  ];
}
```

### Quiz Taking Flow

1. **Start Quiz**:
   ```typescript
   const response = await fetch('/api/quiz-attempt/start', {
     method: 'POST',
     headers: {
       'Content-Type': 'application/json',
       'Authorization': `Bearer ${token}`
     },
     body: JSON.stringify({ quizId: 1 })
   });
   const attempt = await response.json();
   // Store attempt.id for submission
   ```

2. **Display Quiz Questions**:
   ```typescript
   const quiz = await fetch(`/api/quiz/${quizId}`).then(r => r.json());
   // Render quiz.questions with radio buttons for choices
   ```

3. **Submit Answers**:
   ```typescript
   const answers = [
     { questionId: 1, choiceId: 2 },
     { questionId: 2, choiceId: 5 }
   ];
   
   const result = await fetch('/api/quiz-attempt/submit', {
     method: 'POST',
     headers: {
       'Content-Type': 'application/json',
       'Authorization': `Bearer ${token}`
     },
     body: JSON.stringify({
       quizAttemptId: attemptId,
       answers: answers
     })
   }).then(r => r.json());
   
   // Display result with score and explanations
   ```

### Error Handling

```typescript
async function apiCall(url: string, options: RequestInit = {}) {
  try {
    const response = await fetch(url, options);
    
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Request failed');
    }
    
    return response.json();
  } catch (error) {
    console.error('API Error:', error);
    throw error;
  }
}
```

---

**Last Updated**: January 2025  
**API Version**: 1.0  
**Status**: Production Ready
