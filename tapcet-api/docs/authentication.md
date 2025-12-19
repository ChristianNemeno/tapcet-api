# Authentication & Authorization

## Overview

TAPCET Quiz API uses JWT (JSON Web Token) based authentication with ASP.NET Core Identity for user management. This approach provides stateless, secure authentication suitable for modern web and mobile applications.

## Authentication Flow

```
Client                    API                     Identity                Database
  ?                        ?                         ?                        ?
  ???Register Request???????                         ?                        ?
  ?                        ???Create User?????????????                        ?
  ?                        ?                         ???Save User??????????????
  ?                        ?                         ????User Created??????????
  ????Success Message???????                         ?                        ?
  ?                        ?                         ?                        ?
  ???Login Request??????????                         ?                        ?
  ?                        ???Validate Credentials????                        ?
  ?                        ?                         ???Query User?????????????
  ?                        ?                         ????User Data?????????????
  ?                        ????User Valid?????????????                        ?
  ?                        ?                         ?                        ?
  ?                        ???Generate JWT Token??????                        ?
  ????JWT Token?????????????                         ?                        ?
  ?                        ?                         ?                        ?
  ???API Request + Token????                         ?                        ?
  ?                        ???Validate Token??????????                        ?
  ?                        ????Token Valid????????????                        ?
  ????API Response??????????                         ?                        ?
```

## User Registration

### Registration Process

1. Client sends registration data (username, email, password)
2. Server validates input (password strength, email format, uniqueness)
3. ASP.NET Core Identity creates user with hashed password
4. User is assigned default "User" role
5. Server returns success message
6. User can immediately login

### Password Requirements

Configured in `Program.cs`:

```csharp
options.Password.RequireDigit = true;
options.Password.RequireLowercase = true;
options.Password.RequireUppercase = true;
options.Password.RequireNonAlphanumeric = false;
options.Password.RequiredLength = 6;
```

**Requirements**:
- Minimum 6 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- Special characters are optional

### Email Uniqueness

Configured in `Program.cs`:

```csharp
options.User.RequireUniqueEmail = true;
```

Each email can only be registered once.

### Account Lockout

Configured in `Program.cs`:

```csharp
options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
options.Lockout.MaxFailedAccessAttempts = 5;
```

After 5 failed login attempts, account is locked for 5 minutes.

## User Login

### Login Process

1. Client sends credentials (email, password)
2. Server validates credentials using Identity UserManager
3. If valid, server generates JWT token with user claims
4. Server returns token with expiration time
5. Client stores token (localStorage, sessionStorage, or memory)
6. Client includes token in Authorization header for subsequent requests

### JWT Token Structure

**Header**:
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload (Claims)**:
```json
{
  "sub": "user-id-123",
  "email": "john@example.com",
  "jti": "unique-token-id",
  "exp": 1705329600,
  "iss": "TapcetAPI",
  "aud": "TapcetClient"
}
```

**Signature**: Created using HS256 algorithm with secret key

### JWT Configuration

Configured in `appsettings.json`:

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

**Token Expiration**: 60 minutes (1 hour)

### Token Validation

Configured in `Program.cs`:

```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = jwtSettings["Issuer"],
    ValidAudience = jwtSettings["Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
    ClockSkew = TimeSpan.Zero
};
```

**Validation Checks**:
- Token signature is valid
- Token is not expired (ClockSkew = 0 for precise expiration)
- Issuer matches configuration
- Audience matches configuration

## Authorization

### Role-Based Authorization

The system defines two roles:

1. **User** (default)
   - Can create and manage own quizzes
   - Can attempt any active quiz
   - Can view own results and history

2. **Admin**
   - All User permissions
   - Can manage any quiz
   - Can view all user data
   - Can access admin endpoints (future enhancement)

### Authorization Levels

**Public Endpoints** (No authentication required):
```csharp
[AllowAnonymous]
public async Task<IActionResult> GetActiveQuizzes()
```

- GET /api/quiz (view all quizzes)
- GET /api/quiz/{id} (view quiz details)
- GET /api/quiz/active (view active quizzes)

**Authenticated Endpoints** (Valid JWT required):
```csharp
[Authorize]
public async Task<IActionResult> CreateQuiz()
```

- All POST, PUT, DELETE operations
- User-specific GET operations

**Owner-Only Endpoints** (Must own the resource):
- Update/Delete own quizzes
- View own attempt results

Enforced in service layer:
```csharp
var quiz = await _context.Quizzes
    .FirstOrDefaultAsync(q => q.Id == id && q.CreatedById == userId);
```

### Applying Authorization

**Controller Level** (All endpoints require auth):
```csharp
[Route("api/[controller]")]
[Authorize]
public class QuizController : ControllerBase
```

**Method Level** (Override controller-level):
```csharp
[HttpGet("active")]
[AllowAnonymous]
public async Task<IActionResult> GetActiveQuizzes()
```

**Role-Based** (Require specific role):
```csharp
[HttpPost]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> AdminOnlyAction()
```

## Extracting User Identity

### In Controllers

```csharp
private string? GetUserId()
{
    return User.FindFirstValue(ClaimTypes.NameIdentifier);
}
```

The `User` property is populated by ASP.NET Core authentication middleware from the JWT token.

### Available Claims

- `ClaimTypes.NameIdentifier`: User ID (GUID)
- `ClaimTypes.Email`: User email
- `ClaimTypes.Role`: User role(s)
- `JwtRegisteredClaimNames.Sub`: Subject (User ID)
- `JwtRegisteredClaimNames.Jti`: JWT ID (unique token identifier)

### Example Usage

```csharp
[HttpPost]
[Authorize]
public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizDto dto)
{
    var userId = GetUserId();
    if (string.IsNullOrEmpty(userId))
        return Unauthorized(new { message = "User not authenticated" });
    
    var result = await _service.CreateQuizAsync(dto, userId);
    return Ok(result);
}
```

## Security Best Practices

### Secret Key Management

**Development**:
- Store in `appsettings.Development.json`
- Use a complex key (32+ characters)

**Production**:
- Use User Secrets: `dotnet user-secrets set "JwtSettings:SecretKey" "your-key"`
- Or Environment Variables: `JwtSettings__SecretKey=your-key`
- Or Azure Key Vault for cloud deployments

### HTTPS Enforcement

Configured in `Program.cs`:

```csharp
app.UseHttpsRedirection();
```

All HTTP requests are redirected to HTTPS.

### Token Storage (Client-Side)

**Recommended Approaches**:

1. **Memory Storage** (Most secure for SPAs)
   - Store in JavaScript variable
   - Lost on page refresh (requires re-login)
   - Not vulnerable to XSS

2. **sessionStorage** (Balance of security and UX)
   - Survives page refresh
   - Cleared on browser close
   - Vulnerable to XSS

3. **localStorage** (Most persistent, least secure)
   - Survives browser close
   - Most convenient UX
   - Most vulnerable to XSS

**Never store tokens in**:
- Cookies (unless httpOnly and secure)
- URL parameters
- Local storage if handling sensitive data

### Request Headers

```http
GET /api/quiz HTTP/1.1
Host: localhost:7237
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

**Format**: `Bearer <token>` (note the space after "Bearer")

## Token Expiration Handling

### Client-Side Strategy

1. **Store expiration time** from login response
2. **Check before each request** if token is near expiration
3. **Implement token refresh** (future enhancement) or prompt re-login
4. **Handle 401 responses** by redirecting to login

### Server-Side Behavior

When token expires:
- Server returns `401 Unauthorized`
- Client must obtain new token via login
- Current implementation does not support token refresh

### Implementing Token Refresh (Future Enhancement)

```csharp
// Refresh token endpoint
[HttpPost("refresh")]
public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
{
    // Validate refresh token
    // Generate new access token
    // Return new token with expiration
}
```

## Common Authentication Scenarios

### Scenario 1: New User Registration

```http
POST /api/auth/register
Content-Type: application/json

{
  "userName": "johndoe",
  "email": "john@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!"
}
```

Response:
```json
{
  "message": "Registration successful! Please check your email to verify your account."
}
```

### Scenario 2: User Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "Password123!"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "john@example.com",
  "userName": "johndoe",
  "expiresAt": "2024-01-15T15:30:00Z"
}
```

### Scenario 3: Authenticated Request

```http
GET /api/quiz-attempt/user/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Scenario 4: Unauthorized Request

```http
GET /api/quiz-attempt/user/me
(No Authorization header)
```

Response:
```json
{
  "message": "User not authenticated"
}
```

Status: 401 Unauthorized

### Scenario 5: Expired Token

```http
GET /api/quiz-attempt/user/me
Authorization: Bearer <expired-token>
```

Response: Empty body
Status: 401 Unauthorized

### Scenario 6: Invalid Token

```http
GET /api/quiz-attempt/user/me
Authorization: Bearer invalid-token
```

Response: Empty body
Status: 401 Unauthorized

## Troubleshooting

### Token Not Working

**Check**:
1. Token format: `Bearer <token>` with space
2. Token not expired (check `expiresAt`)
3. Secret key matches between generation and validation
4. Issuer and Audience match configuration

### User Cannot Login

**Check**:
1. Credentials are correct (case-sensitive password)
2. Account is not locked out
3. User exists in database
4. Password meets requirements

### 401 on Valid Token

**Check**:
1. Token is not expired (ClockSkew = 0)
2. System clock is synchronized
3. Secret key is correct
4. Issuer/Audience configuration matches

## Security Checklist

Production deployment checklist:

- [ ] Use strong secret key (32+ random characters)
- [ ] Store secret key in secure location (not in code)
- [ ] Enable HTTPS
- [ ] Implement rate limiting
- [ ] Add CORS policy for frontend domains
- [ ] Log authentication failures
- [ ] Monitor for suspicious activity
- [ ] Implement account lockout
- [ ] Use secure password requirements
- [ ] Validate all JWT claims
- [ ] Set appropriate token expiration
- [ ] Implement token refresh mechanism
- [ ] Add two-factor authentication (future)
- [ ] Implement email verification (future)
- [ ] Add password reset functionality (future)
