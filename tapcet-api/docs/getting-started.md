# Getting Started

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- PostgreSQL (local install or Docker)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

---

## Step 1 — Start PostgreSQL

The API expects PostgreSQL at `127.0.0.1:6543` by default.

**Docker (fastest):**
```bash
docker run -d \
  --name tapcet-pg \
  -e POSTGRES_USER=tapcet \
  -e POSTGRES_PASSWORD=TapcetDev123 \
  -e POSTGRES_DB=TapcetDb \
  -p 6543:5432 \
  postgres:16
```

**Local install:** Create a database `TapcetDb` with user `tapcet` / password `TapcetDev123` on port `6543`. (Or change the port to `5432` and update `appsettings.json`.)

---

## Step 2 — Run Migrations

Navigate to the project directory:
```bash
cd tapcet-api/tapcet-api
dotnet ef database update
```

This creates all tables. On first run the app will also seed default roles and the admin user.

---

## Step 3 — Run the API

```bash
dotnet run
```

**Swagger UI:** `https://localhost:7237/swagger`

---

## Step 4 — Verify

Call the login endpoint with the seeded admin account:

```http
POST https://localhost:7237/api/auth/login
Content-Type: application/json

{
  "email": "admin@tapcet.com",
  "password": "Admin@123"
}
```

A successful response returns a JWT token. Copy it and use it as `Bearer <token>` in the `Authorization` header for all protected endpoints.

---

## Common Issues

| Problem | Fix |
|---------|-----|
| `connection refused` on startup | PostgreSQL not running, or wrong port in `appsettings.json` |
| `No migrations found` | Run `dotnet ef database update` from the `tapcet-api/tapcet-api` directory |
| SSL certificate warning in browser | Run `dotnet dev-certs https --trust` |
| Port 7237 already in use | Edit `applicationUrl` in `Properties/launchSettings.json` |

---

## Configuration Reference

All config lives in `appsettings.json`. Override any value with an environment variable using `__` as the separator (e.g. `JwtSettings__SecretKey`).

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=127.0.0.1;Port=6543;Database=TapcetDb;Username=tapcet;Password=TapcetDev123"
  },
  "JwtSettings": {
    "SecretKey": "169c46d773c63b1b01c4dad48a97d38a",
    "Issuer": "TapcetAPI",
    "Audience": "TapcetClient",
    "ExpiryInMinutes": 60
  },
  "RateLimiting": {
    "PublicEndpoints":       { "PermitLimit": 50,  "Window": 60 },
    "AuthenticatedEndpoints":{ "PermitLimit": 150, "Window": 60 },
    "AuthEndpoints":         { "PermitLimit": 10,  "Window": 60 }
  }
}
```
