# Project Analysis and Implementation Guide

## Current Project State

### Overview
- **Project Type**: ASP.NET Core Web API
- **Framework**: .NET 8.0
- **Current State**: Basic template with Swagger UI and a sample WeatherForecast controller
- **Dependencies**: Swashbuckle.AspNetCore (6.6.2) for API documentation

### Existing Structure
```
tapcet-api/
??? Controllers/
?   ??? WeatherForecastController.cs (template - can be removed)
??? WeatherForecast.cs (template - can be removed)
??? Program.cs
??? tapcet-api.csproj
```

## Implementation Roadmap

### Phase 1: Project Setup and Infrastructure

#### 1.1 Install Required NuGet Packages
```
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
```

#### 1.2 Create Project Folder Structure
```
tapcet-api/
??? Controllers/
?   ??? AuthController.cs
?   ??? QuizController.cs
?   ??? QuestionController.cs
?   ??? ResultController.cs
??? Models/
?   ??? User.cs
?   ??? Quiz.cs
?   ??? Question.cs
?   ??? Choice.cs
?   ??? QuizAttempt.cs
?   ??? UserAnswer.cs
??? DTOs/
?   ??? Auth/
?   ?   ??? RegisterDto.cs
?   ?   ??? LoginDto.cs
?   ?   ??? AuthResponseDto.cs
?   ??? Quiz/
?   ?   ??? CreateQuizDto.cs
?   ?   ??? QuizDto.cs
?   ?   ??? QuestionDto.cs
?   ??? Result/
?       ??? SubmitAnswerDto.cs
?       ??? QuizResultDto.cs
??? Data/
?   ??? ApplicationDbContext.cs
??? Services/
?   ??? Interfaces/
?   ?   ??? IAuthService.cs
?   ?   ??? IQuizService.cs
?   ?   ??? IResultService.cs
?   ??? Implementations/
?       ??? AuthService.cs
?       ??? QuizService.cs
?       ??? ResultService.cs
??? Repositories/
?   ??? Interfaces/
?   ?   ??? IQuizRepository.cs
?   ?   ??? IResultRepository.cs
?   ??? Implementations/
?       ??? QuizRepository.cs
?       ??? ResultRepository.cs
??? Middleware/
    ??? ExceptionMiddleware.cs
```

### Phase 2: Database and Models

#### 2.1 Create Domain Models
- **User**: Extended from IdentityUser with additional properties
- **Quiz**: Contains quiz metadata (title, description, created date)
- **Question**: Belongs to a quiz, contains question text
- **Choice**: Multiple choices per question, one marked as correct
- **QuizAttempt**: Tracks when a user starts/completes a quiz
- **UserAnswer**: Records user's selected choice for each question

#### 2.2 Set Up Database Context
- Configure ApplicationDbContext with Identity
- Define entity relationships (one-to-many, many-to-many)
- Configure cascade delete behaviors
- Add database migrations

#### 2.3 Connection String Configuration
Add to appsettings.json:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TapcetDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### Phase 3: Authentication and Authorization

#### 3.1 JWT Authentication Setup
- Configure JWT settings in appsettings.json
- Add JWT configuration in Program.cs
- Implement token generation logic

#### 3.2 User Management Endpoints
- POST /api/auth/register - User registration
- POST /api/auth/login - User login (returns JWT token)
- POST /api/auth/logout - User logout (optional: token blacklisting)

#### 3.3 Role-Based Authorization
- Define roles: Admin, User
- Protect admin endpoints with [Authorize(Roles = "Admin")]

### Phase 4: Quiz Management (Admin Features)

#### 4.1 Quiz CRUD Operations
- POST /api/quiz - Create new quiz (Admin only)
- GET /api/quiz - Get all quizzes
- GET /api/quiz/{id} - Get specific quiz with questions
- PUT /api/quiz/{id} - Update quiz (Admin only)
- DELETE /api/quiz/{id} - Delete quiz (Admin only)

#### 4.2 Question Management
- POST /api/quiz/{quizId}/question - Add question to quiz
- PUT /api/question/{id} - Update question
- DELETE /api/question/{id} - Delete question

### Phase 5: Quiz Attempt and Submission

#### 5.1 User Quiz Flow
- GET /api/quiz/{id}/start - Start a quiz attempt (creates QuizAttempt record)
- GET /api/quiz/{id}/questions - Get questions without showing correct answers
- POST /api/quiz/attempt/{attemptId}/submit - Submit all answers at once

#### 5.2 Answer Validation
- Validate all submitted answers
- Calculate score (correct answers / total questions)
- Store results in database
- Return score and correct answers to user

### Phase 6: Results and History

#### 6.1 Results Endpoints
- GET /api/result/user/{userId} - Get user's quiz history
- GET /api/result/attempt/{attemptId} - Get detailed results of specific attempt
- GET /api/result/quiz/{quizId}/leaderboard - Get top scores for a quiz

#### 6.2 Statistics
- Track total attempts per user
- Calculate average scores
- Track completion time

### Phase 7: Additional Enhancements

#### 7.1 Error Handling
- Global exception middleware
- Standardized error response format
- Validation error handling

#### 7.2 Logging
- Add logging throughout the application
- Log authentication attempts
- Log quiz submissions

#### 7.3 API Documentation
- Enhance Swagger with XML comments
- Add authorization header to Swagger UI
- Document all DTOs and endpoints

## Database Schema Design

### Users Table
- Id (PK)
- UserName
- Email
- PasswordHash
- CreatedDate

### Quizzes Table
- Id (PK)
- Title
- Description
- CreatedById (FK to Users)
- CreatedDate
- IsActive

### Questions Table
- Id (PK)
- QuizId (FK to Quizzes)
- QuestionText
- Order

### Choices Table
- Id (PK)
- QuestionId (FK to Questions)
- ChoiceText
- IsCorrect

### QuizAttempts Table
- Id (PK)
- UserId (FK to Users)
- QuizId (FK to Quizzes)
- StartTime
- EndTime
- Score
- TotalQuestions

### UserAnswers Table
- Id (PK)
- QuizAttemptId (FK to QuizAttempts)
- QuestionId (FK to Questions)
- SelectedChoiceId (FK to Choices)
- IsCorrect

## Implementation Order

### Week 1: Foundation
1. Install all required packages
2. Create folder structure
3. Create all domain models
4. Set up ApplicationDbContext
5. Configure database connection
6. Create and apply initial migration

### Week 2: Authentication
1. Configure JWT authentication
2. Implement AuthService
3. Create AuthController
4. Test registration and login

### Week 3: Quiz Management
1. Create quiz repositories and services
2. Implement quiz CRUD operations
3. Create QuizController
4. Test quiz creation and retrieval

### Week 4: Quiz Attempts and Results
1. Implement quiz attempt logic
2. Create answer submission endpoint
3. Implement scoring algorithm
4. Create results endpoints
5. Test complete user flow

### Week 5: Polish and Testing
1. Add error handling middleware
2. Enhance API documentation
3. Add logging
4. Test all endpoints
5. Fix bugs and edge cases

## Testing Checklist

### Authentication
- [ ] User can register with valid credentials
- [ ] User cannot register with duplicate email
- [ ] User can login with correct credentials
- [ ] User receives JWT token on login
- [ ] Invalid credentials return proper error

### Quiz Management
- [ ] Admin can create quiz with questions
- [ ] Non-admin cannot create quiz
- [ ] Quiz appears in quiz list
- [ ] Quiz details include all questions and choices

### Quiz Attempt
- [ ] User can start a quiz
- [ ] Questions do not reveal correct answers
- [ ] User can submit answers
- [ ] Score is calculated correctly
- [ ] Results are stored in database

### Results
- [ ] User can view their quiz history
- [ ] Results show correct and incorrect answers
- [ ] Leaderboard shows top scores

## Security Considerations

1. Never expose correct answers before submission
2. Validate that user owns the quiz attempt
3. Prevent multiple submissions for same attempt
4. Use HTTPS in production
5. Implement rate limiting for login attempts
6. Validate all user inputs
7. Use parameterized queries (EF Core handles this)
8. Store passwords securely (Identity handles this)

## Next Steps

1. Start with Phase 1.1 - Install required NuGet packages
2. Create the folder structure as outlined
3. Begin implementing models and database context
4. Run your first migration to create the database
5. Implement authentication before moving to quiz features

## Useful Commands

```bash
# Add migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# Run the application
dotnet run

# Watch mode (auto-restart on changes)
dotnet watch run

# Build project
dotnet build
```

## Resources

- ASP.NET Core Identity: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity
- JWT Authentication: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn
- Entity Framework Core: https://learn.microsoft.com/en-us/ef/core/
- API Best Practices: https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design
