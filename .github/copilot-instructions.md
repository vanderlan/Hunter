# Hunter - Repository Custom Instructions for GitHub Copilot

## Project Overview

**Hunter** is an AI-powered recruitment management system that streamlines the entire hiring process from CV analysis to candidate tracking.

- **Repository Size**: ~120 files across 7 projects
- **Primary Language**: C# (.NET 10)
- **Secondary Language**: TypeScript (Angular 17)
- **Database**: PostgreSQL 16.3
- **Architecture**: Clean Architecture with CQRS pattern
- **Deployment**: Docker Compose (3 services)

### Tech Stack
- Backend: ASP.NET Core Web API (.NET 10), Entity Framework Core 10, MediatR, FluentValidation
- Frontend: Angular 17 (standalone components), Angular Material
- Infrastructure: PostgreSQL 16.3, New Relic APM, Docker
- AI Integration: OpenAI GPT-4o OR DeepSeek (dual provider support)
- Additional: PdfPig (PDF extraction), HtmlAgilityPack (web scraping)

---

## Critical Build & Test Instructions

### Prerequisites
- .NET 10 SDK
- Node.js 18+
- PostgreSQL 16.3 (or use Docker Compose)
- EF Core Tools: `dotnet tool install --global dotnet-ef --version 10.0.0`

### Environment Setup

**ALWAYS run database migrations before starting the API:**

```bash
dotnet ef database update -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api
```

**If migration fails, check:**
1. PostgreSQL is running on port 5432
2. Connection string in `appsettings.json` or environment variable `ConnectionStrings__PostgreSql`
3. Default credentials: `Username=postgres`, `Password=123PgSql`, `Database=HunterDev`

### Build Commands (Verified - Execute in This Order)

**Backend:**
```bash
# 1. Restore packages
dotnet restore Hunter.sln

# 2. Build solution
dotnet build Hunter.sln

# 3. Run API (after migrations)
dotnet run --project src/GeminiCode.Hunter.Api
# API runs on http://localhost:5010
# Swagger UI: http://localhost:5010/swagger
```

**Frontend:**
```bash
cd front-end

# 1. Install dependencies (ALWAYS use --legacy-peer-deps)
npm install --legacy-peer-deps

# 2. Run dev server
npm start
# or
ng serve
# Frontend runs on http://localhost:4200

# 3. Build for production
npm run build -- --configuration=production
```

**Full Stack (Docker - Recommended):**
```bash
# Start all services (PostgreSQL, API, Frontend)
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop all services
docker-compose down

# Access:
# - Frontend: http://localhost:5200
# - API: http://localhost:5010
# - PostgreSQL: localhost:5432
```

### Test Commands (Verified)

```bash
# Run all tests with coverage
dotnet test Hunter.sln

# Run tests with detailed coverage output
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=coverage /p:Exclude=[xunit.*]* Hunter.sln

# Integration tests require PostgreSQL
# CI/CD uses real database (not in-memory)
```

**Test database setup (if running tests locally):**
```bash
# Create test database
createdb -h localhost -U postgres HunterTests

# Run migrations for test database
dotnet ef database update -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api --connection "Host=localhost;Port=5432;Database=HunterTests;Username=postgres;Password=123PgSql"
```

### Creating Migrations (ALWAYS Required After Entity Changes)

```bash
# Create new migration
dotnet ef migrations add MigrationName -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api

# Apply migration
dotnet ef database update -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api

# Rollback last migration (if needed)
dotnet ef migrations remove -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api
```

**Common Migration Errors:**
- Error: "Build failed" ? Run `dotnet build Hunter.sln` first
- Error: "No DbContext found" ? Always use `-s src/GeminiCode.Hunter.Api`
- Error: "Pending model changes" ? You forgot to create a migration after entity changes

---

## Project Layout & Architecture

### Solution Structure (Clean Architecture)

```
Hunter/
??? .github/
?   ??? workflows/
?   ?   ??? build.yml          # CI pipeline (build, test, coverage, Sonar)
?   ?   ??? deploy.yml         # Deployment pipeline
?   ??? copilot-instructions.md
?
??? src/
?   ??? GeminiCode.Hunter.Api/              # ?? API Layer (Controllers, Middleware, Config)
?   ?   ??? Controllers/V1/                 # REST API Controllers (versioned)
?   ?   ??? Middleware/ExceptionMiddleware.cs
?   ?   ??? Filters/AutoValidationActionFilter.cs
?   ?   ??? Attributes/AuthorizeForAttribute.cs
?   ?   ??? Configuration/                  # Swagger, Auth, Versioning, etc.
?   ?   ??? Program.cs                      # Entry point
?   ?   ??? Bootstrapper.cs                 # Service registration
?   ?   ??? appsettings.json                # Configuration
?   ?
?   ??? GeminiCode.Hunter.Application.Core/ # ?? Application Layer (Use Cases - CQRS)
?   ?   ??? UseCases/                       # All business logic here
?   ?   ?   ??? {Feature}/Commands/{Action}/
?   ?   ?   ?   ??? {Action}Request.cs
?   ?   ?   ?   ??? {Action}RequestHandler.cs
?   ?   ?   ?   ??? {Action}RequestValidator.cs
?   ?   ?   ?   ??? {Action}Response.cs
?   ?   ?   ??? {Feature}/Queries/{Action}/
?   ?   ?   ??? {Feature}/Notifications/
?   ?   ??? Services/                       # AIService, EmailService, PdfService, etc.
?   ?   ??? BackgroundServices/             # JobSyncBackgroundService
?   ?
?   ??? GeminiCode.Hunter.Domain.Core/      # ?? Domain Layer (Entities, Interfaces)
?   ?   ??? Entities/                       # Domain entities (inherit from BaseEntity)
?   ?   ??? Repositories/                   # Repository interfaces (I*Repository)
?   ?   ??? Services/                       # Service interfaces (I*Service)
?   ?   ??? Exceptions/                     # Custom exceptions
?   ?   ??? Authentication/                 # CurrentUser, JWT config
?   ?   ??? Configuration/                  # Settings classes
?   ?
?   ??? GeminiCode.Hunter.Infra.Data/       # ?? Infrastructure Layer (EF Core, Repos)
?   ?   ??? Context/DataContext.cs          # EF Core DbContext
?   ?   ??? Mapping/                        # Fluent API entity mappings
?   ?   ??? Repository/                     # Repository implementations
?   ?   ??? UnitOfWork/UnitOfWork.cs        # Transaction coordinator
?   ?   ??? Migrations/                     # EF Core migrations
?   ?
?   ??? GeminiCode.Hunter.Crosscutting.Ioc/ # Dependency Injection
?   ?   ??? Dependencies/DependenciesInjectionConfiguration.cs
?   ?
?   ??? GeminiCode.Hunter.Crosscutting.Resources/ # Localization resources
?
??? tests/
?   ??? GeminiCode.Hunter.Test/
?       ??? Unit/                           # Unit tests (handlers, controllers)
?       ??? Integration/                    # Integration tests (API endpoints)
?       ??? Api/V1/                         # API integration tests
?       ??? Shared/Faker/                   # Bogus test data generators
?
??? front-end/                              # Angular 17 application
?   ??? src/app/                            # Angular components
?   ?   ??? features/                       # Feature modules
?   ?   ??? core/                           # Services, guards, interceptors
?   ?   ??? shared/                         # Shared components
?   ??? angular.json                        # Angular configuration
?   ??? package.json                        # Dependencies
?   ??? Dockerfile                          # Frontend Docker image
?
??? Hunter.sln                              # Solution file
??? Dockerfile                              # Backend Docker image
??? docker-compose.yml                      # Full stack orchestration
??? README.md                               # Project documentation
```

### Key Architectural Patterns

**1. CQRS with MediatR (MANDATORY)**
- ALL business logic implemented as Commands (write) or Queries (read)
- Controllers only route requests to MediatR handlers
- Handlers use `IUnitOfWork` for data access
- Example: `UserCreateRequest` ? `UserCreateRequestHandler` ? `UserCreateResponse`

**2. Repository Pattern with Unit of Work**
- Never access `DbContext` directly in handlers
- Use `IUnitOfWork` to access repositories
- Call `await unitOfWork.CommitAsync()` to persist changes
- All repositories inherit from `BaseEntityRepository<T>`

**3. Automatic Validation (FluentValidation)**
- All requests have `*RequestValidator` classes
- `AutoValidationActionFilter` automatically validates and returns 400 errors
- No manual validation in controllers

**4. Exception-Based Error Handling**
- Use custom exceptions: `NotFoundException`, `ConflictException`, `UnauthorizedUserException`, `BusinessException`
- `ExceptionMiddleware` maps to HTTP status codes automatically
- Never return error responses manually in handlers

**5. Entity Conventions**
- All entities inherit from `BaseEntity` (PublicId, CreatedAt, LastUpdated)
- Internal ID: `long {Entity}Id` (primary key, never exposed)
- External ID: `string PublicId` (Guid, used in API)
- Timestamps: `timestamp without time zone` in PostgreSQL
- Mappings: Fluent API in `{Entity}Mapping.cs` files

---

## Critical Domain Rules

### Core Entities & Business Rules

**User**
- Roles: `Admin` (full access) or `Partner` (limited)
- Password: SHA-512 hashed
- Email: unique identifier

**JobPosition**
- Scraped hourly from external source (8 AM - 10 PM)
- AI-enriched ONCE with OpenAI/DeepSeek
- Never re-enrich existing jobs (expensive)
- Status: Active (on external site) or Closed (removed from external site)

**Candidate**
- Created from CV PDF upload
- Name, Email, Phone extracted via AI
- Email is unique - updates existing candidate if email matches
- Must have associated CV file (PDF only, max 10MB)

**CvAnalysis & CvAnalysisMatch**
- Matches candidates to jobs using AI
- Match threshold: > 60% to be included
- Stores: match percentage, matching skills, missing skills, referral reason
- Historical record (never updated, only new analyses created)

**HiringProcess**
- Status flow: `Referred` ? `Screening` ? `Technical Interview` ? `Client Interview` ? `Hired`/`Reproved`
- Only Admin users can update status
- Status history tracked in `HiringProcessStatusHistory` (immutable audit trail)
- Notes can be added by any authenticated user (immutable)

### Data Access Patterns

**ALWAYS use this pattern in handlers:**

```csharp
public class SomeRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<SomeRequestHandler> logger,
    ICurrentUser currentUser)
    : IRequestHandler<SomeRequest, SomeResponse>
{
    public async Task<SomeResponse> Handle(SomeRequest request, CancellationToken cancellationToken)
    {
        // 1. Get data
        var entity = await unitOfWork.EntityRepository.GetByIdAsync(request.Id);
        
        // 2. Validate business rules
        if (entity == null)
            throw new NotFoundException($"Entity {request.Id} not found");
        
        // 3. Modify data
        entity.Property = request.NewValue;
        unitOfWork.EntityRepository.Update(entity);
        
        // 4. Persist (ALWAYS call CommitAsync)
        await unitOfWork.CommitAsync();
        
        // 5. Return response
        return new SomeResponse { Id = entity.PublicId };
    }
}
```

---

## Common Errors & Workarounds

### Error: "Pending model changes detected"
**Cause**: Entity changed but no migration created
**Fix**: 
```bash
dotnet ef migrations add DescriptiveName -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api
dotnet ef database update -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api
```

### Error: "Cannot insert NULL into column"
**Cause**: Forgot to set required properties or timestamps
**Fix**: 
- Check entity mapping has `.IsRequired()` where needed
- For timestamps: `DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)`

### Error: "Operation timed out" (CV analysis)
**Cause**: AI analysis takes 1-5 minutes
**Fix**: Already handled - `[RequestTimeout(300000)]` on CV analysis endpoints

### Error: "npm ERR! peer dependencies conflict"
**Fix**: ALWAYS use `npm install --legacy-peer-deps` in front-end

### Error: "Bearer token has expired"
**Cause**: Access token expires after 1 hour
**Fix**: Use refresh token endpoint (`POST /Auth/RefreshToken`) - already implemented

### Error: "No DbContext named 'DataContext' was found"
**Fix**: Use `-s src/GeminiCode.Hunter.Api` parameter with all `dotnet ef` commands

---

## Validation & CI Pipeline

### GitHub Actions Workflow (.github/workflows/build.yml)

**Runs on**: Every push to `master` and all PRs

**Steps (in order):**
1. Setup .NET 10 & Node.js 18
2. Start PostgreSQL 16.3 container
3. Install EF Core tools
4. Restore packages
5. **Run EF migrations** (required before build)
6. Build backend
7. Run tests with coverage
8. Build frontend
9. SonarCloud analysis (code quality)

**To replicate locally:**
```bash
# 1. Start PostgreSQL
docker run -d -p 5432:5432 -e POSTGRES_DB=HunterTests -e POSTGRES_PASSWORD=123PgSql postgres:16.3

# 2. Run migrations
dotnet ef database update -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api

# 3. Build & test
dotnet build Hunter.sln
dotnet test Hunter.sln

# 4. Build frontend
cd front-end
npm install --legacy-peer-deps
npm run build
```

### Manual Validation Steps

**After making changes, ALWAYS:**
1. Run `dotnet build Hunter.sln` (must succeed)
2. Run `dotnet test Hunter.sln` (all tests must pass)
3. If entities changed: Create migration and apply it
4. Start API and verify Swagger loads (`http://localhost:5010/swagger`)
5. Test the specific endpoint you changed

---

## Configuration & Environment Variables

### appsettings.json (Development)

**Required sections:**
- `ConnectionStrings:PostgreSql` - Database connection
- `AI:Provider` - "OpenAI" or "DeepSeek"
- `OpenAI:ApiKey` and `OpenAI:Model` - OpenAI config
- `DeepSeek:ApiKey` and `DeepSeek:Model` - DeepSeek config
- `Email:*` - SMTP settings (Brevo relay)
- `JobScraper:CareersUrl` - External job board URL
- `FeatureManagement:AutoCandidateMatching` - Feature flag

### Environment Variables (Production/Docker)

**Database:**
- `ConnectionStrings__PostgreSql` (double underscore)

**AI Provider:**
- `AI__Provider=DeepSeek` (or OpenAI)
- `OpenAI__ApiKey=sk-...`
- `DeepSeek__ApiKey=sk-...`

**Email:**
- `Email__SmtpHost`, `Email__SmtpPort`, `Email__FromEmail`, etc.

---

## AI Integration Details

### Supported Providers
1. **OpenAI GPT-4o**: More accurate, higher cost (~$0.03/analysis)
2. **DeepSeek**: 90% cost savings (~$0.003/analysis), slightly less accurate

### AI Service Usage

**CV Analysis (`AnalyzeCvRequest`):**
- Extracts: Name, Email, Phone from CV text
- Matches CV against all active job positions
- Returns matches > 60% with detailed analysis
- **Time**: 1-5 minutes (already has request timeout)

**Job Enrichment (`EnrichJobDetailsAsync`):**
- Extracts structured data from HTML job postings
- Categories: Summary, Responsibilities, Requirements, Must-Have Skills, Nice-To-Have Skills
- **Important**: Only enriches ONCE per job (never re-enrich)

### AI Best Practices
- Temperature: 0.2 (low for consistency)
- Use JSON schema format for structured responses
- Log all AI requests for debugging
- Handle rate limits gracefully (already implemented)

---

## Default Login Credentials

**Admin User:**
- Email: `adm@hunter.com`
- Password: `123`

**Use for testing locally or in Docker**

---

## Important Instructions for Coding Agents

### When Creating New Features

1. **ALWAYS follow CQRS pattern** - Create Request, Handler, Validator, Response
2. **Check similar features first** - Look at existing use cases for patterns
3. **Use dependency injection** - Never use `new` for services
4. **Add validation** - Create FluentValidation validator for all requests
5. **Handle exceptions** - Use custom exceptions (NotFoundException, ConflictException)
6. **Create tests** - Unit tests for handlers, integration tests for endpoints
7. **Update migrations** - If entities changed, create migration before testing

### When Fixing Bugs

1. **Run tests first** - Verify the bug with a failing test
2. **Check logs** - Look for exceptions or errors
3. **Test locally** - Use Docker Compose to replicate production
4. **Validate migrations** - Ensure database schema matches entities
5. **Run full test suite** - Ensure fix doesn't break other features

### When Modifying Entities

1. **Update entity class** in `Domain.Core/Entities/`
2. **Update mapping** in `Infra.Data/Mapping/`
3. **Create migration**: `dotnet ef migrations add DescriptiveName -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api`
4. **Apply migration**: `dotnet ef database update -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api`
5. **Update repository** if new queries needed
6. **Update handlers** that use the entity
7. **Run tests** to ensure nothing breaks

### Trust These Instructions

**The information in this file is verified and accurate. Only search the codebase if:**
- Information here is incomplete
- Information here contradicts actual code
- You need specific implementation details not covered

**Otherwise, trust these instructions to save time and avoid exploration.**

---

## Quick Command Reference

```bash
# Full build & test
dotnet build Hunter.sln && dotnet test Hunter.sln

# Create & apply migration
dotnet ef migrations add MigrationName -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api
dotnet ef database update -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api

# Run API
dotnet run --project src/GeminiCode.Hunter.Api

# Frontend (in front-end directory)
npm install --legacy-peer-deps && npm start

# Docker (full stack)
docker-compose up -d
docker-compose logs -f api
docker-compose down

# Reset database (nuclear option)
dotnet ef database drop -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api --force
dotnet ef database update -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api
```

---

*This file follows GitHub Copilot custom instructions guidelines and is optimized for coding agents to minimize exploration time and command failures.*
