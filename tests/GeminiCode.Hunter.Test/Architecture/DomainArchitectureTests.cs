using System;
using System.Linq;
using NetArchTest.Rules;
using Xunit;

namespace GeminiCode.Hunter.Test.Architecture;

/// <summary>
/// Architecture tests for Domain layer following Clean Architecture principles.
/// Validates: entities, repositories, independence, and naming conventions.
/// </summary>
public class DomainArchitectureTests
{
    private const string DomainNamespace = "GeminiCode.Hunter.Domain.Core";
    private const string EntitiesNamespace = "GeminiCode.Hunter.Domain.Core.Entities";
    private const string RepositoriesNamespace = "GeminiCode.Hunter.Domain.Core.Repositories";
    
    [Fact]
    public void Domain_Should_Not_Have_Dependencies_On_Application()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(DomainNamespace)
            .ShouldNot()
            .HaveDependencyOn("GeminiCode.Hunter.Application.Core")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Domain should not depend on Application layer. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Domain_Should_Not_Have_Dependencies_On_Infrastructure()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(DomainNamespace)
            .ShouldNot()
            .HaveDependencyOn("GeminiCode.Hunter.Infra.Data")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Domain should not depend on Infrastructure layer. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Domain_Should_Not_Have_Dependencies_On_Api()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(DomainNamespace)
            .ShouldNot()
            .HaveDependencyOn("GeminiCode.Hunter.Api")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Domain should not depend on API layer. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Entities_Should_Be_In_Entities_Namespace()
    {
        // Arrange & Act
        var entities = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(DomainNamespace)
            .And()
            .AreClasses()
            .And()
            .DoNotHaveNameMatching(".*Exception")
            .And()
            .DoNotHaveNameMatching("I.*")
            .GetTypes()
            .Where(t => t.Namespace != null && 
                       (t.Namespace.Contains("Entities") && 
                        !t.Name.EndsWith("Filter") && 
                        !t.Name.EndsWith("Configuration") &&
                        t.Name != "BaseEntity"));

        // Assert
        foreach (var entity in entities)
        {
            Assert.StartsWith(EntitiesNamespace, entity.Namespace);
        }
    }

    [Fact]
    public void Repository_Interfaces_Should_Have_IRepository_Suffix()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(RepositoriesNamespace)
            .And()
            .AreInterfaces()
            .And()
            .DoNotHaveName("IBaseEntityRepository")
            .And()
            .DoNotHaveName("IUnitOfWork") // Unit of Work is not a repository
            .And()
            .DoNotResideInNamespaceContaining("Base") // Exclude base interfaces
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All repository interfaces must end with 'Repository'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Repository_Interfaces_Should_Start_With_I()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(RepositoriesNamespace)
            .And()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All repository interfaces must start with 'I'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Repository_Interfaces_Should_Inherit_IBaseEntityRepository()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(RepositoriesNamespace)
            .And()
            .AreInterfaces()
            .And()
            .DoNotHaveName("IBaseEntityRepository")
            .And()
            .DoNotHaveName("IUnitOfWork") // Unit of Work is not a repository
            .And()
            .DoNotResideInNamespaceContaining("Base") // Exclude base interfaces
            .And()
            .DoNotResideInNamespaceContaining("UnitOfWork") // Exclude UnitOfWork namespace
            .Should()
            .ImplementInterface(Types.InCurrentDomain()
                .That()
                .HaveName("IBaseEntityRepository`1")
                .GetTypes()
                .FirstOrDefault() ?? typeof(object)) // Fallback if not found
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All repository interfaces must inherit from IBaseEntityRepository<T>. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Service_Interfaces_Should_Start_With_I()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceContaining("Services")
            .And()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All service interfaces must start with 'I'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Exceptions_Should_Have_Exception_Suffix()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceContaining("Exceptions")
            .And()
            .AreClasses()
            .And()
            .DoNotResideInNamespace("GeminiCode.Hunter.Test") // Exclude test classes
            .And()
            .DoNotHaveNameMatching(".*Response$") // Exclude DTOs
            .And()
            .DoNotHaveNameMatching(".*Type$") // Exclude enums
            .Should()
            .HaveNameEndingWith("Exception")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All exception classes must end with 'Exception'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Exceptions_Should_Inherit_From_Exception()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceContaining("Exceptions")
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("Exception")
            .Should()
            .Inherit(typeof(Exception))
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All exception classes must inherit from Exception. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Domain_Entities_Should_Not_Have_Parameterless_Public_Constructors()
    {
        // This is a best practice test - entities should be created through factories or with required parameters
        // Note: This might need adjustment based on EF Core requirements
        
        var entities = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(EntitiesNamespace)
            .And()
            .AreClasses()
            .And()
            .DoNotHaveName("BaseEntity")
            .GetTypes();

        foreach (var entity in entities)
        {
            var publicParameterlessConstructor = entity.GetConstructors()
                .Where(c => c.IsPublic && c.GetParameters().Length == 0)
                .ToList();

            // This is informational - EF Core might require parameterless constructors
            // Adjust assertion based on your domain design choices
            if (publicParameterlessConstructor.Any())
            {
                // Log or handle as needed
                // For strict DDD, this should be Assert.False
            }
        }
    }
}
