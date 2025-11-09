# SQL Server Docker Setup Guide

## Overview
This guide will help you set up Microsoft SQL Server in a Docker container for local development with your Tapcet Quiz API. This is the recommended approach as it's more reliable than LocalDB and matches production environments better.

## Prerequisites

### 1. Install Docker Desktop

**Download and Install:**
- Visit: https://www.docker.com/products/docker-desktop
- Download Docker Desktop for Windows
- Run the installer
- Restart your computer if prompted

**Verify Installation:**
```powershell
docker --version
```

You should see output like: `Docker version 24.x.x, build xxxxx`

**Start Docker Desktop:**
- Open Docker Desktop from the Start menu
- Wait for Docker to fully start (the whale icon in the system tray should be steady, not animated)

---

## Step-by-Step Setup

### Step 1: Pull SQL Server Image

Open PowerShell or Command Prompt and run:

```powershell
docker pull mcr.microsoft.com/mssql/server:2022-latest
```

**What this does:**
- Downloads the latest SQL Server 2022 container image from Microsoft
- This is a one-time download (~1.5 GB)

**Expected output:**
```
2022-latest: Pulling from mssql/server
...
Status: Downloaded newer image for mcr.microsoft.com/mssql/server:2022-latest
```

---

### Step 2: Run SQL Server Container

Run this command to start a SQL Server container:

```powershell
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" -p 1433:1433 --name sqlserver2022 --hostname sqlserver2022 -d mcr.microsoft.com/mssql/server:2022-latest
```

**Command Breakdown:**
- `-e "ACCEPT_EULA=Y"` - Accepts the SQL Server license agreement
- `-e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd"` - Sets the sa (admin) password
- `-p 1433:1433` - Maps port 1433 from container to your host machine
- `--name sqlserver2022` - Names the container for easy reference
- `--hostname sqlserver2022` - Sets the hostname inside the container
- `-d` - Runs container in detached mode (in the background)
- `mcr.microsoft.com/mssql/server:2022-latest` - The image to use

**Password Requirements:**
Your password must meet SQL Server complexity requirements:
- At least 8 characters
- Contains uppercase letters
- Contains lowercase letters
- Contains numbers
- Contains special characters (!@#$%^&*)

**Expected output:**
```
a1b2c3d4e5f6... (container ID)
```
```
724cf27dbd77ff574555157a6b2679868cb3d7c764dbc9c7ba32901b3871cc9d
```
---

### Step 3: Verify Container is Running

Check if the container is running:

```powershell
docker ps
```

**Expected output:**
```
CONTAINER ID   IMAGE                                        STATUS         PORTS                    NAMES
a1b2c3d4e5f6   mcr.microsoft.com/mssql/server:2022-latest   Up 2 minutes   0.0.0.0:1433->1433/tcp   sqlserver2022
```

**Check container logs:**
```powershell
docker logs sqlserver2022
```

Look for this line to confirm SQL Server started successfully:
```
SQL Server is now ready for client connections.
```

---

### Step 4: Update Connection String in Your Project

Open `tapcet-api/appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=TapcetDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
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

**Important:**
- Replace `YourStrong!Passw0rd` with the same password you used in Step 2
- `Server=localhost,1433` - Connects to localhost on port 1433
- `User Id=sa` - Uses the system administrator account
- `TrustServerCertificate=True` - Required for local development
- `MultipleActiveResultSets=true` - Allows multiple queries on same connection

---

### Step 5: Run Database Migrations

Navigate to your project directory and run migrations:

```powershell
cd "C:\Nemeno\3rd year\tapcet-api\tapcet-api"

# Apply migration to create database
dotnet ef database update
```

**Expected output:**
```
Build started...
Build succeeded.
Applying migration '20240xxx_InitialCreate'.
Done.
```

**What this does:**
- Creates the `TapcetDb` database in your SQL Server container
- Creates all Identity tables (AspNetUsers, AspNetRoles, etc.)
- Sets up your custom User table with additional properties

---

### Step 6: Verify Database Creation

You can verify the database was created using:

**Option 1: Using Docker exec**
```powershell
docker exec -it sqlserver2022 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "SELECT name FROM sys.databases"
```

You should see `TapcetDb` in the list.

**Option 2: Using Azure Data Studio (Recommended)**
1. Download: https://azure.microsoft.com/en-us/products/data-studio
2. Install and open Azure Data Studio
3. Click "New Connection"
4. Enter connection details:
   - **Server:** localhost,1433
   - **Authentication type:** SQL Login
   - **User name:** sa
   - **Password:** YourStrong!Passw0rd
   - **Database:** TapcetDb
   - **Trust server certificate:** Check this box
5. Click "Connect"
6. Expand "Databases" ? "TapcetDb" ? "Tables" to see your tables

---

## Managing Your SQL Server Container

### Stop the Container
```powershell
docker stop sqlserver2022
```

### Start the Container
```powershell
docker start sqlserver2022
```

### Restart the Container
```powershell
docker restart sqlserver2022
```

### Remove the Container
```powershell
# Stop first
docker stop sqlserver2022

# Remove container
docker rm sqlserver2022
```

**Note:** Removing the container will delete all data. To preserve data, use Docker volumes (see Advanced section).

### View Container Logs
```powershell
docker logs sqlserver2022

# Follow logs in real-time
docker logs -f sqlserver2022
```

---

## Common Issues and Solutions

### Issue 1: "Port 1433 is already in use"

**Symptom:**
```
Error response from daemon: driver failed programming external connectivity on endpoint sqlserver2022: Bind for 0.0.0.0:1433 failed: port is already allocated.
```

**Solution:**
Another SQL Server instance is running. Either:

**Option A:** Stop the other SQL Server instance
```powershell
# Check what's using port 1433
netstat -ano | findstr :1433

# Stop local SQL Server service (if installed)
net stop MSSQLSERVER
```

**Option B:** Use a different port
```powershell
# Use port 1434 instead
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" -p 1434:1433 --name sqlserver2022 -d mcr.microsoft.com/mssql/server:2022-latest

# Update connection string to:
# Server=localhost,1434;Database=TapcetDb;...
```

---

### Issue 2: "Login failed for user 'sa'"

**Symptom:**
```
Microsoft.Data.SqlClient.SqlException: Login failed for user 'sa'.
```

**Solutions:**

1. **Wrong password in connection string**
   - Verify the password in `appsettings.json` matches the one you used when creating the container

2. **Password doesn't meet complexity requirements**
   - Stop and remove container: `docker stop sqlserver2022 && docker rm sqlserver2022`
   - Create new container with a stronger password
   - Update `appsettings.json`

---

### Issue 3: "Container keeps restarting"

**Check logs:**
```powershell
docker logs sqlserver2022
```

**Common causes:**
- Insufficient memory (SQL Server needs at least 2 GB RAM)
- Invalid password format
- Conflicting container name

**Solution:**
```powershell
# Remove failed container
docker rm -f sqlserver2022

# Create new one with correct settings
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" -p 1433:1433 --name sqlserver2022 -d mcr.microsoft.com/mssql/server:2022-latest
```

---

### Issue 4: "Docker daemon is not running"

**Symptom:**
```
error during connect: This error may indicate that the docker daemon is not running.
```

**Solution:**
- Start Docker Desktop from the Start menu
- Wait for Docker to fully start (whale icon in system tray should be steady)
- Try your command again

---

## Advanced: Persist Data with Docker Volumes

By default, when you remove a container, all data is lost. To persist data:

### Create Named Volume
```powershell
docker volume create sqlserver-data
```

### Run Container with Volume
```powershell
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" -p 1433:1433 --name sqlserver2022 -v sqlserver-data:/var/opt/mssql -d mcr.microsoft.com/mssql/server:2022-latest
```

**Benefits:**
- Data persists even if you remove the container
- Can recreate container without losing databases
- Easier backup and migration

### List Volumes
```powershell
docker volume ls
```

### Remove Volume (deletes all data)
```powershell
docker volume rm sqlserver-data
```

---

## Testing Your Setup

### Step 1: Run Your API
```powershell
cd "C:\Nemeno\3rd year\tapcet-api\tapcet-api"
dotnet run
```

### Step 2: Open Swagger
Navigate to: https://localhost:5001/swagger (or the port shown in terminal)

### Step 3: Test Registration
1. Click on `POST /api/auth/register`
2. Click "Try it out"
3. Enter test data:
```json
{
  "userName": "testuser",
  "email": "test@example.com",
  "password": "Test@123",
  "confirmPassword": "Test@123"
}
```
4. Click "Execute"
5. You should receive a JWT token

### Step 4: Verify in Database
Using Azure Data Studio or docker exec, run:
```sql
SELECT * FROM AspNetUsers;
```

You should see your test user.

---

## Cleanup Commands

### Remove Everything (nuclear option)
```powershell
# Stop and remove container
docker stop sqlserver2022
docker rm sqlserver2022

# Remove image (optional, saves disk space)
docker rmi mcr.microsoft.com/mssql/server:2022-latest

# Remove volume (deletes all data)
docker volume rm sqlserver-data
```

---

## Quick Reference

### Essential Commands
```powershell
# Start SQL Server
docker start sqlserver2022

# Stop SQL Server
docker stop sqlserver2022

# Check status
docker ps

# View logs
docker logs sqlserver2022

# Connect to database
docker exec -it sqlserver2022 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd"

# Run migrations
dotnet ef database update

# Run API
dotnet run
```

### Connection String Template
```
Server=localhost,1433;Database=TapcetDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true
```

---

## Next Steps

After successful setup:

1. Run `dotnet ef database update` to create your database
2. Start your API with `dotnet run`
3. Test authentication endpoints in Swagger
4. Continue with the Authentication Implementation Guide
5. Build the rest of your Quiz API features

---

## Resources

- Docker Desktop Documentation: https://docs.docker.com/desktop/
- SQL Server Docker Image: https://hub.docker.com/_/microsoft-mssql-server
- Azure Data Studio: https://azure.microsoft.com/en-us/products/data-studio
- SQL Server Connection Strings: https://www.connectionstrings.com/sql-server/

---

## Summary

You now have:
- ? SQL Server running in Docker
- ? Database created with EF Core migrations
- ? Connection string configured
- ? Ready to develop and test your API

Your SQL Server container is isolated, easy to manage, and can be stopped/started without affecting your system. When you deploy to Azure later, you'll use Azure SQL Database with a similar connection string pattern.
