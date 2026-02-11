# Architecture Tests

This folder contains architecture tests that validate the Hunter application follows Clean Architecture principles, CQRS pattern, and consistent naming conventions.

## Purpose

Architecture tests ensure that:
- Code follows established patterns and conventions
- Layer dependencies are correct (Clean Architecture)
- Naming conventions are consistent
- New code adheres to the architectural rules

## Test Files

### 1. CleanArchitectureTests.cs
Validates overall Clean Architecture principles:
- **Domain Independence**: Domain layer has no dependencies on other layers
- **Dependency Flow**: API → Application → Domain ← Infrastructure
- **Layer Boundaries**: No cross-layer violations
- **DTOs**: Domain entities don't leak to API layer

**Key Rules:**
- Domain is completely independent
- Application depends only on Domain
- Infrastructure depends only on Domain
- API is the composition root

### 2. ControllersArchitectureTests.cs
Validates API Controllers:
- **Namespace**: All controllers in `GeminiCode.Hunter.Api.Controllers.V1`
- **Naming**: Must end with `Controller`
- **Inheritance**: Must inherit from `ApiController`
- **Authorization**: Must have `[Authorize]` attribute
- **Dependencies**: Should not depend on Infrastructure

**Example Pattern:**
```csharp
[ApiVersion(1.0)]
[Route("[controller]")]
[Authorize]
public sealed class UsersController(IMediator mediator) : ApiController(mediator)
```

### 3. HandlersArchitectureTests.cs
Validates CQRS Handlers (Commands, Queries, Notifications):
- **Request Suffix**: All requests end with `Request`
- **Handler Suffix**: All handlers end with `RequestHandler`
- **Validator Suffix**: All validators end with `RequestValidator`
- **Response Suffix**: All responses end with `Response`
- **Folder Structure**: `UseCases.{Feature}.Commands|Queries.{Action}`
- **Co-location**: Request, Handler, Validator, Response in same namespace

**Example Pattern:**
```
UseCases/
  Users/
    Commands/
      Create/
        UserCreateRequest.cs
        UserCreateRequestHandler.cs
        UserCreateRequestValidator.cs
        UserCreateResponse.cs
```

### 4. DomainArchitectureTests.cs
Validates Domain layer:
- **Independence**: No dependencies on Application, Infrastructure, or API
- **Repository Interfaces**: Start with `I`, end with `Repository`
- **Repository Inheritance**: All inherit from `IBaseEntityRepository<T>`
- **Exceptions**: End with `Exception`, inherit from `Exception`
- **Service Interfaces**: Start with `I`

**Example Pattern:**
```csharp
public interface IUserRepository : IBaseEntityRepository<User>
{
    Task<User> FindByEmailAsync(string email);
}
```

### 5. InfrastructureArchitectureTests.cs
Validates Infrastructure layer:
- **Dependencies**: Only depends on Domain (not Application or API)
- **Repository Implementation**: End with `Repository`, sealed, inherit `BaseEntityRepository<T>`
- **Naming Match**: Implementation matches interface (e.g., `UserRepository` implements `IUserRepository`)
- **Entity Mappings**: End with `Mapping`, implement `IEntityTypeConfiguration<T>`
- **DataContext**: Inherits from `DbContext`

**Example Pattern:**
```csharp
public sealed class UserRepository(DataContext context) 
    : BaseEntityRepository<User>(context), IUserRepository
{
    // Implementation
}
```

### 6. ApplicationArchitectureTests.cs
Validates Application layer:
- **Dependencies**: Only depends on Domain (not Infrastructure or API)
- **Use Cases Structure**: `UseCases.{Feature}.Commands|Queries|Notifications`
- **Service Implementation**: End with `Service`, implement interface from Domain
- **Background Services**: End with `BackgroundService`, inherit from `BackgroundService`
- **IUnitOfWork Usage**: Handlers use `IUnitOfWork`, not `DbContext` directly

**Example Pattern:**
```csharp
public class UserCreateRequestHandler(
    IUnitOfWork unitOfWork,
    IMediator mediator) : IRequestHandler<UserCreateRequest, UserCreateResponse>
{
    // Implementation
}
```

### 7. NamingConventionsTests.cs
Validates naming conventions:
- **PascalCase**: All types and namespaces
- **Command Verbs**: Create, Update, Delete, Change, etc.
- **Query Verbs**: GetById, GetPaginated, GetAll
- **Boolean Properties**: Start with Is/Has/Can/Should
- **Notifications**: Past tense (Created, Updated, Deleted)
- **Name Matching**: Handler, Validator, Response match Request name

**Example Pattern:**
```
UserCreateRequest → UserCreateRequestHandler
UserCreateRequest → UserCreateRequestValidator
UserCreateRequest → UserCreateResponse
```

## Running Tests

### Run All Architecture Tests
```bash
dotnet test --filter "FullyQualifiedName~Architecture"
```

### Run Specific Test File
```bash
dotnet test --filter "FullyQualifiedName~CleanArchitectureTests"
dotnet test --filter "FullyQualifiedName~HandlersArchitectureTests"
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~Domain_Should_Not_Depend_On_Any_Other_Project_Layer"
```

## CI/CD Integration

These tests are automatically run as part of the CI/CD pipeline (`.github/workflows/build.yml`). All tests must pass before merging to `master`.

## Benefits

1. **Prevent Architectural Drift**: Catches violations early
2. **Onboarding**: New developers learn patterns from test failures
3. **Refactoring Safety**: Ensures refactoring doesn't break architectural rules
4. **Documentation**: Tests serve as executable documentation
5. **Consistency**: Enforces consistent naming and structure

## Common Violations & Fixes

### Violation: "Domain should not depend on Application layer"
**Fix**: Move interface to Domain, implementation to Application/Infrastructure

### Violation: "Handler must end with 'RequestHandler'"
**Fix**: Rename handler to match pattern (e.g., `UserCreateRequestHandler`)

### Violation: "Request and Handler should be in same namespace"
**Fix**: Move files to correct folder structure

### Violation: "Repository must implement IBaseEntityRepository"
**Fix**: 
```csharp
public interface IUserRepository : IBaseEntityRepository<User>
```

### Violation: "Controller should not depend on Infrastructure"
**Fix**: Use `IMediator` and send requests, don't inject repositories

## Best Practices

1. **Run tests locally** before committing
2. **Check test failures** to understand architectural rules
3. **Follow established patterns** from Users CRUD example
4. **Ask before breaking rules** - some violations may need architectural discussion

## Technology

- **NetArchTest.Rules**: Library for architecture testing in .NET
- **xUnit**: Test framework
- **Reflection**: Analyzes code structure at runtime

## References

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [NetArchTest.Rules](https://github.com/BenMorris/NetArchTest)
- [Hunter Custom Instructions](.github/copilot-instructions.md)
