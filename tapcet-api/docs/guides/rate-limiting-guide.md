# Rate Limiting Implementation Guide

## Overview

This guide walks you through implementing rate limiting in the TAPCET Quiz API using ASP.NET Core 7.0+ built-in rate limiting middleware. Rate limiting protects your API from abuse, ensures fair resource usage, and improves overall system stability.

## Target Rate Limits

| Endpoint Type | Limit | Window |
|--------------|-------|--------|
| Unauthenticated endpoints | 100 requests | Per minute per IP |
| Authenticated endpoints | 300 requests | Per minute per user |
| Authentication endpoints | 10 requests | Per minute per IP |

---

## Step 1: Install Required Package

The rate limiting middleware is built into .NET 7+, but ensure you're using .NET 8:

```bash
# Verify your .NET version
dotnet --version
# Should show 8.0.x
```

No additional packages needed — it's built-in!

---

## Step 2: Add Rate Limiting Configuration

### Update `appsettings.json`

Add rate limiting configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "JwtSettings": {
    "...": "..."
  },
  "RateLimiting": {
    "PublicEndpoints": {
      "PermitLimit": 100,
      "Window": 60,
      "QueueLimit": 0
    },
    "AuthenticatedEndpoints": {
      "PermitLimit": 300,
      "Window": 60,
      "QueueLimit": 0
    },
    "AuthEndpoints": {
      "PermitLimit": 10,
      "Window": 60,
      "QueueLimit": 0
    }
  },
  "Logging": {
    "...": "..."
  }
}
```

**Configuration Explained**:
- `PermitLimit`: Number of requests allowed
- `Window`: Time window in seconds
- `QueueLimit`: Number of requests to queue (0 = reject immediately)

---

## Step 3: Configure Rate Limiting in `Program.cs`

### Add Rate Limiting Services

Add this **before** `var app = builder.Build();`:

```csharp
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ... existing services ...

// ==================== RATE LIMITING CONFIGURATION ====================

builder.Services.AddRateLimiter(options =>
{
    // 1. PUBLIC ENDPOINTS POLICY (By IP Address)
    options.AddFixedWindowLimiter("public", limiterOptions =>
    {
        limiterOptions.PermitLimit = builder.Configuration.GetValue<int>("RateLimiting:PublicEndpoints:PermitLimit");
        limiterOptions.Window = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("RateLimiting:PublicEndpoints:Window"));
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = builder.Configuration.GetValue<int>("RateLimiting:PublicEndpoints:QueueLimit");
    });

    // 2. AUTHENTICATED ENDPOINTS POLICY (By User ID)
    options.AddFixedWindowLimiter("authenticated", limiterOptions =>
    {
        limiterOptions.PermitLimit = builder.Configuration.GetValue<int>("RateLimiting:AuthenticatedEndpoints:PermitLimit");
        limiterOptions.Window = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("RateLimiting:AuthenticatedEndpoints:Window"));
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = builder.Configuration.GetValue<int>("RateLimiting:AuthenticatedEndpoints:QueueLimit");
    });

    // 3. AUTH ENDPOINTS POLICY (Stricter limits by IP)
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = builder.Configuration.GetValue<int>("RateLimiting:AuthEndpoints:PermitLimit");
        limiterOptions.Window = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("RateLimiting:AuthEndpoints:Window"));
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = builder.Configuration.GetValue<int>("RateLimiting:AuthEndpoints:QueueLimit");
    });

    // 4. PARTITION BY IP OR USER
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        // Get user ID from JWT claims if authenticated
        var userId = httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Authenticated user - partition by user ID
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: $"user:{userId}",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 300,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
        }
        else
        {
            // Unauthenticated - partition by IP address
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: $"ip:{ipAddress}",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
        }
    });

    // 5. CUSTOMIZE REJECTION RESPONSE
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        var response = new
        {
            message = "Too many requests. Please try again later.",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                ? retryAfter.TotalSeconds
                : 60
        };

        await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
    };
});

// ==================== END RATE LIMITING CONFIGURATION ====================

var app = builder.Build();

// ... existing middleware ...
```

### Add Rate Limiting Middleware

Add this **after** `app.UseAuthentication()` and **before** `app.UseAuthorization()`:

```csharp
app.UseAuthentication();

// ? ADD THIS LINE
app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

**Order matters!** Rate limiting should come after authentication so it can identify users.

---

## Step 4: Apply Rate Limiting to Controllers

### Option A: Apply to Specific Controllers

#### Auth Controller (Stricter Limits)

Update `Controllers/AuthController.cs`:

```csharp
using Microsoft.AspNetCore.RateLimiting;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting("auth")]  // ? ADD THIS
public class AuthController : Controller
{
    // ... existing code ...
}
```

#### Quiz Controller (Mixed Limits)

Update `Controllers/QuizController.cs`:

```csharp
using Microsoft.AspNetCore.RateLimiting;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("authenticated")]  // ? ADD THIS
public class QuizController : ControllerBase
{
    // ... existing code ...

    [HttpGet("active")]
    [AllowAnonymous]
    [DisableRateLimiting]  // ? Disable controller-level, use global
    public async Task<IActionResult> GetActiveQuizzes()
    {
        // This will use the global limiter (100/min per IP)
        var result = await _quizService.GetActiveQuizzesAsync();
        return Ok(result);
    }
}
```

#### Quiz Attempt Controller

Update `Controllers/QuizAttemptController.cs`:

```csharp
using Microsoft.AspNetCore.RateLimiting;

[Route("api/quiz-attempt")]
[ApiController]
[Authorize]
[EnableRateLimiting("authenticated")]  // ? ADD THIS
public class QuizAttemptController : ControllerBase
{
    // ... existing code ...
}
```

### Option B: Use Global Limiter Only

If you prefer the global limiter (automatic IP/user detection), you don't need `[EnableRateLimiting]` attributes. The global limiter in Step 3 will handle everything automatically:

- Unauthenticated requests: 100/min per IP
- Authenticated requests: 300/min per user

---

## Step 5: Test Rate Limiting

### Test with cURL

#### Test Unauthenticated Endpoint

```bash
# Run this in a loop to hit the limit
for i in {1..105}; do
  echo "Request $i"
  curl -X GET http://localhost:5080/api/quiz/active
  sleep 0.5
done
```

**Expected**: First 100 succeed, 101-105 return `429 Too Many Requests`

#### Test Authenticated Endpoint

```bash
# Get token first
TOKEN=$(curl -X POST http://localhost:5080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Password123!"}' \
  | jq -r '.token')

# Make 305 requests
for i in {1..305}; do
  echo "Request $i"
  curl -X GET http://localhost:5080/api/quiz \
    -H "Authorization: Bearer $TOKEN"
  sleep 0.2
done
```

**Expected**: First 300 succeed, 301-305 return `429 Too Many Requests`

#### Test Auth Endpoint (Stricter)

```bash
# Try to login 15 times
for i in {1..15}; do
  echo "Login attempt $i"
  curl -X POST http://localhost:5080/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"email":"test@example.com","password":"wrong"}'
  sleep 3
done
```

**Expected**: First 10 succeed (or fail auth), 11-15 return `429 Too Many Requests`

### Test with Swagger

1. Open Swagger UI: `http://localhost:5080/swagger`
2. Try calling `GET /api/quiz/active` rapidly (use F5 or refresh)
3. After ~100 requests in a minute, you'll get:

```json
{
  "message": "Too many requests. Please try again later.",
  "retryAfter": 60
}
```

---

## Step 6: Verify Rate Limit Headers

ASP.NET Core automatically adds rate limit headers to responses:

```http
HTTP/1.1 200 OK
RateLimit-Limit: 100
RateLimit-Remaining: 42
RateLimit-Reset: 37
```

**Headers Explained**:
- `RateLimit-Limit`: Total requests allowed in window
- `RateLimit-Remaining`: Requests left in current window
- `RateLimit-Reset`: Seconds until window resets

---

## Step 7: Cloud Run Considerations

### Behind a Load Balancer

When deployed to Cloud Run, your app runs behind Google's load balancer. The IP address will be the load balancer's IP, not the client's.

**Fix**: Use the `X-Forwarded-For` header:

```csharp
// In Program.cs, before builder.Build()
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// After app.Build(), before other middleware
app.UseForwardedHeaders();
```

Then update the rate limiter to use the forwarded IP:

```csharp
var ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
    ?? httpContext.Connection.RemoteIpAddress?.ToString()
    ?? "unknown";
```

### Distributed Rate Limiting (Multiple Instances)

If you scale to multiple Cloud Run instances, each instance tracks rate limits independently. This means users can send:
- 100 requests/min to Instance A
- 100 requests/min to Instance B
- = 200 requests/min total

**Solution**: Use a distributed cache like **Redis** (recommended for production).

---

## Step 8: Production Best Practices

### 1. Use Redis for Distributed Rate Limiting

Install package:

```bash
cd tapcet-api
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

Update `Program.cs`:

```csharp
// Add Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "TapcetAPI:";
});

// Update rate limiter to use Redis
// (requires custom implementation or third-party library like AspNetCoreRateLimit)
```

### 2. Add Rate Limit Metrics

Log rate limit hits for monitoring:

```csharp
options.OnRejected = async (context, cancellationToken) =>
{
    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogWarning("Rate limit exceeded for {IP} on {Path}", 
        context.HttpContext.Connection.RemoteIpAddress, 
        context.HttpContext.Request.Path);
    
    // ... existing rejection logic ...
};
```

### 3. Whitelist Known IPs

```csharp
var whitelistedIPs = new[] { "34.120.0.0", "10.0.0.1" }; // Cloud Run health checks, etc.

if (whitelistedIPs.Contains(ipAddress))
{
    return RateLimitPartition.GetNoLimiter("whitelist");
}
```

### 4. Different Limits for Different Roles

```csharp
var isAdmin = httpContext.User?.IsInRole("Admin") ?? false;
var limit = isAdmin ? 1000 : 300;

return RateLimitPartition.GetFixedWindowLimiter(
    partitionKey: $"user:{userId}",
    factory: _ => new FixedWindowRateLimiterOptions
    {
        PermitLimit = limit,
        Window = TimeSpan.FromMinutes(1)
    });
```

---

## Step 9: Update API Documentation

Update `docs/api-reference.md`:

### Add Rate Limiting Section

```markdown
## Rate Limiting

The API implements rate limiting to ensure fair usage and system stability.

### Rate Limits

| User Type | Limit | Window |
|-----------|-------|--------|
| Unauthenticated (by IP) | 100 requests | 1 minute |
| Authenticated (by User) | 300 requests | 1 minute |
| Auth endpoints (by IP) | 10 requests | 1 minute |

### Response Headers

All responses include rate limit information:

```http
RateLimit-Limit: 100
RateLimit-Remaining: 42
RateLimit-Reset: 37
```

### 429 Too Many Requests

When rate limit is exceeded:

**Response**:
```json
{
  "message": "Too many requests. Please try again later.",
  "retryAfter": 60
}
```

**Status Code**: `429 Too Many Requests`

**Retry**: Wait for the number of seconds specified in `retryAfter` before retrying.

### Best Practices

1. **Monitor headers**: Check `RateLimit-Remaining` to know how many requests you have left
2. **Implement backoff**: If you receive a 429, wait for `retryAfter` seconds
3. **Cache responses**: Reduce unnecessary API calls by caching data client-side
4. **Batch requests**: Where possible, use batch endpoints instead of multiple individual requests
```

---

## Step 10: Testing Checklist

Before deploying to Cloud Run:

- [ ] Rate limiting works locally
- [ ] 429 responses are properly formatted
- [ ] Rate limit headers are present in responses
- [ ] Authenticated vs unauthenticated users have different limits
- [ ] Auth endpoints have stricter limits
- [ ] Forwarded headers work correctly (test behind nginx/proxy)
- [ ] Rate limiting doesn't break existing functionality
- [ ] Swagger UI reflects rate limiting behavior
- [ ] Logs show rate limit violations

---

## Troubleshooting

### Issue: All Requests Get 429 Immediately

**Cause**: Rate limiter is too strict or misconfigured.

**Fix**: Check your `appsettings.json` values and ensure `PermitLimit` is not 0.

### Issue: Rate Limiting Not Working

**Cause**: Middleware order is wrong.

**Fix**: Ensure `app.UseRateLimiter()` is called **after** `app.UseAuthentication()`.

### Issue: IP Address is Always the Same

**Cause**: Behind a proxy/load balancer without forwarded headers.

**Fix**: Add `app.UseForwardedHeaders()` as shown in Step 7.

### Issue: Authenticated Users Hit Limit Too Fast

**Cause**: User ID extraction is failing.

**Debug**:
```csharp
var userId = httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
Console.WriteLine($"User ID: {userId}"); // Add logging
```

---

## Summary

You've implemented:
- ? IP-based rate limiting for unauthenticated users (100/min)
- ? User-based rate limiting for authenticated users (300/min)
- ? Stricter limits for auth endpoints (10/min)
- ? Custom 429 responses with retry information
- ? Rate limit headers in all responses
- ? Cloud Run compatibility with forwarded headers

Your API is now protected from abuse and ready for production deployment! ??

---

## Additional Resources

- [ASP.NET Core Rate Limiting Docs](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [Google Cloud Run Best Practices](https://cloud.google.com/run/docs/best-practices)
- [AspNetCoreRateLimit Library](https://github.com/stefanprodan/AspNetCoreRateLimit) (alternative with Redis support)
