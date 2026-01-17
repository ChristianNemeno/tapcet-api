# Cloud Run Containerization Guide (Beginner)

This guide shows how to package the `tapcet-api` ASP.NET Core (.NET 8) project into a Docker container and run it locally the same way Google Cloud Run will run it.

> Goal: produce a container that listens on the port Cloud Run provides and can be deployed to Cloud Run.

---

## What is “containerizing”?

A **container** is a lightweight package that includes:
- your compiled application
- the .NET runtime it needs
- a consistent environment to run it

With Cloud Run, you deploy **a container image**, and Cloud Run runs it and routes HTTPS traffic to it.

---

## Pre-requisites

Install these on your machine:
- **Docker Desktop** (Windows/macOS) or Docker Engine (Linux)
- Optional but recommended: `.NET 8 SDK`

Verify Docker works:
- `docker version`

---

## Step 1: Add a `Dockerfile`

A `Dockerfile` is a recipe that tells Docker how to build your image.

### Recommended `Dockerfile` (multi-stage build)

Place this file at the repository root (same level as the `tapcet-api` folder): `Dockerfile`

**What it does:**
- Stage 1 (`sdk`) restores + publishes your app
- Stage 2 (`aspnet`) runs the published output on a smaller runtime image

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY tapcet-api/tapcet-api.csproj tapcet-api/
RUN dotnet restore tapcet-api/tapcet-api.csproj

# Copy the rest of the source
COPY tapcet-api/ tapcet-api/

# Publish the app
WORKDIR /src/tapcet-api
RUN dotnet publish tapcet-api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Run as non-root user (recommended on Cloud Run)
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

# Cloud Run provides PORT (usually 8080). Bind Kestrel to that.
ENV PORT=8080
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

EXPOSE 8080

ENTRYPOINT ["dotnet", "tapcet-api.dll"]
```

---

## Step 2: Add a `.dockerignore`

A `.dockerignore` prevents unnecessary files (like `bin/`, `obj/`) from being sent into Docker build context.

Create this at repository root: `.dockerignore`

```gitignore
**/.git
**/.vs
**/bin
**/obj
**/TestResults
**/*.user
**/*.suo
**/*.cache
**/*.log
```

---

## Step 3: Understand Ports on Cloud Run

### Local development vs Cloud Run

- **Locally** you might use: `http://localhost:5080`
- **Cloud Run** sends requests to your container on a port stored in env var `PORT` (defaults to **8080**).

That’s why the Dockerfile sets:
- `ASPNETCORE_URLS=http://0.0.0.0:${PORT}`

**Important**:
- `0.0.0.0` means “listen on all network interfaces” (required inside containers)
- **Don’t** listen only on `localhost` inside a container

---

## Step 4: Build the image locally

From your repo root (where the `Dockerfile` is):

```bash
docker build -t tapcet-api:local .
```

If it succeeds, you have an image named `tapcet-api:local`.

---

## Step 5: Run the container locally (Cloud Run style)

Run it like Cloud Run:

```bash
docker run --rm -e PORT=8080 -p 8080:8080 tapcet-api:local
```

Now try calling an endpoint:
- `http://localhost:8080/swagger` (in Development only)

If Swagger is not shown because you’re running `Production`, that’s normal (your `Program.cs` only enables Swagger in development).

---

## Step 6: Configure settings using environment variables

In Cloud Run, you usually don’t ship real secrets inside `appsettings.json`. Instead you set **environment variables**.

.NET maps environment variables using `__` (double underscore) to nested config.

Examples:
- `ConnectionStrings__DefaultConnection`
- `JwtSettings__SecretKey`

Example local run using env vars:

```bash
docker run --rm -p 8080:8080 \
  -e PORT=8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Host=...;Port=5432;Database=...;Username=...;Password=..." \
  -e JwtSettings__SecretKey="..." \
  -e JwtSettings__Issuer="TapcetAPI" \
  -e JwtSettings__Audience="TapcetClient" \
  tapcet-api:local
```

---

## Step 7: Database note (important)

Cloud Run containers are **stateless**. You should use an external database (example: Cloud SQL for Postgres).

Your app’s DB connection string must point to that external DB.

---

## Step 8: Deploy to Cloud Run (high-level)

Typical steps in GCP:
1. Build and push image to Artifact Registry
2. Deploy that image to Cloud Run
3. Configure env vars (connection string, JWT settings, etc.)

If you tell me whether you use:
- Artifact Registry + Docker push, **or**
- Cloud Build from source

…I can write the exact commands for your setup.

---

## Troubleshooting

### Container starts but you can’t access it
- Make sure the app binds to `0.0.0.0` and the correct `PORT`

### You see DB connection errors
- The container is running, but it can’t reach your database (wrong host/port/firewall)

### Swagger not showing
- Your app only enables Swagger in development. Set:
  - `-e ASPNETCORE_ENVIRONMENT=Development`

---

## Checklist

- [ ] `Dockerfile` exists at repo root
- [ ] `.dockerignore` exists
- [ ] `docker build` succeeds
- [ ] `docker run -p 8080:8080` works
- [ ] App reads config from environment variables
- [ ] External database is accessible from Cloud Run
