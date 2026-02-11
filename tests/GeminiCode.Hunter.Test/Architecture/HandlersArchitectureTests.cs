using System.Linq;
using NetArchTest.Rules;
using Xunit;
using MediatR;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;

namespace GeminiCode.Hunter.Test.Architecture;

/// <summary>
/// Architecture tests for MediatR Handlers following CQRS pattern.
/// Validates: naming patterns, namespaces, folder structure, and handler conventions.
/// </summary>
public class HandlersArchitectureTests
{
    private const string ApplicationNamespace = "GeminiCode.Hunter.Application.Core";
    private const string UseCasesNamespace = "GeminiCode.Hunter.Application.Core.UseCases";
    
    [Fact]
    public void CommandHandlers_Should_Have_RequestHandler_Suffix()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(IUnitOfWork).Assembly)
            .That()
            .ResideInNamespaceContaining("Commands")
            .And()
            .AreClasses()
            .And()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .Should()
            .HaveNameEndingWith("RequestHandler")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All command handlers must end with 'RequestHandler'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void QueryHandlers_Should_Have_RequestHandler_Suffix()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(IUnitOfWork).Assembly)
            .That()
            .ResideInNamespaceContaining("Queries")
            .And()
            .AreClasses()
            .And()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .Should()
            .HaveNameEndingWith("RequestHandler")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All query handlers must end with 'RequestHandler'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Requests_Should_Have_Request_Suffix()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(IUnitOfWork).Assembly)
            .That()
            .ResideInNamespace(UseCasesNamespace)
            .And()
            .AreClasses()
            .And()
            .ImplementInterface(typeof(IRequest<>))
            .Should()
            .HaveNameEndingWith("Request")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All request classes must end with 'Request'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Responses_Should_Have_Response_Suffix()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(IUnitOfWork).Assembly)
            .That()
            .ResideInNamespace(UseCasesNamespace)
            .And()
            .AreClasses()
            .And()
            .HaveNameMatching(".*Response$")
            .Should()
            .HaveNameEndingWith("Response")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All response classes must end with 'Response'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Validators_Should_Have_RequestValidator_Suffix()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(IUnitOfWork).Assembly)
            .That()
            .ResideInNamespace(UseCasesNamespace)
            .And()
            .AreClasses()
            .And()
            .Inherit(typeof(FluentValidation.AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("RequestValidator")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All validators must end with 'RequestValidator'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void CommandHandlers_Should_Be_In_Commands_Namespace()
    {
        // Arrange & Act
        var commandHandlers = Types.InAssembly(typeof(IUnitOfWork).Assembly)
            .That()
            .ResideInNamespace(UseCasesNamespace)
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("RequestHandler")
            .And()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .GetTypes()
            .Where(t => !t.Namespace.Contains("Queries") && !t.Namespace.Contains("Notifications"));

        // Assert
        foreach (var handler in commandHandlers)
        {
            Assert.Contains("Commands", handler.Namespace);
        }
    }

    [Fact]
    public void QueryHandlers_Should_Be_In_Queries_Namespace()
    {
        // Arrange & Act
        var queryHandlers = Types.InAssembly(typeof(IUnitOfWork).Assembly)
            .That()
            .ResideInNamespace(UseCasesNamespace)
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("RequestHandler")
            .And()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .GetTypes()
            .Where(t => t.Namespace.Contains("Queries"));

        // Assert
        foreach (var handler in queryHandlers)
        {
            Assert.Contains("Queries", handler.Namespace);
        }
    }

    [Fact]
    public void NotificationHandlers_Should_Have_NotificationHandler_Suffix()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(IUnitOfWork).Assembly)
            .That()
            .ResideInNamespaceContaining("Notifications")
            .And()
            .AreClasses()
            .And()
            .ImplementInterface(typeof(INotificationHandler<>))
            .Should()
            .HaveNameEndingWith("NotificationHandler")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All notification handlers must end with 'NotificationHandler'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Notifications_Should_Have_Notification_Suffix()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(IUnitOfWork).Assembly)
            .That()
            .ResideInNamespaceContaining("Notifications")
            .And()
            .AreClasses()
            .And()
            .ImplementInterface(typeof(INotification))
            .Should()
            .HaveNameEndingWith("Notification")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All notification classes must end with 'Notification'. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Handlers_Should_Not_Depend_On_Infrastructure()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(IUnitOfWork).Assembly)
            .That()
            .ResideInNamespace(UseCasesNamespace)
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("Handler")
            .ShouldNot()
            .HaveDependencyOn("GeminiCode.Hunter.Infra.Data")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Handlers should not depend directly on Infrastructure implementation. Use IUnitOfWork instead. Violations: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void CommandHandlers_Should_Use_IUnitOfWork()
    {
        // Arrange
        var commandHandlers = Types.InAssembly(typeof(IUnitOfWork).Assembly)
            .That()
            .ResideInNamespaceContaining("Commands")
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("RequestHandler")
            .GetTypes();

        // Act & Assert
        foreach (var handler in commandHandlers)
        {
            var constructors = handler.GetConstructors();
            var hasUnitOfWork = constructors.Any(c => 
                c.GetParameters().Any(p => 
                    p.ParameterType.Name == "IUnitOfWork"));

            Assert.True(hasUnitOfWork, 
                $"Command handler {handler.Name} should have IUnitOfWork as constructor dependency");
        }
    }

    [Fact]
    public void Handlers_Namespace_Should_Match_Feature_Structure()
    {
        // Arrange & Act
        var handlers = Types.InAssembly(typeof(IUnitOfWork).Assembly)
            .That()
            .ResideInNamespace(UseCasesNamespace)
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("RequestHandler")
            .GetTypes();

        // Assert - Each handler should be in: UseCases.{Feature}.Commands|Queries.{Action}
        foreach (var handler in handlers)
        {
            var namespacePattern = @"GeminiCode\.Hunter\.Application\.Core\.UseCases\.\w+\.(Commands|Queries)\.\w+";
            Assert.Matches(namespacePattern, handler.Namespace);
        }
    }

    [Fact]
    public void Request_And_Handler_Should_Be_In_Same_Namespace()
    {
        // Arrange
        var handlers = Types.InAssembly(typeof(IUnitOfWork).Assembly)
            .That()
            .ResideInNamespace(UseCasesNamespace)
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("RequestHandler")
            .GetTypes();

        // Act & Assert
        foreach (var handler in handlers)
        {
            var requestName = handler.Name.Replace("Handler", "");
            var expectedRequest = Types.InAssembly(typeof(IUnitOfWork).Assembly)
                .That()
                .ResideInNamespace(handler.Namespace)
                .And()
                .HaveName(requestName)
                .GetTypes()
                .FirstOrDefault();

            Assert.NotNull(expectedRequest);
            Assert.Equal(handler.Namespace, expectedRequest.Namespace);
        }
    }

    [Fact]
    public void Validator_Should_Be_In_Same_Namespace_As_Request()
    {
        // Arrange
        var validators = Types.InAssembly(typeof(IUnitOfWork).Assembly)
            .That()
            .ResideInNamespace(UseCasesNamespace)
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("RequestValidator")
            .GetTypes();

        // Act & Assert
        foreach (var validator in validators)
        {
            var requestName = validator.Name.Replace("Validator", "");
            var expectedRequest = Types.InAssembly(typeof(IUnitOfWork).Assembly)
                .That()
                .ResideInNamespace(validator.Namespace)
                .And()
                .HaveName(requestName)
                .GetTypes()
                .FirstOrDefault();

            Assert.NotNull(expectedRequest);
            Assert.Equal(validator.Namespace, expectedRequest.Namespace);
        }
    }
}
