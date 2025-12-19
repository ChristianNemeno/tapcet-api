# Business Rules & Validation

## Overview

This document outlines all business rules, validation constraints, and data integrity requirements implemented in the TAPCET Quiz API.

## User Management

### Registration Rules

**Username**:
- Required
- Minimum length: 3 characters
- Maximum length: 50 characters
- Must be unique
- Alphanumeric characters and underscores only (enforced by Identity)

**Email**:
- Required
- Valid email format
- Must be unique across all users
- Case-insensitive for uniqueness check

**Password**:
- Required
- Minimum length: 6 characters
- Must contain at least one uppercase letter
- Must contain at least one lowercase letter
- Must contain at least one digit
- Special characters optional (not required)
- Maximum length: 100 characters

**Password Confirmation**:
- Must match password exactly

### Login Rules

**Email**:
- Required
- Must be registered

**Password**:
- Required
- Must match stored hash

**Account Lockout**:
- Maximum 5 failed login attempts
- Lockout duration: 5 minutes
- Counter resets on successful login

## Quiz Management

### Quiz Creation Rules

**Title**:
- Required
- Minimum length: 3 characters
- Maximum length: 200 characters
- Whitespace is trimmed
- Cannot be empty or whitespace-only

**Description**:
- Optional
- Maximum length: 2000 characters
- Can be null or empty

**Questions**:
- Required
- Minimum: 1 question
- No maximum (practical limit by database)
- Each question validated independently

**IsActive**:
- Default: true
- Can be toggled after creation

**CreatedBy**:
- Automatically set from authenticated user
- Cannot be changed after creation

### Question Creation Rules

**Text**:
- Required
- Minimum length: 1 character
- Maximum length: 2000 characters
- Cannot be empty or whitespace-only

**Explanation**:
- Optional
- Maximum length: 2000 characters
- Can be null or empty

**ImageUrl**:
- Optional
- Maximum length: 500 characters
- No format validation (accepts any string)
- Future enhancement: Validate URL format

**Choices**:
- Required
- Minimum: 2 choices
- Maximum: 6 choices
- Each choice validated independently

**Correct Answer Requirement**:
- Exactly 1 choice must have `IsCorrect = true`
- Cannot have 0 correct answers
- Cannot have multiple correct answers

### Choice Creation Rules

**Text**:
- Required
- Minimum length: 1 character
- Maximum length: 500 characters
- Cannot be empty or whitespace-only

**IsCorrect**:
- Boolean value
- Default: false
- Exactly one per question must be true

### Quiz Update Rules

**Ownership**:
- Only quiz creator can update
- Admin role bypass: Not currently implemented

**Updatable Fields**:
- Title (same validation as creation)
- Description (same validation as creation)
- IsActive (boolean toggle)

**Non-Updatable Fields**:
- Id (immutable)
- CreatedAt (immutable)
- CreatedById (immutable)
- Questions (use separate endpoint to add questions)

**Update Validation**:
- At least one field must change
- All validation rules from creation apply

### Quiz Deletion Rules

**Ownership**:
- Only quiz creator can delete
- Admin role bypass: Not currently implemented

**Deletion Behavior**:
- Cascade delete: All questions and choices
- Referential integrity: Cannot delete if quiz attempts exist
- Future enhancement: Soft delete instead of hard delete

**Constraints**:
- Cannot delete quiz with completed attempts
- Recommendation: Set IsActive = false instead

### Quiz Status Toggle Rules

**Ownership**:
- Only quiz creator can toggle status

**Toggle Behavior**:
- Switches between true and false
- No validation on content when disabling
- When enabling, quiz should have questions (recommended, not enforced)

**Effect on Attempts**:
- Active: Users can start new attempts
- Inactive: Users cannot start new attempts
- Existing in-progress attempts not affected

## Quiz Attempt Management

### Start Attempt Rules

**Quiz Validation**:
- Quiz must exist
- Quiz must be active (IsActive = true)
- Quiz must have at least 1 question

**User Validation**:
- User must be authenticated
- No limit on attempts per user per quiz

**Attempt Creation**:
- StartedAt: Set to current UTC time
- CompletedAt: null
- Score: 0
- UserId: From authenticated user
- QuizId: From request

**Duplicate Attempts**:
- Allowed: User can attempt same quiz multiple times
- Each attempt tracked separately

### Submit Quiz Rules

**Attempt Validation**:
- Attempt must exist
- User must own the attempt
- Attempt must not be already completed (CompletedAt = null)

**Answer Requirements**:
- Must answer ALL questions in the quiz
- Answer count must match question count
- Each answer must reference valid question and choice
- Duplicate answers for same question: Not allowed

**Answer Validation**:
- QuestionId must belong to the quiz
- ChoiceId must belong to the question
- No null or invalid IDs

**Submission Behavior**:
- CompletedAt: Set to current UTC time
- Score: Calculated as (Correct / Total) * 100, rounded
- UserAnswers: All answers saved to database
- User statistics: Automatically updated

**Re-submission**:
- Not allowed: Cannot submit same attempt twice
- User must start new attempt

**Partial Submission**:
- Not allowed: Must answer all questions
- No partial scoring

### Scoring Algorithm

```
Score = Round((CorrectAnswers / TotalQuestions) * 100)
```

**Examples**:
- 5/10 correct = 50%
- 7/10 correct = 70%
- 9/10 correct = 90%
- 10/10 correct = 100%

**Rounding**: Mathematical rounding (0.5 rounds up)

**Score Range**: 0-100 (integer)

### View Results Rules

**Ownership**:
- User can only view own attempt results
- Admin role bypass: Not currently implemented

**Completion Requirement**:
- Attempt must be completed (CompletedAt != null)
- In-progress attempts: No results available

**Result Content**:
- Overall score and percentage
- Question-by-question breakdown
- Selected vs correct answers
- Explanations for each question

### User Attempt History Rules

**Access**:
- User can only view own attempts
- No pagination (returns all attempts)

**Ordering**:
- Most recent first (StartedAt DESC)

**Content**:
- Both completed and in-progress attempts
- Summary information only
- Detailed results require separate call

### Quiz Attempts List Rules

**Access**:
- Any authenticated user can view
- Shows completed attempts only

**Filtering**:
- By quiz ID
- Completed attempts only (CompletedAt != null)

**Ordering**:
- By score (highest first)
- Tiebreaker: Fastest completion time
- Includes all users' attempts

### Leaderboard Rules

**Access**:
- Any authenticated user can view

**Filtering**:
- By quiz ID
- Completed attempts only

**Ordering**:
- Primary: Score (highest first)
- Secondary: Duration (fastest first)

**Limit**:
- Minimum: 1 result
- Maximum: 100 results
- Default: 10 results

**Ranking Algorithm**:
1. Sort by score descending
2. For ties, sort by (CompletedAt - StartedAt) ascending
3. Take top N results

## User Statistics

### Automatic Updates

User statistics are updated after each quiz submission:

**TotalQuizAttempts**:
- Count of completed attempts
- Incremented on each submission
- Never decremented (even if attempts deleted)

**AverageScore**:
- Average of all completed attempt scores
- Recalculated on each submission
- Formula: Sum(Scores) / Count(Attempts)
- Rounded to 2 decimal places

**Update Trigger**:
- After QuizAttempt.CompletedAt is set
- Performed in same transaction as submission

## Data Integrity Rules

### Referential Integrity

**Quiz ? User (CreatedById)**:
- ON DELETE: RESTRICT
- Cannot delete user with quizzes
- Recommendation: Implement user soft delete

**Question ? Quiz (QuizId)**:
- ON DELETE: CASCADE
- Deleting quiz deletes all questions

**Choice ? Question (QuestionId)**:
- ON DELETE: CASCADE
- Deleting question deletes all choices

**QuizAttempt ? Quiz (QuizId)**:
- ON DELETE: RESTRICT
- Cannot delete quiz with attempts

**QuizAttempt ? User (UserId)**:
- ON DELETE: RESTRICT
- Cannot delete user with attempts

**UserAnswer ? QuizAttempt (QuizAttemptId)**:
- ON DELETE: CASCADE
- Deleting attempt deletes all answers

**UserAnswer ? Question (QuestionId)**:
- ON DELETE: RESTRICT
- Cannot delete question referenced in answer

**UserAnswer ? Choice (ChoiceId)**:
- ON DELETE: RESTRICT
- Cannot delete choice referenced in answer

### Uniqueness Constraints

**Users**:
- Unique: Email (case-insensitive)
- Unique: UserName (case-insensitive)

**Other Entities**:
- No unique constraints beyond primary keys

### Required Fields

**All Entities**:
- Id (primary key, auto-generated)

**User**:
- UserName
- Email
- PasswordHash

**Quiz**:
- Title
- CreatedById
- CreatedAt
- IsActive

**Question**:
- QuizId
- Text

**Choice**:
- QuestionId
- Text
- IsCorrect

**QuizAttempt**:
- QuizId
- UserId
- StartedAt
- Score

**UserAnswer**:
- QuizAttemptId
- QuestionId
- ChoiceId
- AnsweredAt

## Validation Error Messages

### Registration Errors

- "User name is required" (empty username)
- "User name must be between 3 and 50 characters" (length validation)
- "Email is required" (empty email)
- "Invalid email format" (format validation)
- "Email is already registered" (uniqueness)
- "Password is required" (empty password)
- "Password must be at least 6 characters" (minimum length)
- "Password must contain at least one uppercase letter" (uppercase requirement)
- "Password must contain at least one lowercase letter" (lowercase requirement)
- "Password must contain at least one digit" (digit requirement)
- "Passwords do not match" (confirmation mismatch)

### Login Errors

- "Email is required" (empty email)
- "Password is required" (empty password)
- "Invalid email or password" (authentication failure)
- "Account is locked. Please try again later" (lockout)

### Quiz Creation Errors

- "Quiz title is required" (empty title)
- "Title must be between 3 and 200 characters" (length validation)
- "Description cannot exceed 2000 characters" (max length)
- "At least one question is required" (no questions)
- "Question text is required" (empty question)
- "Question text cannot exceed 2000 characters" (max length)
- "Each question must have between 2 and 6 choices" (choice count)
- "Each question must have exactly one correct answer" (correct count)
- "Choice text is required" (empty choice)
- "Choice text cannot exceed 500 characters" (max length)

### Quiz Update Errors

- "Quiz not found" (invalid ID)
- "You don't have permission to update this quiz" (ownership)
- "Title must be between 3 and 200 characters" (validation)

### Quiz Attempt Errors

- "Quiz ID is required" (empty quiz ID)
- "Invalid quiz ID" (negative or zero)
- "Failed to start quiz. Quiz may be inactive, not found, or has no questions" (start validation)
- "Quiz attempt ID is required" (empty attempt ID)
- "Answers are required" (empty answers list)
- "At least one answer is required" (empty answers)
- "Question ID is required" (empty question ID in answer)
- "Choice ID is required" (empty choice ID in answer)
- "Failed to submit quiz. Ensure you've answered all questions and haven't already submitted this attempt" (submission validation)
- "Result not found. The attempt may not be completed, doesn't exist, or you don't have permission to view it" (result access)

### Leaderboard Errors

- "topCount must be between 1 and 100" (range validation)

## Future Enhancements

### Recommended Validations

1. **ImageUrl Format Validation**
   - Validate URL format
   - Check image accessibility
   - Validate image type (jpg, png, etc.)

2. **Quiz Content Validation**
   - Maximum questions per quiz
   - Minimum questions before activation
   - Duplicate question detection

3. **Rate Limiting**
   - Limit quiz creation per user
   - Limit attempts per quiz per day
   - Prevent brute force on login

4. **Advanced Quiz Features**
   - Time limits per quiz
   - Time limits per question
   - Question randomization
   - Choice randomization

5. **User Restrictions**
   - Email verification requirement
   - Age restriction
   - Terms of service acceptance

6. **Content Moderation**
   - Profanity filter
   - Inappropriate content detection
   - Reporting mechanism
