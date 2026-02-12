using System.Linq;
using GeminiCode.Hunter.Api.Attributes;
using Microsoft.AspNetCore.Authorization;
using NetArchTest.Rules;
using Xunit;

namespace GeminiCode.Hunter.Test.Architecture;

/// <summary>
/// Architecture tests for API Controllers following Hunter's Clean Architecture conventions.
/// Validates: naming patterns, namespaces, inheritance, dependencies, and folder structure.
/// </summary>
public class ControllersArchitectureTests
{
    private const string ControllersNamespace = "GeminiCode.Hunter.Api.Controllers.V1";
    
    [Fact]
    public void Controllers_Should_Be_In_V1_Namespace()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace("GeminiCode.Hunter.Api.Controllers")
            .And()
            .AreClasses()
            .And()
            .DoNotResideInNamespaceContaining("Base") // Exclude base controllers
            .Should()
            .ResideInNamespace(ControllersNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"All controllers must be in {ControllersNamespace}. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Controllers_Should_Have_Controller_Suffix()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace(ControllersNamespace)
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Controller")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All controller classes must end with 'Controller'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Controllers_Should_Inherit_From_ApiController()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace(ControllersNamespace)
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("Controller")
            .And()
            .DoNotHaveName("ApiController")
            .Should()
            .Inherit(Types.InAssembly(typeof(Program).Assembly)
                .That()
                .HaveName("ApiController")
                .GetTypes()
                .First())
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All controllers must inherit from ApiController. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Controllers_Should_Not_Have_Dependencies_On_Infrastructure()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace(ControllersNamespace)
            .And()
            .AreClasses()
            .ShouldNot()
            .HaveDependencyOn("GeminiCode.Hunter.Infra.Data")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Controllers should not depend on Infrastructure layer. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Controllers_Should_Only_Depend_On_Application_And_Domain()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace(ControllersNamespace)
            .And()
            .AreClasses()
            .ShouldNot()
            .HaveDependencyOnAny(
                "GeminiCode.Hunter.Infra.Data",
                "GeminiCode.Hunter.Crosscutting.Ioc"
            )
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Controllers should only depend on Application and Domain layers. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Controllers_Should_Have_Authorize_Attribute()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace(ControllersNamespace)
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("Controller")
            .And()
            .DoNotHaveName("ApiController")
            .And()
            .DoNotHaveName("AuthController")
            .And()
            .DoNotHaveName("UsersController")
            .Should()
            .HaveCustomAttribute(typeof(AuthorizeForAttribute))
            .Or()
            .HaveCustomAttribute(typeof(AuthorizeAttribute))
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All controllers should have [Authorize] attribute. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Controllers_Should_Be_Sealed()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace(ControllersNamespace)
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("Controller")
            .And()
            .DoNotHaveName("ApiController")
            .Should()
            .BeSealed()
            .GetResult();

        // Assert - Warning: This might fail if controllers are not sealed, adjust if needed
        Assert.True(result.IsSuccessful,
            $"Controllers should be sealed (not inheritable). Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }
}
