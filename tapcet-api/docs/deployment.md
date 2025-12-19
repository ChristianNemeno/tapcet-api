# Deployment Guide

## Overview

This guide covers deployment scenarios for the TAPCET Quiz API, from local development to production environments.

## Pre-Deployment Checklist

- [ ] All unit tests passing
- [ ] Integration tests passing
- [ ] Code review completed
- [ ] Database migrations verified
- [ ] Environment variables configured
- [ ] Secrets secured (not in source control)
- [ ] CORS policy configured
- [ ] Logging configured
- [ ] Error handling tested
- [ ] Performance tested
- [ ] Security audit completed

## Local Development Deployment

### Quick Start

```bash
# Clone repository
git clone https://github.com/ChristianNemeno/tapcet-api.git
cd tapcet-api

# Start PostgreSQL (Docker)
docker run --name tapcet-pg \
  -e POSTGRES_USER=tapcet \
  -e POSTGRES_PASSWORD=TapcetDev123 \
  -e POSTGRES_DB=TapcetDb \
  -p 6543:5432 \
  -d postgres:17

# Apply migrations
cd tapcet-api
dotnet ef database update

# Run application
dotnet run
```

### Configuration

**appsettings.Development.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=6543;Database=TapcetDb;Username=tapcet;Password=TapcetDev123"
  },
  "JwtSettings": {
    "SecretKey": "DevelopmentSecretKeyForTesting123456789",
    "Issuer": "TapcetAPI",
    "Audience": "TapcetClient",
    "ExpiryInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## Docker Deployment

### Dockerfile

Create `Dockerfile` in project root:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY tapcet-api/tapcet-api.csproj tapcet-api/
RUN dotnet restore tapcet-api/tapcet-api.csproj

# Copy everything else and build
COPY tapcet-api/ tapcet-api/
WORKDIR /src/tapcet-api
RUN dotnet build tapcet-api.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish tapcet-api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "tapcet-api.dll"]
```

### Docker Compose

Create `docker-compose.yml`:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:17
    container_name: tapcet-postgres
    environment:
      POSTGRES_USER: tapcet
      POSTGRES_PASSWORD: ${DB_PASSWORD}
      POSTGRES_DB: TapcetDb
    ports:
      - "6543:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - tapcet-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U tapcet"]
      interval: 10s
      timeout: 5s
      retries: 5

  api:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: tapcet-api
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:80
      ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=TapcetDb;Username=tapcet;Password=${DB_PASSWORD}"
      JwtSettings__SecretKey: ${JWT_SECRET}
      JwtSettings__Issuer: TapcetAPI
      JwtSettings__Audience: TapcetClient
      JwtSettings__ExpiryInMinutes: 60
    ports:
      - "8080:80"
    depends_on:
      postgres:
        condition: service_healthy
    networks:
      - tapcet-network

volumes:
  postgres_data:

networks:
  tapcet-network:
    driver: bridge
```

### Environment Variables File

Create `.env` file (DO NOT commit to source control):

```bash
DB_PASSWORD=YourStrongPasswordHere
JWT_SECRET=YourSuperSecretJWTKeyHere_MustBe32CharactersOrMore
```

### Build and Run

```bash
# Build images
docker-compose build

# Start services
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

### Run Migrations in Docker

```bash
# Access API container
docker exec -it tapcet-api bash

# Run migrations
dotnet ef database update

# Exit container
exit
```

## Production Deployment

### Security Considerations

1. **Use User Secrets or Environment Variables**

```bash
# Set environment variables
export ConnectionStrings__DefaultConnection="Host=prod-db;Port=5432;Database=TapcetDb;Username=tapcet;Password=STRONG_PASSWORD"
export JwtSettings__SecretKey="VERY_STRONG_SECRET_KEY"
```

2. **Enable HTTPS**

```csharp
// Program.cs
app.UseHttpsRedirection();

// Configure Kestrel
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(443, listenOptions =>
    {
        listenOptions.UseHttps("/path/to/certificate.pfx", "certificate-password");
    });
});
```

3. **Configure CORS**

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Use CORS
app.UseCors("Production");
```

4. **Disable Development Features**

```csharp
// Program.cs
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Production error handling
    app.UseExceptionHandler("/error");
    app.UseHsts();
}
```

### Database Production Setup

1. **Use Managed Database Service**
   - AWS RDS for PostgreSQL
   - Azure Database for PostgreSQL
   - Google Cloud SQL

2. **Connection String Format**

```
Host=prod-db.region.rds.amazonaws.com;Port=5432;Database=TapcetDb;Username=tapcet;Password=STRONG_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

3. **Database Migration Strategy**

```bash
# Generate SQL script
dotnet ef migrations script -o migration.sql

# Review script manually
# Apply via database client or CI/CD pipeline
```

4. **Backup Strategy**

```bash
# Automated backups (daily)
pg_dump -h prod-db.domain.com -U tapcet TapcetDb > backup_$(date +%Y%m%d).sql

# Restore from backup
psql -h prod-db.domain.com -U tapcet TapcetDb < backup_20240115.sql
```

### Application Hosting Options

#### Option 1: Azure App Service

1. **Publish from Visual Studio**
   - Right-click project
   - Publish
   - Choose Azure App Service
   - Configure settings

2. **Publish from CLI**

```bash
# Login to Azure
az login

# Create resource group
az group create --name tapcet-rg --location eastus

# Create app service plan
az appservice plan create --name tapcet-plan --resource-group tapcet-rg --sku B1

# Create web app
az webapp create --resource-group tapcet-rg --plan tapcet-plan --name tapcet-api

# Deploy
dotnet publish -c Release
cd bin/Release/net8.0/publish
zip -r deploy.zip *
az webapp deployment source config-zip --resource-group tapcet-rg --name tapcet-api --src deploy.zip
```

3. **Configure App Settings**

```bash
az webapp config appsettings set --resource-group tapcet-rg --name tapcet-api --settings \
  "ConnectionStrings__DefaultConnection=YOUR_CONNECTION_STRING" \
  "JwtSettings__SecretKey=YOUR_SECRET_KEY"
```

#### Option 2: AWS Elastic Beanstalk

1. **Install EB CLI**

```bash
pip install awsebcli
```

2. **Initialize EB Application**

```bash
eb init -p "64bit Amazon Linux 2 v2.5.0 running .NET Core" tapcet-api --region us-east-1
```

3. **Create Environment**

```bash
eb create tapcet-prod --database.engine postgres --database.username tapcet
```

4. **Deploy**

```bash
dotnet publish -c Release
cd bin/Release/net8.0/publish
zip -r ../deploy.zip *
eb deploy
```

5. **Set Environment Variables**

```bash
eb setenv \
  ConnectionStrings__DefaultConnection="YOUR_CONNECTION_STRING" \
  JwtSettings__SecretKey="YOUR_SECRET_KEY"
```

#### Option 3: Linux Server (Ubuntu)

1. **Install .NET Runtime**

```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

# Install .NET Runtime
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-8.0
```

2. **Deploy Application**

```bash
# Publish locally
dotnet publish -c Release -o ./publish

# Transfer to server
scp -r ./publish user@server:/var/www/tapcet-api

# SSH to server
ssh user@server

# Set permissions
sudo chown -R www-data:www-data /var/www/tapcet-api
sudo chmod -R 755 /var/www/tapcet-api
```

3. **Create Systemd Service**

```bash
sudo nano /etc/systemd/system/tapcet-api.service
```

```ini
[Unit]
Description=TAPCET Quiz API
After=network.target

[Service]
WorkingDirectory=/var/www/tapcet-api
ExecStart=/usr/bin/dotnet /var/www/tapcet-api/tapcet-api.dll
Restart=always
RestartSec=10
SyslogIdentifier=tapcet-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=ConnectionStrings__DefaultConnection=YOUR_CONNECTION_STRING
Environment=JwtSettings__SecretKey=YOUR_SECRET_KEY

[Install]
WantedBy=multi-user.target
```

4. **Start Service**

```bash
sudo systemctl enable tapcet-api
sudo systemctl start tapcet-api
sudo systemctl status tapcet-api
```

5. **Configure Nginx Reverse Proxy**

```bash
sudo nano /etc/nginx/sites-available/tapcet-api
```

```nginx
server {
    listen 80;
    server_name api.yourdomain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
# Enable site
sudo ln -s /etc/nginx/sites-available/tapcet-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

6. **Setup SSL with Let's Encrypt**

```bash
sudo apt-get install certbot python3-certbot-nginx
sudo certbot --nginx -d api.yourdomain.com
```

## CI/CD Pipeline

### GitHub Actions

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Production

on:
  push:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Test
      run: dotnet test --no-restore --verbosity normal

  deploy:
    needs: test
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Publish
      run: dotnet publish -c Release -o ./publish
    
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: tapcet-api
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

## Monitoring & Logging

### Application Insights (Azure)

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

```json
// appsettings.json
{
  "ApplicationInsights": {
    "InstrumentationKey": "YOUR_KEY_HERE"
  }
}
```

### Logging to File

```bash
# Install package
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
```

```csharp
// Program.cs
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/tapcet-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

### Health Checks

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"));

app.MapHealthChecks("/health");
```

## Performance Optimization

### Enable Response Compression

```csharp
// Program.cs
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

app.UseResponseCompression();
```

### Enable Response Caching

```csharp
// Program.cs
builder.Services.AddResponseCaching();

app.UseResponseCaching();

// In controllers
[ResponseCache(Duration = 60)]
public async Task<IActionResult> GetActiveQuizzes()
```

### Database Connection Pooling

Connection pooling is enabled by default in Npgsql. Configure in connection string:

```
Host=localhost;Database=TapcetDb;Username=tapcet;Password=pass;Minimum Pool Size=5;Maximum Pool Size=100
```

## Rollback Strategy

### Application Rollback

**Azure**:
```bash
az webapp deployment slot swap --resource-group tapcet-rg --name tapcet-api --slot staging --target-slot production
```

**Docker**:
```bash
# Tag current version
docker tag tapcet-api:latest tapcet-api:v1.0.0

# Rollback to previous version
docker-compose down
docker-compose up -d tapcet-api:v0.9.0
```

### Database Rollback

```bash
# Revert to previous migration
dotnet ef database update PreviousMigrationName

# Or restore from backup
psql -h prod-db.domain.com -U tapcet TapcetDb < backup_20240115.sql
```

## Troubleshooting Production Issues

### Check Application Logs

```bash
# Azure
az webapp log tail --resource-group tapcet-rg --name tapcet-api

# Docker
docker logs tapcet-api

# Linux systemd
sudo journalctl -u tapcet-api -f
```

### Check Database Connection

```bash
# Test connection
psql -h prod-db.domain.com -U tapcet -d TapcetDb -c "SELECT 1;"

# Check active connections
SELECT * FROM pg_stat_activity WHERE datname = 'TapcetDb';
```

### Check Application Health

```bash
curl https://api.yourdomain.com/health
```

## Post-Deployment Verification

- [ ] Application is accessible
- [ ] Health check endpoint responds
- [ ] Database connection works
- [ ] Authentication works
- [ ] API endpoints respond correctly
- [ ] HTTPS is enforced
- [ ] Logs are being written
- [ ] Performance is acceptable
- [ ] Error handling works
- [ ] Monitoring is active

## Maintenance Windows

Plan for regular maintenance:
- Database backups: Daily at 2 AM
- Application updates: Sundays at 3 AM
- Database maintenance: Monthly
- Security patches: As needed

## Disaster Recovery

1. **Database Backups**: Daily automated backups with 30-day retention
2. **Application Backups**: Store deployable artifacts
3. **Recovery Time Objective (RTO)**: 1 hour
4. **Recovery Point Objective (RPO)**: 24 hours
5. **Test recovery procedures quarterly**
