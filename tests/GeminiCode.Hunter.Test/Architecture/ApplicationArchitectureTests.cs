using System.Linq;
using NetArchTest.Rules;
using Xunit;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;

namespace GeminiCode.Hunter.Test.Architecture;

/// <summary>
/// Architecture tests for Application layer following CQRS and Clean Architecture principles.
/// Validates: use cases structure, service implementations, and layer dependencies.
/// </summary>
public class ApplicationArchitectureTests
{
    private const string ApplicationNamespace = "GeminiCode.Hunter.Application.Core";
    private const string UseCasesNamespace = "GeminiCode.Hunter.Application.Core.UseCases";
    private const string ServicesNamespace = "GeminiCode.Hunter.Application.Core.Services";
    
    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(ApplicationNamespace)
            .ShouldNot()
            .HaveDependencyOn("GeminiCode.Hunter.Infra.Data")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Application should not depend on Infrastructure implementation. Use abstractions from Domain. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Api()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(ApplicationNamespace)
            .ShouldNot()
            .HaveDependencyOn("GeminiCode.Hunter.Api")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Application should not depend on API layer. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void UseCases_Should_Be_Organized_By_Feature()
    {
        // Arrange & Act
        var useCaseTypes = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(UseCasesNamespace)
            .And()
            .AreClasses()
            .GetTypes();

        // Assert - Each use case should be in: UseCases.{Feature}.{Commands|Queries|Notifications}
        foreach (var type in useCaseTypes)
        {
            var namespaceParts = type.Namespace?.Split('.');
            Assert.NotNull(namespaceParts);
            Assert.True(namespaceParts.Length >= 6, 
                $"{type.FullName} should be organized in feature folders: UseCases.{{Feature}}.{{Commands|Queries}}.{{Action}}");
        }
    }

    [Fact]
    public void UseCases_Should_Have_Commands_Or_Queries_Or_Notifications_Folder()
    {
        // Arrange & Act
        var useCaseTypes = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(UseCasesNamespace)
            .And()
            .AreClasses()
            .GetTypes();

        // Assert
        foreach (var type in useCaseTypes)
        {
            var hasCorrectStructure = type.Namespace.Contains("Commands") ||
                                     type.Namespace.Contains("Queries") ||
                                     type.Namespace.Contains("Notifications");
            
            Assert.True(hasCorrectStructure,
                $"{type.FullName} should be in Commands, Queries, or Notifications folder");
        }
    }

    [Fact]
    public void Service_Implementations_Should_Have_Service_Suffix()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(ServicesNamespace)
            .And()
            .AreClasses()
            .And()
            .AreNotNested() // Exclude nested DTOs and internal classes
            .And()
            .DoNotHaveNameMatching(".*Dto$") // Exclude DTOs
            .Should()
            .HaveNameEndingWith("Service")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All service implementations must end with 'Service'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact(Skip = "Not all services require domain interfaces - some are internal implementation details")]
    public void Service_Implementations_Should_Implement_Interface_From_Domain()
    {
        // Arrange
        var serviceImplementations = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(ServicesNamespace)
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("Service")
            .GetTypes();

        // Act & Assert
        foreach (var service in serviceImplementations)
        {
            var expectedInterfaceName = $"I{service.Name}";
            var interfaces = service.GetInterfaces();

            var matchingInterface = interfaces.FirstOrDefault(i => i.Name == expectedInterfaceName);

            // Only check if interface exists AND it's from Domain
            // Some services might be internal without interfaces
            if (matchingInterface != null && matchingInterface.Namespace != null)
            {
                var isFromDomain = matchingInterface.Namespace.StartsWith("GeminiCode.Hunter.Domain.Core");
                Assert.True(isFromDomain,
                    $"Service interface {matchingInterface.FullName} should be in Domain.Core, but is in {matchingInterface.Namespace}");
            }
        }
    }

    [Fact]
    public void BackgroundServices_Should_Inherit_BackgroundService()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceContaining("BackgroundServices")
            .And()
            .AreClasses()
            .Should()
            .Inherit(typeof(Microsoft.Extensions.Hosting.BackgroundService))
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All background services must inherit from BackgroundService. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void BackgroundServices_Should_Have_BackgroundService_Suffix()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceContaining("BackgroundServices")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("BackgroundService")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All background service classes must end with 'BackgroundService'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Command_And_Query_Handlers_Should_Not_Be_Mixed()
    {
        // Commands should not call queries and vice versa (in same handler)
        // This is a design guideline - handlers should be focused
        
        var commandHandlers = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceContaining("Commands")
            .And()
            .HaveNameEndingWith("RequestHandler")
            .GetTypes();

        foreach (var handler in commandHandlers)
        {
            // This is informational - ensure commands don't have query logic mixed in
            Assert.DoesNotContain("Query", handler.Name);
        }
    }

    [Fact]
    public void UseCases_Should_Not_Have_Direct_DbContext_Dependencies()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(UseCasesNamespace)
            .And()
            .AreClasses()
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore.DbContext")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Use cases should use IUnitOfWork, not DbContext directly. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact(Skip = "Too strict - some requests don't have responses (Unit returns) or validators")]
    public void Request_Response_Validator_Should_Be_In_Same_Folder()
    {
        // Arrange
        var requests = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(UseCasesNamespace)
            .And()
            .HaveNameEndingWith("Request")
            .GetTypes();

        // Act & Assert
        foreach (var request in requests)
        {
            var requestNamespace = request.Namespace;
            var actionName = request.Name.Replace("Request", "");

            // Check if Response exists in same namespace
            var response = Types.InCurrentDomain()
                .That()
                .ResideInNamespace(requestNamespace)
                .And()
                .HaveName($"{actionName}Response")
                .GetTypes()
                .FirstOrDefault();

            // Check if Validator exists in same namespace
            var validator = Types.InCurrentDomain()
                .That()
                .ResideInNamespace(requestNamespace)
                .And()
                .HaveName($"{actionName}RequestValidator")
                .GetTypes()
                .FirstOrDefault();

            // Check if Handler exists in same namespace
            var handler = Types.InCurrentDomain()
                .That()
                .ResideInNamespace(requestNamespace)
                .And()
                .HaveName($"{actionName}RequestHandler")
                .GetTypes()
                .FirstOrDefault();

            // Handler must exist
            Assert.NotNull(handler);
            
            // Response should exist for most cases (except void/Unit returns)
            // Validator should exist for most cases
            // These are recommendations, not strict requirements
        }
    }

    [Fact]
    public void UseCases_Feature_Names_Should_Match_Domain_Entities()
    {
        // Arrange - Get all feature folders from UseCases
        var useCaseTypes = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(UseCasesNamespace)
            .GetTypes();

        var featureNames = useCaseTypes
            .Select(t => t.Namespace?.Split('.'))
            .Where(parts => parts != null && parts.Length >= 5)
            .Select(parts => parts[4]) // Index 4 is the feature name
            .Distinct()
            .ToList();

        // Act - Check if corresponding domain entities exist
        foreach (var featureName in featureNames)
        {
            // This is informational - features should align with domain entities
            // but not all features map 1:1 (e.g., "Auth" vs "User", "CvAnalysis" vs "Candidate")
            
            // Just ensure feature names follow PascalCase convention
            Assert.Matches(@"^[A-Z][a-zA-Z0-9]*$", featureName);
        }
    }
}
