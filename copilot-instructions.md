# Hunter - Copilot Instructions

## Project Overview

**Hunter** is an AI-powered recruitment management system built with .NET 10 and Angular 17. It streamlines the entire hiring process from CV analysis to candidate tracking using advanced AI technology (OpenAI GPT-4o or DeepSeek).

### Core Purpose
- Intelligent CV analysis and matching with job positions
- Complete hiring process lifecycle management
- AI-powered job enrichment from external sources
- Role-based recruitment workflow automation

---

## Technology Stack

### Backend (.NET 10)
- **Framework**: ASP.NET Core Web API (.NET 10)
- **Database**: PostgreSQL 16.3 with Entity Framework Core 10
- **Architecture**: CQRS pattern with MediatR
- **Authentication**: JWT with Refresh Tokens
- **Monitoring**: New Relic APM
- **Logging**: Serilog (currently disabled for resource constraints)
- **AI Providers**: 
  - OpenAI GPT-4o (default)
  - DeepSeek (90% cost savings alternative)

### Frontend (Angular 17)
- **Framework**: Angular 17 with Standalone Components
- **UI Library**: Angular Material Design
- **State Management**: RxJS
- **Language**: TypeScript
- **Styling**: SCSS

### Key Libraries
- **EF Core 10**: Data access with Fluent API mappings
- **MediatR**: CQRS and notification pattern
- **FluentValidation**: Input validation
- **PdfPig**: PDF text extraction
- **HtmlAgilityPack**: Web scraping
- **OpenAI SDK**: AI integration
- **Bogus**: Test data generation (tests only)

---

## Architecture & Design Patterns

### 1. Project Structure (Clean Architecture)

```
Hunter/
??? src/
?   ??? GeminiCode.Hunter.Api/              # API Controllers & Configuration
?   ??? GeminiCode.Hunter.Application.Core/ # Use Cases (CQRS), Services, Background Services
?   ??? GeminiCode.Hunter.Domain.Core/      # Entities, Repositories, Interfaces
?   ??? GeminiCode.Hunter.Infra.Data/       # EF Core, Repositories, Migrations
?   ??? GeminiCode.Hunter.Crosscutting.Ioc/ # Dependency Injection
?   ??? GeminiCode.Hunter.Crosscutting.Resources/ # Localization
??? tests/
?   ??? GeminiCode.Hunter.Test/             # Unit & Integration Tests
??? front-end/                              # Angular 17 Application
```

### 2. CQRS Pattern (MediatR)

**All business logic is implemented as Commands or Queries with MediatR:**

- **Commands**: Modify data (Create, Update, Delete)
- **Queries**: Read data (GetById, GetPaginated)
- **Notifications**: Cross-cutting concerns (emails, logging)

**File Naming Convention:**
```
UseCases/{Feature}/{Type}/{Action}/
  - {Action}Request.cs          # MediatR request
  - {Action}RequestHandler.cs   # Handler implementation
  - {Action}RequestValidator.cs # FluentValidation
  - {Action}Response.cs         # Response DTO (optional)
```

**Example Structure:**
```
UseCases/
  Users/
    Commands/
      Create/
        - UserCreateRequest.cs
        - UserCreateRequestHandler.cs
        - UserCreateRequestValidator.cs
        - UserCreateResponse.cs
    Queries/
      GetById/
        - UserGetByIdRequest.cs
        - UserGetByIdRequestHandler.cs
        - UserGetByIdResponse.cs
    Notifications/
      UserCreated/
        - UserCreatedNotification.cs
        - UserCreatedNotificationHandler.cs
```

### 3. Repository Pattern with Unit of Work

**All data access goes through repositories:**

```csharp
// Accessing repositories
public class SomeHandler(IUnitOfWork unitOfWork)
{
    public async Task Handle()
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(id);
        await unitOfWork.CandidateRepository.AddAsync(candidate);
        await unitOfWork.CommitAsync(); // Save changes
    }
}
```

**Key Points:**
- Never use `DbContext` directly in handlers
- Always use `IUnitOfWork` to access repositories
- Call `unitOfWork.CommitAsync()` to persist changes
- Repositories inherit from `BaseEntityRepository<T>`

### 4. Entity Conventions

**BaseEntity** (all entities inherit from this):
```csharp
public abstract class BaseEntity
{
    public string PublicId { get; set; }      // Guid exposed to API
    public DateTime LastUpdated { get; set; } // Auto-updated by EF
    public DateTime CreatedAt { get; set; }   // Auto-set by EF
}
```

**Entity Mapping Conventions:**
- Use Fluent API (never Data Annotations)
- Configure in `{Entity}Mapping.cs` files
- Timestamps as `timestamp without time zone`
- All primary keys are `long` (internal, never exposed)
- Exposed IDs are `PublicId` (string Guid)
- Use indexes for foreign keys and frequently queried fields

**Example Mapping:**
```csharp
public class CandidateMapping : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.HasKey(x => x.CandidateId);
        builder.Property(x => x.PublicId).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp without time zone");
        
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

### 5. Validation Strategy

**Use FluentValidation for all input validation:**

```csharp
public class UserCreateRequestValidator : AbstractValidator<UserCreateRequest>
{
    public UserCreateRequestValidator(IStringLocalizer<HunterResources> stringLocalizer)
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage(stringLocalizer[MessagesKeys.UserMessages.EmptyName]);
        
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage(stringLocalizer[MessagesKeys.UserMessages.EmptyEmail]);
    }
}
```

**Validation is automatic via `AutoValidationActionFilter`:**
- Validators are automatically discovered and executed
- Returns `400 BadRequest` with `ValidationProblemDetails`
- No need to manually call validation in controllers

### 6. Exception Handling

**Custom Exceptions (always use these):**

```csharp
// 404 Not Found
throw new NotFoundException($"User with PublicId {id} not found");

// 409 Conflict
throw new ConflictException("Email already in use");

// 401 Unauthorized
throw new UnauthorizedUserException("Invalid credentials");

// 400 Bad Request
throw new BusinessException("Invalid operation", "Business Rule Violation");
```

**ExceptionMiddleware automatically handles:**
- Maps exceptions to appropriate HTTP status codes
- Returns structured `ExceptionResponse`
- Logs internal server errors
- Never throw generic `Exception` - use specific custom exceptions

### 7. Authentication & Authorization

**JWT with Refresh Token:**
- Access token expires in 1 hour
- Refresh token for long-lived sessions
- Claims: Name, Email, Role (Admin/Partner), Id (PublicId)

**Role-Based Authorization:**
```csharp
[AuthorizeFor(Roles.Admin, Roles.Partner)] // Controller level
public class CandidatesController : ApiController { }

[AuthorizeFor(Roles.Admin)] // Action level - Admin only
public async Task<IActionResult> Delete(string id) { }
```

**Current User Access:**
```csharp
public class Handler(ICurrentUser currentUser)
{
    // Access authenticated user info
    var userId = currentUser.Id;        // PublicId
    var name = currentUser.Name;
    var email = currentUser.Email;
    var isAdmin = currentUser.IsAdmin;
}
```

### 8. Background Services

**Pattern for Hosted Services:**
```csharp
public class JobSyncBackgroundService(
    ILogger<JobSyncBackgroundService> logger,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            
            // Execute logic
            await mediator.Send(new SyncJobsRequest(), stoppingToken);
            
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
```

**Key Points:**
- Always use `IServiceScopeFactory` to create scopes
- Never inject scoped services directly (only `ILogger` and `IServiceScopeFactory`)
- Handle `OperationCanceledException` gracefully
- Log errors and implement retry logic

---

## Core Domain Entities

### User
- Roles: `Admin` (full access) / `Partner` (limited)
- Password hashed with SHA-512
- Can receive email notifications for job changes
- Tracks last login timestamp

### JobPosition
- Scraped from external sources (ApplyJob)
- AI-enriched with structured data (summary, responsibilities, requirements, skills)
- Enrichment happens once (never re-enriched)
- Status: Active/Closed based on external availability

### Candidate
- Created from CV upload and AI extraction
- Name, Email, Phone extracted via AI
- Email is unique identifier (updates existing if found)
- Linked to user who uploaded the CV
- Stores analysis timestamp

### CvAnalysis & CvAnalysisMatch
- Historical record of CV analysis runs
- One analysis can have multiple matches (one per job)
- Match percentage threshold: > 60% to be included
- Stores: matching skills, missing skills, referral reason

### HiringProcess
- Complete lifecycle: `Referred` ? `Screening` ? `Technical Interview` ? `Client Interview` ? `Hired/Reproved`
- Only Admin can update status
- Referral reason explains why candidate was referred
- Can be started from CV analysis or manually

### HiringProcessStatusHistory
- Audit trail of status changes
- Immutable timeline
- Shows who changed status and when

### HiringProcessNote
- Collaborative notes system
- Any authenticated user can add notes
- Cannot be edited or deleted

### RefreshToken
- JWT refresh token storage
- One per user (updated on refresh)
- Expires in 30 days

### CandidateFile & File
- Stores uploaded CVs (PDF format only)
- Binary data stored in database
- Supports soft delete (one active CV per candidate)

---

## Coding Conventions

### General Rules

1. **Naming**
   - Controllers: `{Feature}Controller` (e.g., `UsersController`)
   - Use cases: `{Entity}{Action}Request/Response/Handler`
   - Validators: `{Request}Validator`
   - Repositories: `I{Entity}Repository`
   - Services: `I{Service}Service`

2. **No Comments**
   - Code should be self-documenting
   - Use meaningful names for methods and variables
   - Only add XML comments for public APIs (controllers)

3. **Dependency Injection**
   - Use primary constructors in C# 12+: `public class MyClass(IService service)`
   - Never use `new` for services - always inject via constructor

4. **Async/Await**
   - All I/O operations must be async
   - Use `async/await` consistently
   - Never use `.Result` or `.Wait()` - always `await`

5. **Nullable Reference Types**
   - Enabled project-wide
   - Use `?` for nullable types
   - Use `!` only when 100% certain (avoid if possible)

### API Controller Conventions

```csharp
[ApiVersion(1.0)]
[ApiController]
[Route("[controller]")]
[AuthorizeFor(Roles.Admin, Roles.Partner)]
public class UsersController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet]
    [SwaggerResponse((int)HttpStatusCode.OK, "Success", typeof(PagedList<UserDto>))]
    public async Task<IActionResult> Get([FromQuery] UserGetPaginatedRequest filter)
        => Ok(await Mediator.Send(filter));

    [HttpPost]
    [SwaggerResponse((int)HttpStatusCode.Created, "Created", typeof(UserCreateResponse))]
    public async Task<IActionResult> Post([FromBody] UserCreateRequest request)
        => Created(await Mediator.Send(request));

    [HttpPut("{id}")]
    [SwaggerResponse((int)HttpStatusCode.NoContent, "Updated")]
    public async Task<IActionResult> Put(string id, [FromBody] UserUpdateRequest request)
    {
        request.PublicId = id;
        await Mediator.Send(request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [SwaggerResponse((int)HttpStatusCode.NoContent, "Deleted")]
    public async Task<IActionResult> Delete(string id)
    {
        await Mediator.Send(new UserDeleteRequest(id));
        return NoContent();
    }
}
```

**Controller Rules:**
- Inherit from `ApiController(IMediator)`
- Use Swagger attributes for documentation
- Keep thin - delegate all logic to handlers
- Use expression-bodied members when possible
- Always set route parameter on request object for PUT/DELETE

### Handler Conventions

```csharp
public class UserCreateRequestHandler(
    IUnitOfWork unitOfWork,
    ILogger<UserCreateRequestHandler> logger,
    ICurrentUser currentUser)
    : IRequestHandler<UserCreateRequest, UserCreateResponse>
{
    public async Task<UserCreateResponse> Handle(
        UserCreateRequest request, 
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating user: {Email} by {User}", 
            request.Email, currentUser.Name);

        // 1. Validate business rules (not input validation)
        var existingUser = await unitOfWork.UserRepository
            .GetByEmailAsync(request.Email);
        
        if (existingUser != null)
            throw new ConflictException("Email already exists");

        // 2. Create entity
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Password = HashPassword(request.Password),
            Profile = request.Profile
        };

        // 3. Persist
        await unitOfWork.UserRepository.AddAsync(user);
        await unitOfWork.CommitAsync();

        // 4. Publish notification (if needed)
        await mediator.Publish(new UserCreatedNotification(user));

        // 5. Return response
        return new UserCreateResponse
        {
            PublicId = user.PublicId,
            Name = user.Name,
            Email = user.Email
        };
    }
}
```

**Handler Rules:**
- One responsibility per handler
- Log important actions
- Validate business rules (not input - that's FluentValidation)
- Use transactions implicitly via UnitOfWork
- Publish notifications for cross-cutting concerns
- Always return responses (even if empty DTO)

### Service Conventions

**AI Service Example:**
```csharp
public class AIService : IAIService
{
    private ChatClient _chatClient;
    private readonly ILogger<AIService> _logger;
    private readonly string _provider; // "OpenAI" or "DeepSeek"

    public async Task<CvAnalysisResult> AnalyzeCvAgainstJobsAsync(
        string cvText, 
        List<JobPositionSummary> jobPositions)
    {
        _logger.LogInformation("Analyzing CV using {Provider}", _provider);

        // Build prompt
        var prompt = BuildPrompt(cvText, jobPositions);

        // Call AI with structured output
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are an expert HR recruiter..."),
            new UserChatMessage(prompt)
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0.2f, // Low for consistency
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(...)
        };

        var response = await _chatClient.CompleteChatAsync(messages, options);
        
        // Parse and validate response
        var result = JsonSerializer.Deserialize<CvAnalysisResult>(response);
        
        return result;
    }
}
```

**Service Rules:**
- Implement interface from `Domain.Core/Services`
- Always inject `ILogger<T>`
- Handle exceptions and log errors
- Use configuration from `appsettings.json`
- Support dual providers (OpenAI & DeepSeek) for AI services

---

## Testing Conventions

### Unit Tests (xUnit)

```csharp
public class UserCreateRequestHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_CreatesUser()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<UserCreateRequestHandler>>();
        var handler = new UserCreateRequestHandler(unitOfWork, logger);
        
        var request = new UserCreateRequest
        {
            Name = "John Doe",
            Email = "john@example.com"
        };

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Email.Should().Be("john@example.com");
        await unitOfWork.UserRepository.Received(1).AddAsync(Arg.Any<User>());
    }
}
```

### Integration Tests

**Use real PostgreSQL database (via Docker in CI):**
- Tests run against `HunterTests` database
- Database is migrated before tests
- Uses `WebApplicationFactory<Program>` for in-memory API
- Clean up test data after each test

**Test Data Generation with Bogus:**
```csharp
public class UserFaker : Faker<User>
{
    public UserFaker()
    {
        RuleFor(u => u.Name, f => f.Person.FullName);
        RuleFor(u => u.Email, f => f.Person.Email);
        RuleFor(u => u.Profile, f => f.PickRandom<UserProfile>());
    }
}
```

---

## Environment Configuration

### appsettings.json Structure

```json
{
  "ConnectionStrings": {
    "PostgreSql": "Host=localhost;Port=5432;Database=HunterDev;Username=postgres;Password=123PgSql"
  },
  
  "ApplicationSettings": {
    "TimeZoneId": "E. South America Standard Time",
    "ConvertTimezoneInResponses": true
  },
  
  "AI": {
    "Provider": "DeepSeek" // "OpenAI" or "DeepSeek"
  },
  
  "OpenAI": {
    "ApiKey": "sk-...",
    "Model": "gpt-4o"
  },
  
  "DeepSeek": {
    "ApiKey": "sk-...",
    "Model": "deepseek-chat"
  },
  
  "Email": {
    "SmtpHost": "smtp-relay.brevo.com",
    "SmtpPort": 587,
    "FromEmail": "noreply@hunter.com",
    "ToEmails": ["admin@hunter.com"]
  },
  
  "JobScraper": {
    "CareersUrl": "https://taller.applytojob.com/apply/"
  },
  
  "FeatureManagement": {
    "AutoCandidateMatching": true
  }
}
```

### Environment Variables (Docker)

**Priority**: Environment Variables > appsettings.json

```bash
# Database
ConnectionStrings__PostgreSql="Host=postgresql;Port=5432;Database=HunterDev"

# AI Provider
AI__Provider=DeepSeek
OpenAI__ApiKey=sk-...
DeepSeek__ApiKey=sk-...

# Email
Email__SmtpHost=smtp.example.com
Email__SmtpPort=587
Email__FromEmail=noreply@hunter.com
```

---

## AI Integration Guidelines

### Supported Providers

1. **OpenAI GPT-4o** (default, more accurate)
   - Model: `gpt-4o`
   - Supports strict JSON schema
   - Higher cost (~$0.03 per analysis)

2. **DeepSeek** (cost-optimized, 90% savings)
   - Model: `deepseek-chat`
   - JSON object format (no strict schema)
   - Lower cost (~$0.003 per analysis)

### AI Service Usage

**CV Analysis:**
- Extract: Name, Email, Phone
- Match against all active job positions
- Return: Match percentage, matching skills, missing skills, referral reason
- Threshold: > 60% to be included in results

**Job Enrichment:**
- Extract from HTML: Summary, Responsibilities, Requirements
- Categorize skills: Must-Have vs Nice-To-Have
- One-time enrichment (never re-enrich existing jobs)

### Prompt Engineering Best Practices

```csharp
private string BuildPrompt(string cvText, List<JobPositionSummary> jobs)
{
    var sb = new StringBuilder();
    sb.AppendLine("You are an expert HR recruiter. Analyze this CV:");
    sb.AppendLine(cvText);
    sb.AppendLine();
    sb.AppendLine("Match against these positions:");
    
    foreach (var job in jobs)
    {
        sb.AppendLine($"Job: {job.Title} at {job.CompanyCode}");
        sb.AppendLine($"Requirements: {job.Requirements}");
    }
    
    sb.AppendLine();
    sb.AppendLine("Return structured JSON with match percentages > 60%");
    
    return sb.ToString();
}
```

**Prompt Rules:**
- Clear role definition ("You are an expert...")
- Structured format with sections
- Explicit output format requirements
- Examples for complex scenarios
- Temperature 0.2 for consistency

---

## Common Tasks & Patterns

### Adding a New Entity

1. **Create Entity** (`Domain.Core/Entities/`)
   ```csharp
   public class MyEntity : BaseEntity
   {
       public long MyEntityId { get; set; }
       public string Name { get; set; }
   }
   ```

2. **Create Mapping** (`Infra.Data/Mapping/`)
   ```csharp
   public class MyEntityMapping : IEntityTypeConfiguration<MyEntity>
   {
       public void Configure(EntityTypeBuilder<MyEntity> builder)
       {
           builder.HasKey(x => x.MyEntityId);
           builder.Property(x => x.PublicId).HasMaxLength(50).IsRequired();
           builder.HasIndex(x => x.PublicId).IsUnique();
       }
   }
   ```

3. **Add to DbContext** (`Infra.Data/Context/DataContext.cs`)
   ```csharp
   public DbSet<MyEntity> MyEntities { get; set; }
   ```

4. **Create Repository Interface** (`Domain.Core/Repositories/`)
   ```csharp
   public interface IMyEntityRepository : IBaseEntityRepository<MyEntity>
   {
       Task<MyEntity> GetByNameAsync(string name);
   }
   ```

5. **Implement Repository** (`Infra.Data/Repository/`)
   ```csharp
   public class MyEntityRepository(DataContext context) 
       : BaseEntityRepository<MyEntity>(context), IMyEntityRepository
   {
       public async Task<MyEntity> GetByNameAsync(string name)
           => await DataContext.MyEntities
               .FirstOrDefaultAsync(x => x.Name == name);
   }
   ```

6. **Register in UnitOfWork** (`Infra.Data/UnitOfWork/UnitOfWork.cs`)
   ```csharp
   public IMyEntityRepository MyEntityRepository =>
       _myEntityRepository ??= new MyEntityRepository(_context);
   ```

7. **Create Migration**
   ```bash
   dotnet ef migrations add AddMyEntity -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api
   ```

### Adding a New Use Case (CQRS)

1. **Create Request** (`Application.Core/UseCases/{Feature}/Commands/{Action}/`)
   ```csharp
   public record MyActionRequest : IRequest<MyActionResponse>
   {
       public string Name { get; init; }
   }
   ```

2. **Create Response**
   ```csharp
   public record MyActionResponse
   {
       public string PublicId { get; init; }
       public string Name { get; init; }
   }
   ```

3. **Create Validator**
   ```csharp
   public class MyActionRequestValidator : AbstractValidator<MyActionRequest>
   {
       public MyActionRequestValidator()
       {
           RuleFor(x => x.Name).NotEmpty();
       }
   }
   ```

4. **Create Handler**
   ```csharp
   public class MyActionRequestHandler(IUnitOfWork unitOfWork)
       : IRequestHandler<MyActionRequest, MyActionResponse>
   {
       public async Task<MyActionResponse> Handle(
           MyActionRequest request, 
           CancellationToken cancellationToken)
       {
           // Implementation
       }
   }
   ```

5. **Add Controller Action**
   ```csharp
   [HttpPost]
   public async Task<IActionResult> MyAction([FromBody] MyActionRequest request)
       => Ok(await Mediator.Send(request));
   ```

### Adding a Notification Handler

1. **Create Notification** (`Application.Core/UseCases/{Feature}/Notifications/{Name}/`)
   ```csharp
   public record MyNotification(MyEntity Entity) : INotification;
   ```

2. **Create Handler**
   ```csharp
   public class MyNotificationHandler(ILogger<MyNotificationHandler> logger)
       : INotificationHandler<MyNotification>
   {
       public async Task Handle(MyNotification notification, CancellationToken cancellationToken)
       {
           logger.LogInformation("Notification received");
           // Cross-cutting logic (emails, logging, etc.)
       }
   }
   ```

3. **Publish from Handler**
   ```csharp
   await mediator.Publish(new MyNotification(entity), cancellationToken);
   ```

---

## API Documentation (Swagger)

### Swagger Attributes

```csharp
/// <summary>
/// Creates a new user in the system
/// </summary>
/// <param name="request">User creation data</param>
/// <returns>Created user with PublicId</returns>
[HttpPost]
[SwaggerResponse((int)HttpStatusCode.Created, "User created", typeof(UserCreateResponse))]
[SwaggerResponse((int)HttpStatusCode.BadRequest, "Validation failed")]
[SwaggerResponse((int)HttpStatusCode.Conflict, "Email already exists")]
[SwaggerResponse((int)HttpStatusCode.Unauthorized)]
public async Task<IActionResult> Post([FromBody] UserCreateRequest request)
    => Created(await Mediator.Send(request));
```

**Rules:**
- Use XML comments for method documentation
- Use `SwaggerResponse` for all possible status codes
- Include response type for successful responses
- Document business errors (Conflict, NotFound, etc.)

---

## Deployment & CI/CD

### GitHub Actions Workflow

**build.yml** (on push to master):
1. Setup .NET 10 & Node.js 18
2. Start PostgreSQL 16.3 in Docker
3. Run EF migrations
4. Build backend & run tests with coverage
5. Build Angular frontend
6. Sonar analysis (code quality)

### Docker Compose Services

```yaml
services:
  api:          # .NET 10 API on port 5010
  frontend:     # Angular 17 on port 5200
  postgresql:   # PostgreSQL 16.3 on port 5432
```

**Default Credentials:**
- Email: `adm@hunter.com`
- Password: `123`

### Running Locally

```bash
# With Docker (recommended)
docker-compose up -d

# Without Docker
dotnet ef database update -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api
dotnet run --project src/GeminiCode.Hunter.Api

cd front-end
npm install --legacy-peer-deps
ng serve
```

---

## Troubleshooting

### Common Issues

1. **Migration Errors**
   ```bash
   # Reset database
   dotnet ef database drop -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api --force
   dotnet ef database update -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api
   ```

2. **Timestamp Issues (PostgreSQL)**
   - Always use `timestamp without time zone`
   - Store in UTC: `DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)`
   - Convert to timezone in responses if needed

3. **AI Provider Errors**
   - Check API key in `appsettings.json` or environment variables
   - Verify `AI:Provider` is set to "OpenAI" or "DeepSeek"
   - Check rate limits and quotas

4. **Memory/Timeout Issues**
   - Increase `Kestrel:Limits:RequestHeadersTimeout`
   - Use `[RequestTimeout(300000)]` for long operations (CV analysis)
   - Optimize AI prompts to reduce tokens

---

## Feature Flags

**Current Flags:**
- `AutoCandidateMatching`: Enable automatic candidate-to-job matching after CV upload

**Usage:**
```csharp
public class Handler(IFeatureManager featureManager)
{
    public async Task Handle()
    {
        if (await featureManager.IsEnabledAsync(FeatureFlags.AutoCandidateMatching))
        {
            // Feature logic
        }
    }
}
```

---

## Important Notes for Copilot

### When Creating New Code

1. **Always follow CQRS pattern** - No direct database calls in controllers
2. **Use existing patterns** - Look at similar features first
3. **Inject dependencies** - Never use `new` for services
4. **Handle exceptions** - Use custom exceptions (NotFoundException, ConflictException, etc.)
5. **Add validation** - Create FluentValidation validator for all requests
6. **Log important actions** - Inject ILogger and log key events
7. **Write tests** - Unit tests for handlers, integration tests for controllers
8. **Update migrations** - Always create migration after entity changes

### When Fixing Bugs

1. **Check existing code first** - Look for similar implementations
2. **Test locally** - Run with Docker Compose before committing
3. **Check logs** - Use Serilog output for debugging (currently disabled)
4. **Validate migrations** - Run `dotnet ef migrations add` if entities changed
5. **Run tests** - Ensure all tests pass before pushing

### When Reviewing Requirements

1. **Clarify business rules** - Ask about edge cases
2. **Check authorization** - What roles can access this feature?
3. **Consider AI cost** - Will this significantly increase AI API calls?
4. **Think about scale** - Will this work with 1000+ candidates/jobs?
5. **Plan notifications** - Should users be notified of this action?

---

## Quick Reference

### Common Commands

```bash
# Build
dotnet build Hunter.sln

# Run tests
dotnet test Hunter.sln

# Run API
dotnet run --project src/GeminiCode.Hunter.Api

# Create migration
dotnet ef migrations add MigrationName -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api

# Apply migrations
dotnet ef database update -p src/GeminiCode.Hunter.Infra.Data -s src/GeminiCode.Hunter.Api

# Docker
docker-compose up -d        # Start all services
docker-compose down         # Stop all services
docker-compose logs api     # View API logs
```

### Important URLs

- **Frontend**: http://localhost:5200
- **API**: http://localhost:5010
- **Swagger**: http://localhost:5010/swagger
- **Health Check**: http://localhost:5010/diagnostics/health

### Key Files

- `Program.cs` - Application entry point
- `Bootstrapper.cs` - Service configuration
- `appsettings.json` - Configuration
- `docker-compose.yml` - Container orchestration
- `.github/workflows/build.yml` - CI pipeline

---

## Glossary

- **CV Analysis**: AI-powered matching of candidate CV against job positions
- **Job Enrichment**: AI extraction of structured data from job postings
- **Hiring Process**: Complete lifecycle from referral to hire/reprove
- **Match Percentage**: AI-calculated compatibility score (0-100%)
- **Referral Reason**: AI explanation of why candidate is a good fit
- **PublicId**: Guid-based ID exposed to API (vs internal long ID)
- **Unit of Work**: Pattern for transactional data access
- **CQRS**: Command Query Responsibility Segregation
- **Background Service**: Scheduled task (e.g., job sync every hour)

---

## Version History

- **v1.0** - Initial release with CV analysis and hiring process
- **Current** - AI dual provider support (OpenAI + DeepSeek), job sync automation

---

*Last Updated: January 2025*
*Target Framework: .NET 10*
*Database: PostgreSQL 16.3*
