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