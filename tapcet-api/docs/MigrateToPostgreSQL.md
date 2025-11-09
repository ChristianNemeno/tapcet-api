# Migration Guide: SQL Server to PostgreSQL

## Overview
This guide will help you migrate your Tapcet Quiz API from SQL Server to PostgreSQL. PostgreSQL is open-source, works well with .NET 8, and has excellent free hosting options (Railway, Render, Supabase, Neon, etc.).

## Why PostgreSQL?
- ? **Free hosting options** (Railway, Render, Supabase, Neon, ElephantSQL)
- ? **Open-source** - no licensing concerns
- ? **Excellent .NET support** via Npgsql
- ? **Strong data integrity** and performance
- ? **Works seamlessly with EF Core**
- ? **Lower resource usage** than SQL Server

---

## Prerequisites
- Existing SQL Server setup working
- Docker Desktop installed
- Basic understanding of database migrations

---

## Step-by-Step Migration

### Step 1: Install PostgreSQL NuGet Package

First, remove SQL Server provider and add PostgreSQL:

```powershell
cd tapcet-api

# Remove SQL Server package
dotnet remove package Microsoft.EntityFrameworkCore.SqlServer

# Add PostgreSQL package (compatible with .NET 8 and EF Core 8.0.11)
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.11
```

**Why version 8.0.11?**
- Matches your existing EF Core packages (8.0.11)
- Compatible with .NET 8
- Stable and well-tested

---

### Step 2: Update Program.cs

Update the database configuration to use PostgreSQL:

**File: tapcet-api/Program.cs**

Find this line:
```csharp
// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Replace with:**
```csharp
// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Add using statement at the top:**
```csharp
// Remove this (if exists)
// using Microsoft.EntityFrameworkCore.SqlServer;

// SQL Server uses built-in, PostgreSQL needs explicit using
// No additional using needed - UseNpgsql is available after installing the package
```

---

### Step 3: Update Connection String

Update your `appsettings.json` with PostgreSQL connection string:

**File: tapcet-api/appsettings.json**

**Replace:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=TapcetDb;User Id=sa;Password=Jimboylo1!;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**With:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=TapcetDb;Username=postgres;Password=YourStrong!Password"
  },
  "JwtSettings": {
    "SecretKey": "169c46d773c63b1b01c4dad48a97d38a",
    "Issuer": "TapcetAPI",
    "Audience": "TapcetClient",
    "ExpiryInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Connection String Breakdown:**
- `Host=localhost` - Database server address
- `Port=5432` - PostgreSQL default port
- `Database=TapcetDb` - Database name (will be created automatically)
- `Username=postgres` - Default admin user
- `Password=YourStrong!Password` - Your chosen password

---

### Step 4: Update ApplicationDbContextFactory

Update the design-time factory to use PostgreSQL:

**File: tapcet-api/Data/ApplicationDbContextFactory.cs**

**Replace:**
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace tapcet_api.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Create DbContextOptions
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
```

**With:**
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace tapcet_api.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Create DbContextOptions
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
```

---

### Step 5: Set Up PostgreSQL in Docker

Stop your SQL Server container and start PostgreSQL:

**Stop SQL Server:**
```powershell
docker stop sqlserver2022
```

**Start PostgreSQL:**
```powershell
docker run --name postgres-tapcet -e POSTGRES_PASSWORD=YourStrong!Password -e POSTGRES_DB=TapcetDb -p 5432:5432 --restart unless-stopped -d postgres:16
```

**Command Breakdown:**
- `--name postgres-tapcet` - Container name
- `-e POSTGRES_PASSWORD=YourStrong!Password` - Set admin password
- `-e POSTGRES_DB=TapcetDb` - Create database automatically
- `-p 5432:5432` - Map PostgreSQL port
- `--restart unless-stopped` - Auto-restart on system reboot
- `-d postgres:16` - Use PostgreSQL 16 (latest stable)

**Verify PostgreSQL is Running:**
```powershell
docker ps
```

You should see `postgres-tapcet` running.

**Check Logs:**
```powershell
docker logs postgres-tapcet
```

Look for: `database system is ready to accept connections`

---

### Step 6: Remove Old Migrations and Create New Ones

Since you're switching databases, you need to recreate migrations:

```powershell
cd tapcet-api

# Remove old SQL Server migrations
dotnet ef migrations remove

# If above fails due to multiple migrations, delete the Migrations folder manually
# Then create new PostgreSQL migration
dotnet ef migrations add InitialCreate

# Apply migration to PostgreSQL
dotnet ef database update
```

**What this does:**
- Removes SQL Server-specific migration code
- Creates new PostgreSQL-compatible migrations
- Creates all Identity tables in PostgreSQL
- Sets up your custom User table

---

### Step 7: Build and Test

Build your project to ensure everything compiles:

```powershell
dotnet build
```

**Expected output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

If you get errors, check:
- All `using Microsoft.EntityFrameworkCore.SqlServer` statements are removed
- `Npgsql.EntityFrameworkCore.PostgreSQL` package is installed
- Connection string is correct

---

### Step 8: Run and Test Application

Start your API:

```powershell
dotnet run
```

Navigate to Swagger UI (check terminal for the exact URL, typically `https://localhost:5001/swagger`)

**Test Registration:**
```json
{
  "userName": "testuser",
  "email": "test@example.com",
  "password": "Test@123",
  "confirmPassword": "Test@123"
}
```

**Test Login:**
```json
{
  "email": "test@example.com",
  "password": "Test@123"
}
```

**Test Admin Login:**
```json
{
  "email": "admin@tapcet.com",
  "password": "Admin@123"
}
```

---

### Step 9: Verify Database with pgAdmin (Optional)

**Install pgAdmin 4:**
- Download: https://www.pgadmin.org/download/
- Install and open pgAdmin

**Connect to PostgreSQL:**
1. Right-click **Servers** ? **Register** ? **Server**
2. **General tab:**
   - Name: `Local PostgreSQL Docker`
3. **Connection tab:**
   - Host: `localhost`
   - Port: `5432`
   - Maintenance database: `postgres`
   - Username: `postgres`
   - Password: `YourStrong!Password`
4. Click **Save**

**Browse Your Database:**
- Expand: **Servers** ? **Local PostgreSQL Docker** ? **Databases** ? **TapcetDb** ? **Schemas** ? **public** ? **Tables**

You should see all ASP.NET Identity tables.

**Run Query:**
```sql
SELECT * FROM "AspNetUsers";
SELECT * FROM "AspNetRoles";
```

**Note:** PostgreSQL is case-sensitive for table names. Use double quotes.

---

## Managing PostgreSQL Container

### Essential Commands

```powershell
# Start PostgreSQL
docker start postgres-tapcet

# Stop PostgreSQL
docker stop postgres-tapcet

# Restart PostgreSQL
docker restart postgres-tapcet

# View logs
docker logs postgres-tapcet

# Follow logs in real-time
docker logs -f postgres-tapcet

# Check status
docker ps

# Connect to PostgreSQL CLI
docker exec -it postgres-tapcet psql -U postgres -d TapcetDb
```

### PostgreSQL CLI Commands (if you connect via psql)

```sql
-- List databases
\l

-- Connect to database
\c TapcetDb

-- List tables
\dt

-- Describe table
\d "AspNetUsers"

-- Query data
SELECT * FROM "AspNetUsers";

-- Exit
\q
```

---

## Persistence with Docker Volumes

To preserve data when you remove the container:

```powershell
# Create volume
docker volume create postgres-data

# Remove old container
docker stop postgres-tapcet
docker rm postgres-tapcet

# Create new container with volume
docker run --name postgres-tapcet -e POSTGRES_PASSWORD=YourStrong!Password -e POSTGRES_DB=TapcetDb -p 5432:5432 -v postgres-data:/var/lib/postgresql/data --restart unless-stopped -d postgres:16
```

---

## Free PostgreSQL Hosting Options

When you're ready to deploy, consider these **free options**:

### 1. **Supabase** (Recommended)
- **URL:** https://supabase.com
- **Free tier:** 500MB database, 2GB bandwidth/month
- **Features:** Built-in Auth, Storage, Realtime subscriptions
- **Region:** Global (choose closest to your users)

### 2. **Neon**
- **URL:** https://neon.tech
- **Free tier:** 512MB storage, 10GB data transfer
- **Features:** Serverless PostgreSQL, branching
- **Region:** AWS (multiple regions)

### 3. **Railway**
- **URL:** https://railway.app
- **Free tier:** $5 credit/month (usually sufficient for dev)
- **Features:** PostgreSQL + app hosting
- **Easy deployment:** Connect GitHub repo

### 4. **Render**
- **URL:** https://render.com
- **Free tier:** 90 days free PostgreSQL
- **Features:** Full PostgreSQL instance
- **Easy deployment:** Connect GitHub repo

### 5. **ElephantSQL**
- **URL:** https://www.elephantsql.com
- **Free tier:** 20MB database (good for testing)
- **Features:** Managed PostgreSQL
- **Region:** Multiple data centers

---

## Deployment Connection String Example

When you deploy to a hosting provider, your connection string will look like:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-db-host.region.provider.com;Port=5432;Database=your-db-name;Username=your-username;Password=your-password;SSL Mode=Require;"
  }
}
```

**For production, use environment variables instead:**

```csharp
// In Program.cs or Startup
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
```

---

## Comparison: SQL Server vs PostgreSQL

| Feature | SQL Server (Docker) | PostgreSQL (Docker) |
|---------|-------------------|-------------------|
| **Image Size** | ~1.5 GB | ~150 MB |
| **Memory Usage** | ~2 GB+ | ~100-500 MB |
| **Port** | 1433 | 5432 |
| **Free Hosting** | Azure SQL (limited) | Many options |
| **Open Source** | ? No | ? Yes |
| **EF Core Support** | Excellent | Excellent |
| **Identity Support** | Native | Excellent |

---

## Troubleshooting

### Issue 1: "Npgsql.NpgsqlException: 28P01: password authentication failed"

**Solution:**
- Verify password in `appsettings.json` matches Docker container password
- Password is case-sensitive

### Issue 2: "Could not connect to server"

**Solution:**
```powershell
# Check if container is running
docker ps

# If not, start it
docker start postgres-tapcet

# Check logs
docker logs postgres-tapcet
```

### Issue 3: "relation 'AspNetUsers' does not exist"

**Solution:**
- Table names in PostgreSQL are case-sensitive
- Run migrations:
```powershell
dotnet ef database update
```

### Issue 4: Port 5432 already in use

**Solution:**
```powershell
# Find what's using the port
netstat -ano | findstr :5432

# Stop local PostgreSQL service (if installed)
net stop postgresql-x64-14

# Or use different port
docker run ... -p 5433:5432 ...
# Update connection string: Port=5433
```

---

## Rollback to SQL Server (If Needed)

If you need to switch back:

1. **Stop PostgreSQL:**
```powershell
docker stop postgres-tapcet
```

2. **Reinstall SQL Server package:**
```powershell
dotnet remove package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.11
```

3. **Update `Program.cs`:**
```csharp
options.UseSqlServer(...)
```

4. **Restore SQL Server connection string**

5. **Start SQL Server container:**
```powershell
docker start sqlserver2022
```

6. **Remove and recreate migrations**

---

## Summary

You have successfully migrated to PostgreSQL! ??

**Completed:**
- ? Removed SQL Server dependency
- ? Installed PostgreSQL provider (Npgsql.EntityFrameworkCore.PostgreSQL 8.0.11)
- ? Updated connection string
- ? Configured PostgreSQL in Docker
- ? Created PostgreSQL migrations
- ? Tested authentication endpoints
- ? Ready for free hosting options

**Benefits:**
- ?? Access to free hosting (Supabase, Neon, Railway, etc.)
- ?? Lighter resource usage
- ?? Open-source database
- ?? Better deployment flexibility

**Next Steps:**
1. Test all authentication features with PostgreSQL
2. Choose a free hosting provider (Supabase recommended)
3. Continue building Quiz management features
4. Deploy when ready!

---

## Quick Reference

### PostgreSQL Commands
```powershell
# Start container
docker start postgres-tapcet

# Stop container
docker stop postgres-tapcet

# View logs
docker logs postgres-tapcet

# Connect to database
docker exec -it postgres-tapcet psql -U postgres -d TapcetDb

# Run migrations
dotnet ef migrations add MigrationName
dotnet ef database update

# Build and run
dotnet build
dotnet run
```

### Connection String Template
```
Host=localhost;Port=5432;Database=TapcetDb;Username=postgres;Password=YourPassword
```

---

## Resources

- **Npgsql Documentation:** https://www.npgsql.org/efcore/
- **PostgreSQL Official Docs:** https://www.postgresql.org/docs/
- **pgAdmin Download:** https://www.pgadmin.org/download/
- **Supabase (Free Hosting):** https://supabase.com
- **Neon (Free Hosting):** https://neon.tech
- **Railway (Free Hosting):** https://railway.app
- **Docker PostgreSQL Image:** https://hub.docker.com/_/postgres

---

**Your Tapcet Quiz API is now powered by PostgreSQL!** ????
