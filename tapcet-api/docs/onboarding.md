# Onboarding Guide

## Goal

This onboarding guide is designed to help a new developer run the TAPCET Quiz API locally, understand the project structure, and contribute changes safely.

## What You Need

- .NET 8 SDK
- PostgreSQL 17 or Docker
- Git
- Visual Studio 2022 / VS Code / Rider

## Step 1: Clone and Restore

```bash
git clone https://github.com/ChristianNemeno/tapcet-api.git
cd tapcet-api
cd tapcet-api

dotnet restore
```

## Step 2: Configure Environment

### Database

Recommended: run PostgreSQL through Docker.

```bash
docker run --name tapcet-pg \
  -e POSTGRES_USER=tapcet \
  -e POSTGRES_PASSWORD=TapcetDev123 \
  -e POSTGRES_DB=TapcetDb \
  -p 6543:5432 \
  -d postgres:17
```

### appsettings

Update `tapcet-api/appsettings.Development.json` with your connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=6543;Database=TapcetDb;Username=tapcet;Password=TapcetDev123"
  }
}
```

### JWT Secret

For local development you can set the secret in `appsettings.Development.json`.
For production, use environment variables.

Example:

```json
{
  "JwtSettings": {
    "SecretKey": "ReplaceThisWithASecretKeyOf32CharsOrMore",
    "Issuer": "TapcetAPI",
    "Audience": "TapcetClient",
    "ExpiryInMinutes": 60
  }
}
```

## Step 3: Apply EF Core Migrations

```bash
cd tapcet-api
dotnet ef database update
```

## Step 4: Run the API

```bash
dotnet run
```

Open Swagger:
- https://localhost:7237/swagger

## Step 5: Verify the Core Flow

1. Register a user
2. Login and copy the JWT token
3. In Swagger, click Authorize and paste `Bearer <token>`
4. Create a quiz with at least one question
5. Start a quiz attempt
6. Submit answers
7. Fetch attempt result

## Project Structure

- `Controllers/`: HTTP endpoints
- `Services/`: business logic
- `DTO/`: request/response models
- `Models/`: EF entities
- `Data/`: `ApplicationDbContext`

## Where to Start Reading Code

1. `Program.cs` (startup, DI, auth)
2. `Controllers/AuthController.cs`
3. `Controllers/QuizController.cs`
4. `Controllers/QuizAttemptController.cs`
5. `Services/Implementations/QuizService.cs`
6. `Services/Implementations/QuizAttemptService.cs`

## Common Development Tasks

### Add an API endpoint

1. Add method to interface in `Services/Interfaces/`
2. Implement it in `Services/Implementations/`
3. Add the controller endpoint in `Controllers/`
4. Update `docs/api-reference.md`

### Add a database field

1. Update EF model in `Models/`
2. Create migration:
   ```bash
   dotnet ef migrations add <Name>
   ```
3. Apply migration:
   ```bash
   dotnet ef database update
   ```

## Troubleshooting

- If Swagger does not load, check the HTTPS port from `Properties/launchSettings.json`.
- If migrations fail, ensure the database connection string points to a running PostgreSQL instance.
- If JWT validation fails, check that `Issuer`, `Audience`, and `SecretKey` match the token generator.
