# Authentication & Authorization Demo

A comprehensive educational project demonstrating full **Authentication** and **Authorization** flows in **ASP.NET Core**, built step-by-step for learning and portfolio purposes.

Core technologies and concepts covered:

- ASP.NET Core Identity (user management, roles, claims)
- JWT (JSON Web Tokens) for stateless authentication
- Refresh Tokens with secure storage and rotation
- Policy-based Authorization
- Role-Based Access Control (RBAC)
- Best security practices (OWASP alignment, HTTPS, token validation, etc.)

The project follows a structured daily progression, making it easy to follow, understand, and extend.

## Project Goals

- Serve as a clear, production-grade reference for backend authentication & authorization in .NET
- Be suitable for sharing on LinkedIn, GitHub, and job applications
- Explain every important decision and security consideration

## Current Progress

### Day 1 – Project Setup & Foundation
- Created ASP.NET Core Web API project (.NET 10)
- Installed required NuGet packages:
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore
  - Microsoft.EntityFrameworkCore.Sqlite
  - Microsoft.EntityFrameworkCore.Tools
  - Microsoft.EntityFrameworkCore.Design
- Implemented `ApplicationDbContext` inheriting from `IdentityDbContext`
- Configured SQLite database (`app.db`) via connection string
- Set up ASP.NET Core Identity services with basic password rules
- Added authentication & authorization middleware pipeline
- Generated and applied initial EF Core migration (`InitialIdentityMigration`)
- Enabled Swagger UI for API testing

Database file `app.db` is now created in the project root with all standard Identity tables.

### Day 2 – ASP.NET Core Identity Basics: Registration & Login

- Created DTOs: `RegisterDto` and `LoginDto`
- Implemented `AccountController` with:
  - `POST api/account/register` — user creation with secure password hashing
  - `POST api/account/login` — password sign-in using SignInManager
- Automatic password policy enforcement (length, complexity)
- Basic input validation and error reporting
- Tested endpoints via Swagger

### Day 3 – Login / Logout Flows & Session Management

- Enhanced `Login` endpoint:
  - Used `SignInManager.PasswordSignInAsync` with proper parameters
  - Supported "Remember Me" (persistent vs. session cookie)
  - Enabled lockout on failure
  - Improved error messages (invalid credentials, lockout, 2FA hint)
- Added secure `Logout` endpoint using `SignInManager.SignOutAsync()`
- Introduced basic protected endpoint with `[Authorize]` attribute
- Demonstrated cookie-based session management & termination
- Tested full flow: register > login > access protected > logout > access denied

### Day 4 – JWT Integration with Identity

- Installed `Microsoft.AspNetCore.Authentication.JwtBearer`
- Created `JwtSettings` configuration class and bound from appsettings.json
- Implemented `JwtTokenService` for generating signed JWTs with claims (sub, email, roles)
- Configured JWT bearer authentication scheme as default in Program.cs
- Updated `Login` endpoint to validate credentials via Identity > issue JWT on success
- Replaced cookie-based auth with stateless token-based authentication
- Tested token issuance and bearer authentication via Swagger

### Day 5 – JWT Security & Claims Hardening

- Moved secret key to User Secrets (avoid committing to Git)
- Reduced access token lifetime to 15 minutes (short-lived tokens)
- Added standard claims: sub, email, unique_name, jti, iat, auth_time
- Included role claims via ClaimTypes.Role for [Authorize(Roles = "...")]
- Strengthened TokenValidationParameters:
  - Strict issuer, audience, lifetime, and signing key checks
  - Minimal clock skew (5 seconds)
  - Require signed tokens and expiration
- Aligned with OWASP JWT best practices: no sensitive data in payload, strong validation, no "none" algorithm support
- Tested token integrity, expiration enforcement, and claim presence

### Day 6 – Refresh Token Implementation with Rotation and Revocation

- Created `RefreshToken` entity with hashed storage for security
- Implemented secure random generation of long-lived refresh tokens
- Stored tokens in database with expiration and revocation checks
- Added token rotation: revoke old token and issue new one on every refresh
- Created `/api/account/refresh` endpoint
- Implemented revocation of all tokens for a user
- Security features: hashing, IP logging, rotation to mitigate replay and leakage risks

### Day 7 – Refresh Token Error Handling & Improvements

- Enhanced error handling in `RefreshAsync`:
  - Precise checks for revoked, expired, not found, and empty/missing tokens
  - Clear, user-friendly error messages (in English for consistency with project documentation)
- Added `/api/account/revoke` endpoint to allow authenticated users to manually revoke all their refresh tokens
- Implemented simple console logging for successful refresh operations and rotation events (useful for debugging)
- Comprehensive testing of failure scenarios:
  - Expired refresh token
  - Revoked refresh token
  - Invalid / tampered token
  - Missing or empty refresh token
- Ensured rotation continues to invalidate previous tokens reliably