# Business Rules & Validation Documentation

## Overview

This document outlines all business rules, validation constraints, and functional requirements for the TAPCET Quiz API. These rules ensure data integrity, security, and proper application behavior.

---

## 1. Authentication & Authorization

### 1.1 User Registration

**Business Rules:**
- Email addresses must be unique across all users
- Usernames must be unique
- All new users are assigned the "User" role by default
- User accounts are created with UTC timestamp

**Password Requirements:**
- Minimum length: 6 characters
- Must contain at least one digit (0-9)
- Must contain at least one lowercase letter (a-z)
- Must contain at least one uppercase letter (A-Z)
- Non-alphanumeric characters are optional

**Validation Rules:**
```csharp
// RegisterDto validations
- Email: Required, valid email format
- Username: Required, 3-20 characters
- Password: Required, meets password policy
```

**Business Logic:**
1. Check if email already exists
2. If exists, reject registration
3. Create user with hashed password
4. Assign "User" role
5. Generate JWT token
6. Return authentication response

**Error Scenarios:**
- Email already registered ? 400 Bad Request
- Invalid email format ? 400 Bad Request
- Weak password ? 400 Bad Request
- Database error ? Return null (logged)

---

### 1.2 User Login

**Business Rules:**
- Users can log in with email and password
- Failed login attempts are tracked (5 max before lockout)
- Lockout duration: 5 minutes
- JWT tokens expire after 60 minutes

**Validation Rules:**
```csharp
// LoginDto validations
- Email: Required, valid email format
- Password: Required
```

**Business Logic:**
1. Find user by email
2. If not found, reject login
3. Verify password hash
4. If invalid, reject login
5. Get user roles
6. Generate JWT token with claims
7. Return authentication response

**JWT Token Claims:**
- NameIdentifier: User ID
- Name: Username
- Email: User email
- Role: All user roles
- Jti: Unique token identifier

**Error Scenarios:**
- User not found ? 401 Unauthorized
- Invalid password ? 401 Unauthorized
- Account locked ? 401 Unauthorized
- Database error ? Return null (logged)

---

### 1.3 Role-Based Access Control

**Roles:**
- **Admin**: Full system access
  - Can create/update/delete any quiz
  - Can view all attempts
  - System administration
  
- **User**: Standard user access
  - Can create own quizzes
  - Can update/delete own quizzes
  - Can attempt any active quiz
  - Can view own attempts

**Authorization Rules:**
- Quiz creation: Authenticated users (any role)
- Quiz update: Owner or Admin
- Quiz delete: Owner or Admin
- Quiz toggle status: Owner or Admin
- Start quiz attempt: Authenticated users, quiz must be active
- Submit quiz: Owner of attempt
- View attempt results: Owner of attempt or Admin

---

## 2. Quiz Management

### 2.1 Quiz Creation

**Business Rules:**
- Quiz must have at least 1 question
- Each question must have 2-6 choices
- Each question must have exactly 1 correct answer
- Quiz creator is automatically set to authenticated user
- New quizzes are active by default
- Creation timestamp is set to UTC now

**Validation Rules:**
```csharp
// CreateQuizDto
- Title: Required, 3-200 characters
- Description: Optional, max 2000 characters
- Questions: Required, minimum 1 question

// CreateQuestionDto
- Text: Required, 5-100 characters
- Explanation: Optional, max 300 characters
- ImageUrl: Optional, must be valid URL format
- Choices: Required, 2-6 choices minimum

// CreateChoiceDto
- Text: Required, max 500 characters
- IsCorrect: Required boolean
```

**Business Logic:**
1. Validate quiz has questions
2. For each question:
   - Validate has 2-6 choices
   - Count correct choices
   - If not exactly 1, reject
3. Map DTO to entities
4. Set CreatedById to current user
5. Set CreatedAt to now
6. Set IsActive to true
7. Save to database
8. Return quiz with full details

**Error Scenarios:**
- No questions ? Return null, log warning
- Too few choices (< 2) ? Return null, log warning
- Too many choices (> 6) ? Validation error
- No correct answer ? Return null, log warning
- Multiple correct answers ? Return null, log warning
- Database error ? Return null, log error

---

### 2.2 Quiz Updates

**Business Rules:**
- Only quiz creator or admin can update quiz
- Can update: Title, Description, IsActive
- Cannot update: Questions (use separate endpoint)
- Cannot update: CreatedAt, CreatedById
- Updates don't affect existing attempts

**Validation Rules:**
```csharp
// UpdateQuizDto
- Title: Required, 3-200 characters
- Description: Optional, max 2000 characters
- IsActive: Required boolean
```

**Business Logic:**
1. Find quiz by ID
2. Verify quiz exists
3. Verify user is owner or admin
4. Update allowed properties
5. Save changes
6. Return updated quiz

**Error Scenarios:**
- Quiz not found ? Return null, log warning
- User not authorized ? Return null, log warning
- Validation error ? Return null
- Database error ? Return null, log error

---

### 2.3 Quiz Deletion

**Business Rules:**
- Only quiz creator or admin can delete quiz
- Deletion is permanent (hard delete)
- Existing attempts are preserved (restrict delete behavior)
- If attempts exist, deletion may fail due to FK constraint

**Business Logic:**
1. Find quiz by ID
2. Verify quiz exists
3. Verify user is owner or admin
4. Delete quiz from database
5. Return success/failure

**Error Scenarios:**
- Quiz not found ? Return false, log warning
- User not authorized ? Return false, log warning
- Has attempts ? Database FK constraint error
- Database error ? Return false, log error

**Recommendation:**
Consider implementing soft delete (IsDeleted flag) instead of hard delete to preserve data integrity.

---

### 2.4 Toggle Quiz Status

**Business Rules:**
- Only quiz creator or admin can toggle status
- Toggling IsActive to false prevents new attempts
- Existing attempts can still be completed
- Users can still view inactive quizzes (but not attempt them)

**Business Logic:**
1. Find quiz by ID
2. Verify quiz exists
3. Verify user is owner or admin
4. Toggle IsActive (true ? false)
5. Save changes
6. Return success/failure

---

### 2.5 Add Question to Quiz

**Business Rules:**
- Only quiz creator can add questions
- Same validation as quiz creation for questions
- Can add questions to quiz even if attempts exist
- New questions won't affect existing attempts

**Validation Rules:**
Same as CreateQuestionDto in quiz creation

**Business Logic:**
1. Find quiz by ID
2. Verify quiz exists
3. Verify user is owner
4. Validate question (2-6 choices, 1 correct)
5. Add question to quiz
6. Save changes
7. Return updated quiz

---

## 3. Quiz Attempts

### 3.1 Starting a Quiz Attempt

**Business Rules:**
- Quiz must exist and be active
- Quiz must have at least 1 question
- User can have multiple attempts at same quiz
- Attempt is created with Score = 0
- StartedAt is set to current UTC time
- CompletedAt is null until submitted

**Business Logic:**
1. Find quiz by ID
2. Verify quiz exists
3. Verify quiz is active
4. Verify quiz has questions
5. Create new QuizAttempt
   - QuizId = quiz ID
   - UserId = current user
   - StartedAt = now
   - Score = 0
   - CompletedAt = null
6. Save to database
7. Return attempt details

**Error Scenarios:**
- Quiz not found ? Return null, log warning
- Quiz inactive ? Return null, log warning
- Quiz has no questions ? Return null, log warning
- Database error ? Return null, log error

---

### 3.2 Submitting Quiz Answers

**Business Rules:**
- User must own the attempt
- Attempt must not be already completed
- Must provide answer for ALL questions
- Answers are validated against quiz questions
- Each question can only be answered once
- Score is calculated automatically
- CompletedAt is set to current UTC time
- User statistics are updated automatically

**Validation Rules:**
```csharp
// SubmitQuizDto
- QuizAttemptId: Required
- Answers: Required, minimum 1

// SubmitAnswerDto
- QuestionId: Required
- ChoiceId: Required
```

**Scoring Algorithm:**
```csharp
correctAnswers = 0
foreach (answer in answers) {
    if (selectedChoice.IsCorrect) {
        correctAnswers++
    }
}
score = Round((correctAnswers / totalQuestions) * 100)
```

**Business Logic:**
1. Find quiz attempt by ID with all related data
2. Verify attempt exists
3. Verify user owns attempt
4. Verify attempt not already completed
5. Verify answer count matches question count
6. For each answer:
   - Find corresponding question
   - Find selected choice
   - Find correct choice
   - Check if answer is correct
   - Create UserAnswer record
   - Add to results
7. Calculate score
8. Update attempt:
   - Score = calculated score
   - CompletedAt = now
9. Save all UserAnswers
10. Update user statistics
11. Return detailed results

**User Statistics Update:**
1. Get all completed attempts for user
2. Count total attempts
3. Calculate average score
4. Update User.TotalQuizAttempts
5. Update User.AverageScore
6. Save changes

**Error Scenarios:**
- Attempt not found ? Return null, log warning
- User doesn't own attempt ? Return null, log warning
- Attempt already completed ? Return null, log warning
- Answer count mismatch ? Return null, log warning
- Invalid question ID ? Skip, log warning, continue
- Invalid choice ID ? Skip, log warning, continue
- Database error ? Return null, log error

---

### 3.3 Viewing Attempt Results

**Business Rules:**
- User must own the attempt (or be admin)
- Attempt must be completed
- Results show:
  - Overall score and percentage
  - Total/correct/incorrect counts
  - Time taken
  - Question-by-question breakdown
  - Selected answer vs correct answer
  - Explanations for each question

**Business Logic:**
1. Find attempt by ID with all related data
2. Verify attempt exists
3. Verify user owns attempt
4. Verify attempt is completed
5. Build result DTO:
   - Overall statistics
   - Question results with answers
6. Return results

**Error Scenarios:**
- Attempt not found ? Return null, log warning
- User doesn't own attempt ? Return null, log warning
- Attempt not completed ? Return null, log warning
- Database error ? Return null, log error

---

### 3.4 User Attempt History

**Business Rules:**
- Users can view all their own attempts
- Includes both completed and in-progress attempts
- Ordered by most recent first
- Shows basic attempt info (not detailed results)

**Business Logic:**
1. Query all attempts for user
2. Order by StartedAt descending
3. Map to response DTOs
4. Return list

---

### 3.5 Quiz Attempt History

**Business Rules:**
- Shows all completed attempts for a specific quiz
- Ordered by score (highest first)
- Secondary sort by completion time (fastest first)
- Does not include in-progress attempts

**Business Logic:**
1. Query all attempts for quiz
2. Filter: CompletedAt is not null
3. Order by:
   - Score descending
   - Duration (CompletedAt - StartedAt) ascending
4. Map to response DTOs
5. Return list

---

## 4. Leaderboard System

### 4.1 Quiz Leaderboard

**Business Rules:**
- Shows top N performers for a quiz (default 10)
- Only includes completed attempts
- Ranking criteria:
  1. Score (higher is better)
  2. Completion time (faster is better)
- If user has multiple attempts, all are shown

**Ranking Algorithm:**
```csharp
ORDER BY 
  Score DESC,
  (CompletedAt - StartedAt) ASC
LIMIT topCount
```

**Business Logic:**
1. Query all attempts for quiz
2. Filter: CompletedAt is not null
3. Order by score DESC, then duration ASC
4. Take top N (default 10)
5. Include user information
6. Map to response DTOs
7. Return list

**Customization:**
- topCount parameter (default = 10)
- Can be 1-100 (validation in controller)

---

## 5. Data Validation Summary

### Field-Level Validation

| Field | Type | Required | Min | Max | Additional Rules |
|-------|------|----------|-----|-----|-----------------|
| User.Email | string | Yes | - | 256 | Valid email format, unique |
| User.UserName | string | Yes | 3 | 20 | Unique |
| User.Password | string | Yes | 6 | - | Password policy |
| Quiz.Title | string | Yes | 3 | 200 | - |
| Quiz.Description | string | No | - | 2000 | - |
| Question.Text | string | Yes | 5 | 100 | - |
| Question.Explanation | string | No | - | 300 | - |
| Question.ImageUrl | string | No | - | - | Valid URL |
| Choice.Text | string | Yes | 1 | 500 | - |
| Question.Choices | List | Yes | 2 | 6 | Exactly 1 must be correct |

### Entity-Level Validation

**Quiz:**
- Must have at least 1 question
- All questions must be valid

**Question:**
- Must have 2-6 choices
- Exactly 1 choice must be correct

**QuizAttempt:**
- Quiz must exist
- Quiz must be active (for starting)
- Must not be already completed (for submitting)
- Answer count must match question count

---

## 6. Security & Privacy

### 6.1 Authentication Security

**Implemented:**
- Password hashing (ASP.NET Identity default)
- JWT token-based authentication
- Token expiration (60 minutes)
- HTTPS enforcement
- Account lockout after 5 failed attempts

**Recommendations:**
- Implement refresh tokens
- Add rate limiting
- Implement CORS policy
- Add request validation middleware
- Log all authentication attempts

---

### 6.2 Authorization Security

**Implemented:**
- Role-based access control
- Ownership verification for quiz operations
- User-specific data isolation

**Rules:**
- Users can only modify own quizzes
- Users can only view own attempts
- Admins have full access
- All endpoints require authentication (except login/register)

---

### 6.3 Data Privacy

**Rules:**
- Passwords are never returned in responses
- Sensitive user data is not exposed in DTOs
- User emails are protected
- Attempt results are private to user

**Recommendations:**
- Implement data encryption at rest
- Add audit logging
- Implement GDPR compliance features
- Add data export functionality

---

## 7. Error Handling Strategy

### Service Layer Error Handling

**Pattern:**
```csharp
try {
    // Business logic
    return result;
}
catch (Exception ex) {
    _logger.LogError(ex, "Error message with context");
    return null; // or appropriate default
}
```

**Logging Levels:**
- **Warning**: Business rule violations, not found scenarios
- **Error**: Exceptions, database errors, unexpected failures
- **Information**: Successful operations, state changes

### Controller Layer Error Handling

**Pattern:**
```csharp
var result = await _service.MethodAsync();
if (result == null) {
    return BadRequest/NotFound/Unauthorized(...);
}
return Ok(result);
```

**HTTP Status Codes:**
- 200 OK: Successful operation
- 201 Created: Resource created
- 400 Bad Request: Validation error, business rule violation
- 401 Unauthorized: Authentication failed
- 403 Forbidden: Authorization failed (not implemented yet)
- 404 Not Found: Resource not found
- 500 Internal Server Error: Unexpected server error

---

## 8. Performance Considerations

### Database Queries

**Optimization Rules:**
- Always use `.Include()` for needed navigation properties
- Use `.AsNoTracking()` for read-only queries
- Project to DTOs to limit data transfer
- Use indexes on frequently queried fields

**N+1 Query Prevention:**
- Use `.ThenInclude()` for nested relationships
- Avoid lazy loading in loops
- Prefer eager loading for known relationships

### Caching Opportunities

**Recommended for:**
- Active quizzes list
- Quiz leaderboards (cache for 5 minutes)
- User statistics (invalidate on attempt completion)

---

## 9. Business Logic Flows

### Complete Quiz Flow

```
1. User Registration/Login
   ?
2. User Browses Active Quizzes
   ?
3. User Starts Quiz Attempt
   - Creates QuizAttempt record
   - Records StartedAt timestamp
   ?
4. User Answers Questions
   - Frontend manages this
   ?
5. User Submits Answers
   - Validates all questions answered
   - Creates UserAnswer records
   - Calculates score
   - Sets CompletedAt
   - Updates user statistics
   ?
6. User Views Results
   - Shows score, percentage
   - Shows correct/incorrect breakdown
   - Shows detailed answers
   ?
7. User Appears on Leaderboard
   (if score is in top N)
```

### Quiz Creation Flow

```
1. Admin/User Creates Quiz
   - Provides title, description
   - Adds questions with choices
   ?
2. Validation
   - Each question has 2-6 choices
   - Each question has 1 correct answer
   ?
3. Save to Database
   - Creates Quiz entity
   - Creates Question entities
   - Creates Choice entities
   ?
4. Quiz is Active
   - Available for attempts
```

---

## 10. Future Business Rules

### Planned Features

1. **Quiz Categories/Tags**
   - Organize quizzes by topic
   - Filter by category

2. **Time Limits**
   - Quiz.TimeLimitMinutes field
   - Automatically submit when time expires

3. **Question Shuffling**
   - Randomize question order
   - Randomize choice order

4. **Difficulty Levels**
   - Easy, Medium, Hard
   - Filter by difficulty

5. **Passing Score**
   - Quiz.PassingScore field
   - Pass/Fail status on attempts

6. **Retake Policies**
   - Limit number of attempts
   - Cooldown period between attempts

7. **Question Types**
   - Multiple choice (current)
   - True/False
   - Multiple select
   - Fill in the blank

8. **Quiz Sharing**
   - Public vs Private quizzes
   - Share links
   - Embed codes

9. **Analytics**
   - Question difficulty analysis
   - Common wrong answers
   - Completion rates

10. **Gamification**
    - Achievements/Badges
    - Points system
    - Streak tracking

---

**Last Updated**: 2024
**Version**: 1.0.0 (In Development)
