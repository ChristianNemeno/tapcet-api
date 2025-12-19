# Security Policy

## Supported Versions

This project currently supports the latest code on the `master` branch.

## Reporting a Vulnerability

If you discover a security vulnerability, do not open a public issue.

Instead, report it privately to the maintainers with:
- A clear description of the vulnerability
- Steps to reproduce
- Impact assessment
- Any suggested mitigation

## Security Considerations

### Secrets

- Do not commit JWT secret keys or database passwords.
- Use environment variables or User Secrets for local development.
- Use a dedicated secret store for production deployments.

### Authentication

- JWT tokens expire based on `JwtSettings:ExpiryInMinutes`.
- The API requires HTTPS redirection by default.

### Authorization

- Controllers are protected with `[Authorize]` by default, except specific public endpoints.
- Resource ownership checks are verified in the service layer.

### Input Validation

- DTOs use data annotations for validation.
- Services validate business rules (e.g. number of choices, single correct answer).

### Rate Limiting

Rate limiting is not currently implemented. If exposed publicly, add rate limiting at the reverse proxy or application layer.

## Recommended Production Hardening

- Enforce secret management (environment variables / vault)
- Add global exception handling with consistent error responses
- Add rate limiting for authentication and submission endpoints
- Add CORS restrictions to frontend domains
- Add monitoring and alerting
