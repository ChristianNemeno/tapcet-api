# Getting Started

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 8.0 SDK** or later
- **PostgreSQL 17** or later
- **Docker** (optional, for containerized PostgreSQL)
- **Git** for version control
- **Visual Studio 2022**, **VS Code**, or **Rider** (recommended IDEs)

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/ChristianNemeno/tapcet-api.git
cd tapcet-api
```

### 2. Database Setup

#### Option A: Using Docker (Recommended)

```bash
docker run --name tapcet-pg \
  -e POSTGRES_USER=tapcet \
  -e POSTGRES_PASSWORD=TapcetDev123 \
  -e POSTGRES_DB=TapcetDb \
  -p 6543:5432 \
  -d postgres:17
```

#### Option B: Local PostgreSQL Installation

1. Install PostgreSQL 17 from https://www.postgresql.org/download/
2. Create a new database named `TapcetDb`
3. Create a user with appropriate permissions

### 3. Configure Connection String

Update `appsettings.json` or `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=6543;Database=TapcetDb;Username=tapcet;Password=TapcetDev123"
  }
}
```

### 4. Configure JWT Settings

Ensure JWT settings are configured in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyHere_MustBe32CharactersOrMore",
    "Issuer": "TapcetAPI",
    "Audience": "TapcetClient",
    "ExpiryInMinutes": 60
  }
}
```

**Important**: For production, use User Secrets or environment variables instead of storing secrets in configuration files.

### 5. Restore Dependencies

```bash
dotnet restore
```

### 6. Apply Database Migrations

```bash
cd tapcet-api
dotnet ef database update
```

This will create all necessary tables and seed initial data (admin user and roles).

### 7. Run the Application

```bash
dotnet run
```

The API will start and be available at:
- HTTPS: https://localhost:7237
- HTTP: http://localhost:5080

### 8. Access Swagger Documentation

Navigate to https://localhost:7237/swagger to view and test the API documentation.

## Default Credentials

The application seeds a default admin account:

- **Email**: admin@tapcet.com
- **Password**: Admin@123
- **Role**: Admin

**Important**: Change this password immediately in production environments.

## Verify Installation

### Test Database Connection

```bash
docker exec -it tapcet-pg psql -U tapcet -d TapcetDb -c "SELECT version();"
```

### Test API Health

```bash
curl https://localhost:7237/api/auth/login -k
```

You should receive a response indicating the endpoint is accessible.

## Common Issues

### Port Already in Use

If port 7237 or 5080 is already in use, modify `launchSettings.json`:

```json
"applicationUrl": "https://localhost:7237;http://localhost:5080"
```

Change to available ports.

### Database Connection Failed

Verify PostgreSQL is running:

```bash
docker ps | grep tapcet-pg
```

If not running, start the container:

```bash
docker start tapcet-pg
```

### Migration Errors

If migrations fail, try:

```bash
dotnet ef database drop --force
dotnet ef database update
```

## Development Tools

### Recommended VS Code Extensions

- C# Dev Kit
- REST Client
- Docker
- PostgreSQL Explorer

### Recommended Visual Studio Extensions

- Entity Framework Power Tools
- Swagger Generator

## Next Steps

- Review the [Architecture](./architecture.md) documentation
- Explore the [API Reference](./api-reference.md)
- Read about [Authentication](./authentication.md)
- Check the [Testing Guide](./testing-guide.md)
