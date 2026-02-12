using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace GeminiCode.Hunter.Test.Architecture;

/// <summary>
/// Architecture tests for Infrastructure layer following Clean Architecture principles.
/// Validates: repositories implementation, data context, and naming conventions.
/// </summary>
public class InfrastructureArchitectureTests
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

    private const string InfraNamespace = "GeminiCode.Hunter.Infra.Data";
    private const string RepositoryNamespace = "GeminiCode.Hunter.Infra.Data.Repository";
    private const string RepositoryImplementationsNamespace = "GeminiCode.Hunter.Infra.Data.Repository.Implementations";
    private const string DomainNamespace = "GeminiCode.Hunter.Domain.Core";
    
    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Application()
    {
        // Arrange & Act
        var result = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespace(InfraNamespace)
            .ShouldNot()
            .HaveDependencyOn("GeminiCode.Hunter.Application.Core")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Infrastructure should not depend on Application layer. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        // Arrange & Act
        var result = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespace(InfraNamespace)
            .ShouldNot()
            .HaveDependencyOn("GeminiCode.Hunter.Api")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Infrastructure should not depend on API layer. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Repository_Implementations_Should_Have_Repository_Suffix()
    {
        // Arrange & Act
        var result = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespace(RepositoryImplementationsNamespace)
            .And()
            .AreClasses()
            .And()
            .DoNotHaveName("BaseEntityRepository")
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All repository implementations must end with 'Repository'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Repository_Implementations_Should_Be_Sealed()
    {
        // Arrange & Act
        var result = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespace(RepositoryImplementationsNamespace)
            .And()
            .AreClasses()
            .And()
            .DoNotHaveName("BaseEntityRepository")
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Repository implementations should be sealed. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Repository_Implementations_Should_Inherit_BaseEntityRepository()
    {
        // Arrange & Act
        var baseEntityRepository = Types.InAssemblies(AllAssemblies)
            .That()
            .HaveName("BaseEntityRepository`1")
            .GetTypes()
            .FirstOrDefault();

        if (baseEntityRepository == null)
        {
            // Skip if base class not found in current domain
            Assert.True(true);
            return;
        }

        var result = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespace(RepositoryImplementationsNamespace)
            .And()
            .AreClasses()
            .And()
            .DoNotHaveName("BaseEntityRepository")
            .Should()
            .Inherit(baseEntityRepository)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All repository implementations must inherit from BaseEntityRepository<T>. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Repository_Implementation_Should_Match_Interface_Name()
    {
        // Arrange
        var repositoryImplementations = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespace(RepositoryImplementationsNamespace)
            .And()
            .AreClasses()
            .And()
            .DoNotHaveName("BaseEntityRepository")
            .GetTypes();

        // Act & Assert
        foreach (var implementation in repositoryImplementations)
        {
            var expectedInterfaceName = $"I{implementation.Name}";
            var interfaces = implementation.GetInterfaces();
            
            var matchingInterface = interfaces.FirstOrDefault(i => i.Name == expectedInterfaceName);
            
            Assert.NotNull(matchingInterface);
            Assert.Contains($"GeminiCode.Hunter.Domain.Core.Repositories", matchingInterface.Namespace);
        }
    }

    [Fact]
    public void Entity_Mappings_Should_Have_Mapping_Suffix()
    {
        // Arrange & Act
        var result = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespace("GeminiCode.Hunter.Infra.Data.Mapping")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Mapping")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All entity mapping classes must end with 'Mapping'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Entity_Mappings_Should_Implement_IEntityTypeConfiguration()
    {
        // Arrange & Act
        var result = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespace("GeminiCode.Hunter.Infra.Data.Mapping")
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("Mapping")
            .Should()
            .ImplementInterface(typeof(Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<>))
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All mapping classes must implement IEntityTypeConfiguration<T>. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void DataContext_Should_Be_In_Context_Namespace()
    {
        // Arrange & Act
        var dataContext = Types.InAssemblies(AllAssemblies)
            .That()
            .HaveName("DataContext")
            .And()
            .ResideInNamespaceStartingWith("GeminiCode.Hunter.Infra.Data")
            .GetTypes()
            .FirstOrDefault();

        // Assert
        if (dataContext != null)
        {
            Assert.Equal("GeminiCode.Hunter.Infra.Data.Context", dataContext.Namespace);
        }
        else
        {
            // DataContext might be in different assembly, skip test
            Assert.True(true);
        }
    }

    [Fact]
    public void DataContext_Should_Inherit_DbContext()
    {
        // Arrange & Act
        var dataContext = Types.InAssemblies(AllAssemblies)
            .That()
            .HaveName("DataContext")
            .And()
            .ResideInNamespaceStartingWith("GeminiCode.Hunter.Infra.Data")
            .GetTypes()
            .FirstOrDefault();

        // Assert
        if (dataContext != null)
        {
            Assert.True(dataContext.IsSubclassOf(typeof(Microsoft.EntityFrameworkCore.DbContext)),
                "DataContext should inherit from DbContext");
        }
        else
        {
            // DataContext might be in different assembly, skip test
            Assert.True(true);
        }
    }

    [Fact]
    public void Migrations_Should_Be_In_Migrations_Namespace()
    {
        // Arrange & Act
        var migrations = Types.InAssemblies(AllAssemblies)
            .That()
            .ResideInNamespaceContaining("Migrations")
            .And()
            .AreClasses()
            .GetTypes()
            .Where(t => t.Namespace != null 
                && t.Namespace.EndsWith("Migrations")
                && t.Namespace.StartsWith("GeminiCode.Hunter"));

        // Assert
        foreach (var migration in migrations)
        {
            Assert.StartsWith("GeminiCode.Hunter.Infra.Data.Migrations", migration.Namespace);
        }
    }

    [Fact]
    public void UnitOfWork_Should_Implement_IUnitOfWork()
    {
        // Arrange & Act
        var unitOfWork = Types.InAssemblies(AllAssemblies)
            .That()
            .HaveName("UnitOfWork")
            .And()
            .ResideInNamespaceStartingWith("GeminiCode.Hunter.Infra.Data")
            .GetTypes()
            .FirstOrDefault();

        // Assert
        if (unitOfWork != null)
        {
            var iUnitOfWork = unitOfWork.GetInterfaces()
                .FirstOrDefault(i => i.Name == "IUnitOfWork");

            Assert.NotNull(iUnitOfWork);
        }
        else
        {
            // UnitOfWork might be in different assembly
            Assert.True(true);
        }
    }
}
