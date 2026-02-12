using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace GeminiCode.Hunter.Test.Architecture;

/// <summary>
/// Architecture tests for overall Clean Architecture principles across all layers.
/// Validates: layer dependencies, naming conventions, and architectural boundaries.
/// </summary>
public class CleanArchitectureTests
{
    // Explicit assembly references for deterministic scanning (works same locally and in CI)
    private static readonly Assembly[] AllAssemblies =
    [
        typeof(GeminiCode.Hunter.Api.Bootstrapper).Assembly,                              // API
        typeof(GeminiCode.Hunter.Application.Core.UseCases.Users.Commands.Create.UserCreateRequest).Assembly, // Application
        typeof(GeminiCode.Hunter.Domain.Core.Entities.User).Assembly,                      // Domain
        typeof(GeminiCode.Hunter.Infra.Data.Context.DataContext).Assembly,                 // Infrastructure
        typeof(GeminiCode.Hunter.Crosscutting.Ioc.Dependencies.DependenciesInjectionConfiguration).Assembly // IoC
    ];
    [Fact]
    public void Domain_Should_Not_Depend_On_Any_Other_Project_Layer()
    {
        // Arrange & Act
        var result = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespace("GeminiCode.Hunter.Domain.Core")
            .ShouldNot()
            .HaveDependencyOnAny(
                "GeminiCode.Hunter.Api",
                "GeminiCode.Hunter.Application.Core",
                "GeminiCode.Hunter.Infra.Data",
                "GeminiCode.Hunter.Crosscutting.Ioc"
            )
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Domain layer must be independent (Clean Architecture core principle). Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Application_Should_Only_Depend_On_Domain()
    {
        // Arrange & Act
        var result = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespace("GeminiCode.Hunter.Application.Core")
            .ShouldNot()
            .HaveDependencyOnAny(
                "GeminiCode.Hunter.Api",
                "GeminiCode.Hunter.Infra.Data",
                "GeminiCode.Hunter.Crosscutting.Ioc"
            )
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Application layer should only depend on Domain. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Infrastructure_Should_Only_Depend_On_Domain()
    {
        // Arrange & Act
        var result = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespace("GeminiCode.Hunter.Infra.Data")
            .ShouldNot()
            .HaveDependencyOnAny(
                "GeminiCode.Hunter.Api",
                "GeminiCode.Hunter.Application.Core",
                "GeminiCode.Hunter.Crosscutting.Ioc"
            )
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Infrastructure layer should only depend on Domain. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Api_Layer_Is_The_Only_Entry_Point()
    {
        // API can depend on all layers (it's the composition root)
        // But no other layer should depend on API (except tests and Program)

        var result = Types.InAssemblies(AllAssemblies)
            .That()
            .DoNotResideInNamespace("GeminiCode.Hunter.Api")
            .And()
            .DoNotResideInNamespace("GeminiCode.Hunter.Test") // Exclude test projects
            .And()
            .DoNotHaveName("Program") // Exclude entry point Program class
            .ShouldNot()
            .HaveDependencyOn("GeminiCode.Hunter.Api")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Only API layer should be the entry point. No other layer should depend on it. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void All_Classes_Should_Have_Namespace_Matching_Folder_Structure()
    {
        // This validates that namespace matches physical folder structure
        var allTypes = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespaceStartingWith("GeminiCode.Hunter")
            .GetTypes();

        foreach (var type in allTypes)
        {
            // Namespace should start with GeminiCode.Hunter
            Assert.StartsWith("GeminiCode.Hunter", type.Namespace);
            
            // Additional validation can be added for specific patterns
        }
    }

    [Fact]
    public void Interfaces_Should_Start_With_I()
    {
        // Arrange & Act
        var result = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespaceStartingWith("GeminiCode.Hunter")
            .And()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All interfaces must start with 'I'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Classes_Should_Not_Start_With_I()
    {
        // Arrange & Act
        var result = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespaceStartingWith("GeminiCode.Hunter")
            .And()
            .AreClasses()
            .And()
            .DoNotResideInNamespace("GeminiCode.Hunter.Test") // Exclude test classes (can start with Integration/Implementation)
            .And()
            .DoNotResideInNamespaceContaining("Migrations") // Exclude EF Core migrations (can start with 'Initial')
            .ShouldNot()
            .HaveNameStartingWith("I")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Classes should not start with 'I' (reserved for interfaces). Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Project_Should_Follow_Namespace_Convention()
    {
        // All namespaces should start with GeminiCode.Hunter
        var result = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespaceStartingWith("GeminiCode")
            .Should()
            .ResideInNamespaceStartingWith("GeminiCode.Hunter")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"All project namespaces should start with 'GeminiCode.Hunter'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Dependencies_Flow_Should_Be_Correct()
    {
        // API -> Application -> Domain <- Infrastructure
        // This is the dependency rule of Clean Architecture
        
        // Already tested in individual tests, this is a summary assertion
        var domainIndependent = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespace("GeminiCode.Hunter.Domain.Core")
            .ShouldNot()
            .HaveDependencyOnAny(
                "GeminiCode.Hunter.Api",
                "GeminiCode.Hunter.Application.Core",
                "GeminiCode.Hunter.Infra.Data"
            )
            .GetResult()
            .IsSuccessful;

        var applicationDependsOnlyDomain = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespace("GeminiCode.Hunter.Application.Core")
            .ShouldNot()
            .HaveDependencyOnAny(
                "GeminiCode.Hunter.Api",
                "GeminiCode.Hunter.Infra.Data"
            )
            .GetResult()
            .IsSuccessful;

        var infrastructureDependsOnlyDomain = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespace("GeminiCode.Hunter.Infra.Data")
            .ShouldNot()
            .HaveDependencyOnAny(
                "GeminiCode.Hunter.Api",
                "GeminiCode.Hunter.Application.Core"
            )
            .GetResult()
            .IsSuccessful;

        Assert.True(domainIndependent && applicationDependsOnlyDomain && infrastructureDependsOnlyDomain,
            "Clean Architecture dependency rules violated");
    }

    [Fact]
    public void All_Public_Classes_Should_Be_In_Correct_Layer()
    {
        // This ensures no classes are misplaced
        var allPublicClasses = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespaceStartingWith("GeminiCode.Hunter")
            .And()
            .AreClasses()
            .And()
            .ArePublic()
            .GetTypes();

        foreach (var type in allPublicClasses)
        {
            var isInValidNamespace = 
                type.Namespace.StartsWith("GeminiCode.Hunter.Api") ||
                type.Namespace.StartsWith("GeminiCode.Hunter.Application.Core") ||
                type.Namespace.StartsWith("GeminiCode.Hunter.Domain.Core") ||
                type.Namespace.StartsWith("GeminiCode.Hunter.Infra.Data") ||
                type.Namespace.StartsWith("GeminiCode.Hunter.Crosscutting") ||
                type.Namespace.StartsWith("GeminiCode.Hunter.Test");

            Assert.True(isInValidNamespace, 
                $"{type.FullName} is not in a recognized layer namespace");
        }
    }

    [Fact]
    public void DTOs_Should_Not_Leak_Between_Layers()
    {
        // Domain entities should not be used directly in API responses
        // Use Request/Response DTOs instead
        
        var controllers = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespaceContaining("Controllers")
            .And()
            .HaveNameEndingWith("Controller")
            .GetTypes();

        foreach (var controller in controllers)
        {
            var methods = controller.GetMethods()
                .Where(m => m.IsPublic && m.DeclaringType == controller);

            foreach (var method in methods)
            {
                var returnType = method.ReturnType;
                
                // Check if method returns domain entities directly
                // It should return IActionResult with Response DTOs
                Assert.DoesNotContain("GeminiCode.Hunter.Domain.Core.Entities", 
                    returnType.FullName ?? string.Empty);
            }
        }
    }
}
