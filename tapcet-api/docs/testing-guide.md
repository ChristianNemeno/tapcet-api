# Testing Guide

## Overview

This guide provides instructions for testing the TAPCET Quiz API using various methods including Swagger UI, manual testing, and automated testing approaches.

## Testing with Swagger UI

Swagger UI provides an interactive interface for testing API endpoints.

### Accessing Swagger

1. Start the application:
   ```bash
   dotnet run
   ```

2. Navigate to: https://localhost:7237/swagger

3. You should see the Swagger UI with all available endpoints

### Authentication in Swagger

Most endpoints require authentication. Follow these steps:

1. **Register a Test User**
   - Expand `POST /api/auth/register`
   - Click "Try it out"
   - Enter test user data:
     ```json
     {
       "userName": "testuser",
       "email": "test@example.com",
       "password": "Test123!",
       "confirmPassword": "Test123!"
     }
     ```
   - Click "Execute"
   - Verify 200 OK response

2. **Login**
   - Expand `POST /api/auth/login`
   - Click "Try it out"
   - Enter credentials:
     ```json
     {
       "email": "test@example.com",
       "password": "Test123!"
     }
     ```
   - Click "Execute"
   - Copy the `token` from the response

3. **Authorize Swagger**
   - Click the "Authorize" button at the top
   - Enter: `Bearer <paste-your-token-here>`
   - Click "Authorize"
   - Click "Close"

4. **Test Protected Endpoints**
   - All subsequent requests will include the token
   - Token expires after 60 minutes

## Common Use Cases

### Use Case 1: Complete Quiz Workflow

**Objective**: Create a quiz, attempt it, and view results

**Steps**:

1. **Login**
   ```
   POST /api/auth/login
   Body: { "email": "test@example.com", "password": "Test123!" }
   ```

2. **Create Quiz**
   ```
   POST /api/quiz
   Body: {
     "title": "JavaScript Basics",
     "description": "Test your JS knowledge",
     "questions": [
       {
         "text": "What is a closure?",
         "explanation": "A function with access to outer scope",
         "choices": [
           { "text": "A loop", "isCorrect": false },
           { "text": "A function within a function", "isCorrect": true },
           { "text": "A variable", "isCorrect": false }
         ]
       },
       {
         "text": "What is hoisting?",
         "explanation": "Variable declarations are moved to top",
         "choices": [
           { "text": "Variable lifting", "isCorrect": true },
           { "text": "A loop structure", "isCorrect": false }
         ]
       }
     ]
   }
   ```
   Note the `id` from response (e.g., 1)

3. **View Quiz**
   ```
   GET /api/quiz/1
   ```

4. **Start Attempt**
   ```
   POST /api/quiz-attempt/start
   Body: { "quizId": 1 }
   ```
   Note the attempt `id` from response (e.g., 1)

5. **Submit Answers**
   ```
   POST /api/quiz-attempt/submit
   Body: {
     "quizAttemptId": 1,
     "answers": [
       { "questionId": 1, "choiceId": 2 },
       { "questionId": 2, "choiceId": 4 }
     ]
   }
   ```
   Review the score and results

6. **View Results Later**
   ```
   GET /api/quiz-attempt/1/result
   ```

7. **Check History**
   ```
   GET /api/quiz-attempt/user/me
   ```

### Use Case 2: Quiz Management

**Objective**: Create, update, and manage quizzes

**Steps**:

1. **Create Quiz** (as shown in Use Case 1)

2. **Update Quiz**
   ```
   PUT /api/quiz/1
   Body: {
     "title": "JavaScript Advanced",
     "description": "Updated description",
     "isActive": true
   }
   ```

3. **Add Question**
   ```
   POST /api/quiz/1/questions
   Body: {
     "text": "What is async/await?",
     "explanation": "Modern syntax for promises",
     "choices": [
       { "text": "Synchronous code", "isCorrect": false },
       { "text": "Promise syntax sugar", "isCorrect": true },
       { "text": "A loop", "isCorrect": false }
     ]
   }
   ```

4. **Toggle Status**
   ```
   PATCH /api/quiz/1/toggle
   ```

5. **Delete Quiz**
   ```
   DELETE /api/quiz/1
   ```

### Use Case 3: Leaderboard

**Objective**: View top performers for a quiz

**Steps**:

1. **Multiple Users Attempt Same Quiz**
   - User A: 80% score
   - User B: 100% score
   - User C: 90% score

2. **View Leaderboard**
   ```
   GET /api/quiz-attempt/quiz/1/leaderboard?topCount=5
   ```
   
3. **Expected Order**:
   - 1st: User B (100%)
   - 2nd: User C (90%)
   - 3rd: User A (80%)

### Use Case 4: Error Handling

**Test Invalid Scenarios**:

1. **Incomplete Quiz Submission**
   ```
   POST /api/quiz-attempt/submit
   Body: {
     "quizAttemptId": 1,
     "answers": [
       { "questionId": 1, "choiceId": 2 }
       // Missing question 2 answer
     ]
   }
   Expected: 400 Bad Request
   ```

2. **Invalid Quiz ID**
   ```
   GET /api/quiz/99999
   Expected: 404 Not Found
   ```

3. **Unauthorized Access**
   ```
   GET /api/quiz-attempt/user/me
   (No Authorization header)
   Expected: 401 Unauthorized
   ```

4. **Update Others' Quiz**
   ```
   PUT /api/quiz/1
   (As different user who didn't create it)
   Expected: 404 Not Found
   ```

## Manual Testing with cURL

### Register User

```bash
curl -X POST https://localhost:7237/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "testuser",
    "email": "test@example.com",
    "password": "Test123!",
    "confirmPassword": "Test123!"
  }' \
  -k
```

### Login

```bash
curl -X POST https://localhost:7237/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }' \
  -k
```

Save the token from response.

### Create Quiz

```bash
curl -X POST https://localhost:7237/api/quiz \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "title": "Test Quiz",
    "description": "A test quiz",
    "questions": [
      {
        "text": "Question 1",
        "choices": [
          { "text": "Answer A", "isCorrect": true },
          { "text": "Answer B", "isCorrect": false }
        ]
      }
    ]
  }' \
  -k
```

### Get Active Quizzes

```bash
curl -X GET https://localhost:7237/api/quiz/active \
  -H "Content-Type: application/json" \
  -k
```

## Testing with Postman

### Setup

1. **Import Collection**
   Create a new collection named "TAPCET Quiz API"

2. **Set Base URL Variable**
   - Click collection settings
   - Variables tab
   - Add variable: `baseUrl` = `https://localhost:7237`

3. **Add Authentication**
   - Collection settings
   - Authorization tab
   - Type: Bearer Token
   - Token: `{{token}}`

### Create Requests

1. **Register**
   - Method: POST
   - URL: `{{baseUrl}}/api/auth/register`
   - Body: JSON with registration data

2. **Login**
   - Method: POST
   - URL: `{{baseUrl}}/api/auth/login`
   - Body: JSON with login data
   - Tests tab:
     ```javascript
     pm.environment.set("token", pm.response.json().token);
     ```

3. **Create Quiz**
   - Method: POST
   - URL: `{{baseUrl}}/api/quiz`
   - Authorization: Inherit from parent
   - Body: JSON with quiz data

### Test Scripts

**Save Token After Login**:
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

var jsonData = pm.response.json();
pm.environment.set("token", jsonData.token);
```

**Validate Response**:
```javascript
pm.test("Quiz created successfully", function () {
    pm.response.to.have.status(201);
    var jsonData = pm.response.json();
    pm.expect(jsonData.id).to.exist;
    pm.environment.set("quizId", jsonData.id);
});
```

## Unit Testing

### Prerequisites

```bash
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Moq
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

### Test Project Structure

```
tapcet-api.Tests/
??? Controllers/
?   ??? QuizControllerTests.cs
?   ??? QuizAttemptControllerTests.cs
??? Services/
?   ??? AuthServiceTests.cs
?   ??? QuizServiceTests.cs
?   ??? QuizAttemptServiceTests.cs
??? tapcet-api.Tests.csproj
```

### Example: QuizService Tests

```csharp
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using tapcet_api.Data;
using tapcet_api.DTO.Quiz;
using tapcet_api.Services.Implementations;
using Xunit;

namespace tapcet_api.Tests.Services
{
    public class QuizServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<QuizService>> _loggerMock;
        private readonly QuizService _service;

        public QuizServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // Setup AutoMapper
            var config = new MapperConfiguration(cfg => {
                cfg.AddProfile<QuizProfile>();
                cfg.AddProfile<QuestionProfile>();
                cfg.AddProfile<ChoiceProfile>();
            });
            _mapper = config.CreateMapper();

            // Setup logger mock
            _loggerMock = new Mock<ILogger<QuizService>>();

            // Create service
            _service = new QuizService(_context, _mapper, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateQuizAsync_ValidQuiz_ReturnsQuizResponse()
        {
            // Arrange
            var createDto = new CreateQuizDto
            {
                Title = "Test Quiz",
                Description = "Test Description",
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Question 1",
                        Choices = new List<CreateChoiceDto>
                        {
                            new CreateChoiceDto { Text = "A", IsCorrect = true },
                            new CreateChoiceDto { Text = "B", IsCorrect = false }
                        }
                    }
                }
            };

            // Act
            var result = await _service.CreateQuizAsync(createDto, "user123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Quiz", result.Title);
            Assert.Equal(1, result.QuestionCount);
        }

        [Fact]
        public async Task CreateQuizAsync_InvalidQuiz_ReturnsNull()
        {
            // Arrange
            var createDto = new CreateQuizDto
            {
                Title = "Test Quiz",
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Question 1",
                        Choices = new List<CreateChoiceDto>
                        {
                            // No correct answer - invalid
                            new CreateChoiceDto { Text = "A", IsCorrect = false },
                            new CreateChoiceDto { Text = "B", IsCorrect = false }
                        }
                    }
                }
            };

            // Act
            var result = await _service.CreateQuizAsync(createDto, "user123");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetQuizByIdAsync_ExistingQuiz_ReturnsQuiz()
        {
            // Arrange
            var createDto = new CreateQuizDto
            {
                Title = "Test Quiz",
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Text = "Question 1",
                        Choices = new List<CreateChoiceDto>
                        {
                            new CreateChoiceDto { Text = "A", IsCorrect = true },
                            new CreateChoiceDto { Text = "B", IsCorrect = false }
                        }
                    }
                }
            };
            var created = await _service.CreateQuizAsync(createDto, "user123");

            // Act
            var result = await _service.GetQuizByIdAsync(created.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(created.Id, result.Id);
        }

        [Fact]
        public async Task GetQuizByIdAsync_NonExistentQuiz_ReturnsNull()
        {
            // Act
            var result = await _service.GetQuizByIdAsync(99999);

            // Assert
            Assert.Null(result);
        }
    }
}
```

### Running Unit Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run specific test
dotnet test --filter "FullyQualifiedName~QuizServiceTests.CreateQuizAsync_ValidQuiz_ReturnsQuizResponse"

# Generate code coverage report
dotnet test /p:CollectCoverage=true
```

## Integration Testing

### Example: End-to-End Test

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;

namespace tapcet_api.Tests.Integration
{
    public class QuizWorkflowTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public QuizWorkflowTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CompleteQuizWorkflow_Success()
        {
            // 1. Register
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
            {
                userName = "testuser",
                email = "test@example.com",
                password = "Test123!",
                confirmPassword = "Test123!"
            });
            Assert.True(registerResponse.IsSuccessStatusCode);

            // 2. Login
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "test@example.com",
                password = "Test123!"
            });
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Token);

            // 3. Create Quiz
            var createQuizResponse = await _client.PostAsJsonAsync("/api/quiz", new
            {
                title = "Test Quiz",
                questions = new[]
                {
                    new
                    {
                        text = "Question 1",
                        choices = new[]
                        {
                            new { text = "A", isCorrect = true },
                            new { text = "B", isCorrect = false }
                        }
                    }
                }
            });
            var quiz = await createQuizResponse.Content.ReadFromJsonAsync<QuizResponse>();

            // 4. Start Attempt
            var startResponse = await _client.PostAsJsonAsync("/api/quiz-attempt/start", new
            {
                quizId = quiz.Id
            });
            var attempt = await startResponse.Content.ReadFromJsonAsync<AttemptResponse>();

            // 5. Submit
            var submitResponse = await _client.PostAsJsonAsync("/api/quiz-attempt/submit", new
            {
                quizAttemptId = attempt.Id,
                answers = new[]
                {
                    new { questionId = quiz.Questions[0].Id, choiceId = quiz.Questions[0].Choices[0].Id }
                }
            });
            var result = await submitResponse.Content.ReadFromJsonAsync<QuizResult>();

            // Assert
            Assert.Equal(100, result.Score);
        }
    }
}
```

## Performance Testing

### Load Testing with Apache Bench

```bash
# Test login endpoint
ab -n 1000 -c 10 -p login.json -T application/json \
  https://localhost:7237/api/auth/login

# Test quiz listing
ab -n 1000 -c 10 -H "Authorization: Bearer TOKEN" \
  https://localhost:7237/api/quiz
```

### Database Performance

```sql
-- Check query performance
EXPLAIN ANALYZE 
SELECT * FROM "QuizAttempts" 
WHERE "QuizId" = 1 AND "CompletedAt" IS NOT NULL 
ORDER BY "Score" DESC 
LIMIT 10;

-- Check index usage
SELECT * FROM pg_stat_user_indexes 
WHERE schemaname = 'public';
```

## Testing Checklist

### Functional Testing

- [ ] User can register
- [ ] User can login
- [ ] User receives valid JWT token
- [ ] User can create quiz
- [ ] Quiz validates correctly
- [ ] User can view all quizzes
- [ ] User can view quiz details
- [ ] User can update own quiz
- [ ] User cannot update others' quiz
- [ ] User can delete own quiz
- [ ] User cannot delete others' quiz
- [ ] User can toggle quiz status
- [ ] User can add questions
- [ ] User can start quiz attempt
- [ ] Cannot start inactive quiz
- [ ] User can submit complete answers
- [ ] Cannot submit incomplete answers
- [ ] Score calculated correctly
- [ ] User can view attempt results
- [ ] User can view attempt history
- [ ] Leaderboard ranks correctly
- [ ] User statistics update correctly

### Error Handling

- [ ] 400 for invalid input
- [ ] 401 for missing authentication
- [ ] 403 for insufficient permissions
- [ ] 404 for missing resources
- [ ] 500 errors logged properly

### Security Testing

- [ ] Passwords are hashed
- [ ] JWT tokens expire
- [ ] Invalid tokens rejected
- [ ] Users cannot access others' data
- [ ] SQL injection prevented
- [ ] XSS prevented

### Performance Testing

- [ ] Response time < 200ms for simple queries
- [ ] Response time < 1000ms for complex queries
- [ ] Database queries optimized
- [ ] N+1 query problems prevented

## Troubleshooting Tests

### Common Issues

**Database Not Seeded**:
```bash
dotnet ef database drop --force
dotnet ef database update
```

**Port Already in Use**:
Change port in `launchSettings.json`

**SSL Certificate Issues**:
```bash
dotnet dev-certs https --trust
```

**Test Database Conflicts**:
Use unique database names per test:
```csharp
Guid.NewGuid().ToString()
```
