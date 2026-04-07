# Authentication

## How It Works

The API uses **JWT Bearer tokens**. Users register or log in via `/api/auth`, receive a token, and include it in the `Authorization` header of subsequent requests.

User accounts, password hashing, and account lockout are managed by **ASP.NET Core Identity**.

---

## Auth Flow

```
1. Register or Login
   POST /api/auth/register  →  creates account, returns JWT immediately
   POST /api/auth/login     →  verifies credentials, returns JWT

2. Use the token
   GET /api/quiz
   Authorization: Bearer eyJhbGci...

3. Token expires after 60 minutes — user must log in again
```

---

## Token Structure

**Algorithm:** HMAC-SHA256
**Expiry:** 60 minutes (set by `JwtSettings:ExpiryInMinutes`)

| Claim | Value |
|-------|-------|
| `sub` (NameIdentifier) | User ID (GUID string) |
| `email` | User's email address |
| `role` | `Admin` or `User` |
| `jti` | Unique token ID (GUID) |

The `sub` claim is used by controllers to identify the caller:
```csharp
User.FindFirstValue(ClaimTypes.NameIdentifier)
```

---

## Roles

| Role | Who has it | What they can do |
|------|-----------|-----------------|
| `Admin` | Default admin only (seeded) | Everything, including Subject management |
| `User` | All registered users | Create quizzes, take quizzes, view own data |

Role is embedded in the token at login. No per-request role lookup.

---

## Registration Rules

| Field | Constraint |
|-------|-----------|
| `username` | 3–50 characters, must be unique |
| `email` | Valid email format, must be unique |
| `password` | 6–100 characters, must include uppercase, lowercase, and digit |
| `confirmPassword` | Must match `password` |

---

## Account Lockout

After **5 consecutive failed login attempts**, the account is locked for **5 minutes**. This is enforced by Identity's `CheckPasswordSignInAsync`. The `auth` rate-limiting policy (10 req/60 sec) provides an additional layer before lockout is even reached.

---

## Endpoint Auth Requirements

Controllers use three patterns:

| Attribute | Meaning |
|-----------|---------|
| `[AllowAnonymous]` | No token required |
| `[Authorize]` | Any valid token (any role) |
| `[Authorize(Roles = "Admin")]` | Must have `Admin` role |

Ownership checks (e.g., "you can only edit your own quiz") are enforced in the **service layer**, not via attributes.

---

## Auth Endpoints

All three endpoints have `[EnableRateLimiting("auth")]` — limited to **10 requests per 60 seconds**.

```
POST /api/auth/register    — Create account
POST /api/auth/login       — Login
POST /api/auth/check-email — Check if email exists (query param: ?email=...)
```

---

## Known Limitations

| Gap | Impact |
|-----|--------|
| No token refresh endpoint | Users must re-login after 60 minutes |
| No token revocation | A stolen token is valid until expiry |
| No password reset | Forgotten passwords have no recovery flow |
| No email verification | Any email address can be used on registration |
| No CORS policy | Browser clients will fail on preflight requests |
