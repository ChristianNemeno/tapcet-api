# TAPCET Quiz API - Complete Documentation

## Table of Contents
1. [Project Overview](#project-overview)
2. [Technology Stack](#technology-stack)
3. [Architecture](#architecture)
4. [Getting Started](#getting-started)
5. [Documentation Index](#documentation-index)

---

## Project Overview

**TAPCET Quiz API** is a comprehensive RESTful API built with .NET 8 that provides a complete quiz management and assessment system. The API enables users to create, manage, take quizzes, and track their performance over time.

### Key Features

#### Core Functionalities
- **User Authentication & Authorization**
  - JWT-based authentication
  - Role-based access control (Admin, User)
  - Secure user registration and login
  - Email uniqueness validation

- **Quiz Management**
  - Create quizzes with multiple questions
  - Update quiz metadata (title, description, status)
  - Delete quizzes (with ownership validation)
  - Toggle quiz active/inactive status
  - Add questions to existing quizzes
  - View all quizzes or active quizzes only

- **Question Management**
  - Multiple choice questions with 2-6 options
  - Exactly one correct answer per question
  - Support for question explanations
  - Optional image URLs for visual questions
  - Minimum 5 characters, maximum 100 characters for question text

- **Quiz Attempts & Assessment**
  - Start quiz attempts (only for active quizzes)
  - Submit answers for evaluation
  - Automatic scoring and percentage calculation
  - Detailed results with correct/incorrect answers
  - Question-by-question breakdown
  - Timestamp tracking (start time, completion time, duration)

- **User Statistics & Progress Tracking**
  - Total quiz attempts counter
  - Average score calculation
  - Personal attempt history
  - Quiz-specific attempt history

- **Leaderboard System**
  - Quiz-specific leaderboards
  - Ranking based on:
    1. Score (primary)
    2. Completion time (secondary - faster is better)
  - Configurable top N results

### Business Goals
1. Provide an intuitive platform for knowledge assessment
2. Enable educators to create and manage quizzes efficiently
3. Track learner progress and performance metrics
4. Encourage competitive learning through leaderboards
5. Ensure data security and user privacy

---

## Technology Stack

### Backend Framework
- **.NET 8** - Latest LTS version with C# 12.0
- **ASP.NET Core Web API** - RESTful API framework

### Database
- **PostgreSQL** - Primary relational database
- **Entity Framework Core 8.0** - ORM for data access
- **Npgsql** - PostgreSQL provider for EF Core

### Authentication & Security
- **ASP.NET Core Identity** - User management and authentication
- **JWT (JSON Web Tokens)** - Stateless authentication
- **Microsoft.AspNetCore.Authentication.JwtBearer** - JWT middleware

### Data Mapping & Validation
- **AutoMapper 12.0** - Object-to-object mapping
- **Data Annotations** - Model validation

### API Documentation
- **Swagger/OpenAPI (Swashbuckle)** - Interactive API documentation
- JWT authentication integration in Swagger UI

### Development Tools
- **Entity Framework Core Tools** - Migrations and scaffolding
- **Entity Framework Core Design** - Design-time components

---

## Architecture

### Architectural Pattern
The project follows a **Layered Architecture** with clear separation of concerns:

```
???????????????????????????????????????
?         Controllers Layer           ?  ? HTTP endpoints, request/response handling
???????????????????????????????????????
?         Services Layer              ?  ? Business logic, validation, orchestration
???????????????????????????????????????
?         Data Access Layer           ?  ? Entity Framework DbContext, repositories
???????????????????????????????????????
?         Database (PostgreSQL)       ?  ? Persistent storage
???????????????????????????????????????
```

### Design Patterns Used

1. **Repository Pattern** (via EF Core DbContext)
   - Abstracts data access logic
   - Provides a collection-like interface for entities

2. **Service Pattern**
   - Encapsulates business logic
   - Separates concerns from controllers
   - Interfaces for dependency injection

3. **Dependency Injection**
   - Constructor injection throughout
   - Loose coupling between components
   - Easier testing and maintainability

4. **DTO (Data Transfer Object) Pattern**
   - Separates internal models from API contracts
   - Controls what data is exposed
   - Validation at the boundary

5. **AutoMapper Profiles**
   - Centralized mapping configurations
   - Reduces boilerplate code

### Project Structure

```
tapcet-api/
??? Controllers/          # API endpoints
?   ??? AuthController.cs
?   ??? [Quiz Controllers - To Be Implemented]
??? Services/
?   ??? Interfaces/       # Service contracts
?   ?   ??? IAuthService.cs
?   ?   ??? IQuizService.cs
?   ?   ??? IQuizAttemptService.cs
?   ??? Implementations/  # Service implementations
?       ??? AuthService.cs
?       ??? QuizService.cs
?       ??? QuizAttemptService.cs
??? Models/               # Domain entities
?   ??? User.cs
?   ??? Quiz.cs
?   ??? Question.cs
?   ??? Choice.cs
?   ??? QuizAttempt.cs
?   ??? UserAnswer.cs
??? DTO/                  # Data Transfer Objects
?   ??? Auth/
?   ??? Quiz/
?   ??? Question/
?   ??? Choice/
?   ??? Attempt/
??? Data/                 # Database context
?   ??? ApplicationDbContext.cs
?   ??? ApplicationDbContextFactory.cs
??? Mappings/             # AutoMapper profiles
?   ??? QuizProfile.cs
?   ??? QuestionProfile.cs
?   ??? ChoiceProfile.cs
?   ??? QuizAttemptProfile.cs
??? Migrations/           # EF Core migrations
??? docs/                 # Documentation
??? Program.cs            # Application entry point
```

---

## Getting Started

### Prerequisites
- .NET 8 SDK
- PostgreSQL 12+ database server
- Visual Studio 2022 / VS Code / Rider

### Database Configuration
The application uses PostgreSQL with the following default connection:
- **Host**: 127.0.0.1
- **Port**: 6543
- **Database**: TapcetDb
- **Username**: tapcet
- **Password**: TapcetDev123

Update `appsettings.json` to match your database configuration.

### Initial Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/ChristianNemeno/tapcet-api
   cd tapcet-api
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update database connection**
   Edit `appsettings.json` ConnectionStrings section if needed

4. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI**
   Navigate to: `https://localhost:<port>/swagger`

### Default Admin Account
On first run, an admin account is automatically created:
- **Email**: admin@tapcet.com
- **Username**: admin
- **Password**: Admin@123
- **Role**: Admin

---

## Documentation Index

Detailed documentation is organized into separate files:

1. **[Entity Relationship Documentation](./EntityRelationship.md)**
   - Complete database schema
   - Entity relationships and constraints
   - Foreign key relationships
   - Cascade delete behaviors

2. **[Business Rules & Validation](./BusinessRules.md)**
   - Functional requirements
   - Business logic rules
   - Validation constraints
   - Security policies

3. **[API Endpoints Guide](./APIEndpoints.md)**
   - Complete endpoint reference
   - Request/response examples
   - Authentication requirements
   - Status codes and error handling

4. **[Service Layer Documentation](./ServiceLayer.md)**
   - Service responsibilities
   - Method documentation
   - Business logic flow
   - Error handling patterns

5. **[DTO Reference](./DTOReference.md)**
   - Complete DTO definitions
   - Validation rules
   - Usage examples

6. **[Implementation Guide](./ImplementationGuide.md)**
   - Step-by-step implementation instructions
   - Missing controllers to implement
   - Testing strategies
   - Deployment considerations

7. **[Future Enhancements](./FutureEnhancements.md)**
   - Planned features
   - Scalability considerations
   - Performance optimizations
   - Advanced features roadmap

---

## Quick Links

- **GitHub Repository**: https://github.com/ChristianNemeno/tapcet-api
- **Technology**: .NET 8, C# 12.0
- **Database**: PostgreSQL
- **Authentication**: JWT Bearer Tokens

---

## Support & Contribution

This project is under active development. For questions, issues, or contributions, please refer to the GitHub repository.

---

**Last Updated**: 2024
**Version**: 1.0.0 (In Development)
