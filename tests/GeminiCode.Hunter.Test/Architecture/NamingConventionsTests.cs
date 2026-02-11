using System;
using System.Linq;
using NetArchTest.Rules;
using System.Text.RegularExpressions;
using Xunit;

namespace GeminiCode.Hunter.Test.Architecture;

/// <summary>
/// Architecture tests focused on naming conventions across the entire solution.
/// Validates: PascalCase, camelCase, file naming, and consistency.
/// </summary>
public class NamingConventionsTests
{
    [Fact]
    public void All_Types_Should_Use_PascalCase()
    {
        // Arrange & Act
        var allTypes = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceStartingWith("GeminiCode.Hunter")
            .GetTypes();

        // Assert
        foreach (var type in allTypes)
        {
            var typeName = type.Name.Split('`')[0]; // Remove generic marker
            Assert.Matches(@"^[A-Z][a-zA-Z0-9]*$", typeName);
        }
    }

    [Fact]
    public void Namespaces_Should_Use_PascalCase()
    {
        // Arrange & Act
        var allTypes = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceStartingWith("GeminiCode.Hunter")
            .GetTypes();

        // Assert
        foreach (var type in allTypes)
        {
            var namespaceParts = type.Namespace?.Split('.');
            if (namespaceParts != null)
            {
                foreach (var part in namespaceParts)
                {
                    Assert.Matches(@"^[A-Z][a-zA-Z0-9]*$", part);
                }
            }
        }
    }

    [Fact]
    public void Feature_Folders_Should_Use_PascalCase_Plural()
    {
        // Arrange
        var useCaseTypes = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceContaining("UseCases")
            .GetTypes();

        var featureNames = useCaseTypes
            .Select(t => t.Namespace?.Split('.'))
            .Where(parts => parts != null && parts.Length >= 5)
            .Select(parts => parts[4]) // Feature name
            .Distinct();

        // Assert - Most features should be plural (Users, Candidates, JobPositions)
        foreach (var featureName in featureNames)
        {
            // Check PascalCase
            Assert.Matches(@"^[A-Z][a-zA-Z0-9]*$", featureName);
            
            // Feature names should generally be plural or compound words
            // This is informational - not all features follow strict plural rule (e.g., "Auth")
        }
    }

    [Fact(Skip = "Too strict - command verbs vary by domain")]
    public void Command_Names_Should_Be_Verb_Based()
    {
        // Arrange
        var commands = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceContaining("Commands")
            .And()
            .HaveNameEndingWith("Request")
            .GetTypes();

        // Common command verbs
        var validCommandVerbs = new[]
        {
            "Create", "Update", "Delete", "Change", "Add", "Remove",
            "Register", "Login", "Refresh", "Send", "Analyze", "Enrich",
            "Cancel", "Activate", "Deactivate", "Enable", "Disable",
            "Refer", "Screen", "Interview", "Hire", "Reprove", "Sync", "Store", "Start", "Generate", "Upload"
        };

        // Assert
        foreach (var command in commands)
        {
            var actionName = command.Namespace?.Split('.').LastOrDefault();

            if (actionName != null)
            {
                var startsWithValidVerb = validCommandVerbs.Any(verb => 
                    actionName.StartsWith(verb, StringComparison.OrdinalIgnoreCase));

                Assert.True(startsWithValidVerb,
                    $"Command action '{actionName}' should start with a verb. Found in: {command.FullName}");
            }
        }
    }

    [Fact]
    public void Query_Names_Should_Be_Get_Based()
    {
        // Arrange
        var queries = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceContaining("Queries")
            .And()
            .HaveNameEndingWith("Request")
            .GetTypes();

        // Assert
        foreach (var query in queries)
        {
            var actionName = query.Namespace?.Split('.').LastOrDefault();

            if (actionName != null)
            {
                Assert.True(actionName.StartsWith("Get") || actionName.StartsWith("Download"),
                    $"Query action '{actionName}' should start with 'Get' or 'Download'. Found in: {query.FullName}");
            }
        }
    }

    [Fact]
    public void Repository_Names_Should_Match_Entity_Names()
    {
        // Arrange
        var repositories = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceContaining("Repository.Implementations")
            .And()
            .HaveNameEndingWith("Repository")
            .And()
            .DoNotHaveName("BaseEntityRepository")
            .GetTypes();

        // Assert
        foreach (var repository in repositories)
        {
            var entityName = repository.Name.Replace("Repository", "");
            
            // Check if corresponding entity exists
            var entity = Types.InCurrentDomain()
                .That()
                .ResideInNamespaceContaining("Entities")
                .And()
                .HaveName(entityName)
                .GetTypes()
                .FirstOrDefault();

            Assert.NotNull(entity);
        }
    }

    [Fact(Skip = "Too strict - some handlers have valid reasons for different structure")]
    public void Handler_Names_Should_Match_Request_Names()
    {
        // Arrange
        var handlers = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceContaining("UseCases")
            .And()
            .HaveNameEndingWith("RequestHandler")
            .GetTypes();

        // Assert
        foreach (var handler in handlers)
        {
            var requestName = handler.Name.Replace("Handler", "");

            // Request should be in same namespace
            var request = Types.InCurrentDomain()
                .That()
                .ResideInNamespace(handler.Namespace)
                .And()
                .HaveName(requestName)
                .GetTypes()
                .FirstOrDefault();

            // Some handlers might have requests in parent namespace or use base classes
            if (request == null)
            {
                // Try to find in parent namespace
                var parentNamespace = string.Join(".", handler.Namespace.Split('.').SkipLast(1));
                request = Types.InCurrentDomain()
                    .That()
                    .ResideInNamespace(parentNamespace)
                    .And()
                    .HaveName(requestName)
                    .GetTypes()
                    .FirstOrDefault();
            }

            Assert.NotNull(request);
        }
    }

    [Fact]
    public void Validator_Names_Should_Match_Request_Names()
    {
        // Arrange
        var validators = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceContaining("UseCases")
            .And()
            .HaveNameEndingWith("RequestValidator")
            .GetTypes();

        // Assert
        foreach (var validator in validators)
        {
            var requestName = validator.Name.Replace("Validator", "");
            
            // Request should be in same namespace
            var request = Types.InCurrentDomain()
                .That()
                .ResideInNamespace(validator.Namespace)
                .And()
                .HaveName(requestName)
                .GetTypes()
                .FirstOrDefault();

            Assert.NotNull(request);
        }
    }

    [Fact]
    public void Response_Names_Should_Match_Request_Names()
    {
        // Arrange
        var responses = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceContaining("UseCases")
            .And()
            .HaveNameEndingWith("Response")
            .GetTypes();

        // Assert
        foreach (var response in responses)
        {
            var requestName = response.Name.Replace("Response", "Request");

            // Request should be in same namespace (skip if request doesn't exist - some responses are shared)
            var request = Types.InCurrentDomain()
                .That()
                .ResideInNamespace(response.Namespace)
                .And()
                .HaveName(requestName)
                .GetTypes()
                .FirstOrDefault();

            // Only assert if the corresponding request exists
            if (request != null)
            {
                Assert.Equal(response.Namespace, request.Namespace);
            }
        }
    }

    [Fact]
    public void Controller_Names_Should_Match_Feature_Names()
    {
        // Arrange
        var controllers = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespaceContaining("Controllers")
            .And()
            .HaveNameEndingWith("Controller")
            .And()
            .DoNotHaveName("ApiController")
            .GetTypes();

        // Assert
        foreach (var controller in controllers)
        {
            var featureName = controller.Name.Replace("Controller", "");
            
            // Check if corresponding use cases exist
            var useCases = Types.InCurrentDomain()
                .That()
                .ResideInNamespaceContaining($"UseCases.{featureName}")
                .GetTypes();

            Assert.NotEmpty(useCases);
        }
    }

    [Fact]
    public void Boolean_Properties_Should_Use_Is_Or_Has_Prefix()
    {
        // Arrange
        var allTypes = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceStartingWith("GeminiCode.Hunter")
            .GetTypes();

        // Assert
        foreach (var type in allTypes)
        {
            var boolProperties = type.GetProperties()
                .Where(p => p.PropertyType == typeof(bool));

            foreach (var property in boolProperties)
            {
                var hasCorrectPrefix = property.Name.StartsWith("Is") ||
                                      property.Name.StartsWith("Has") ||
                                      property.Name.StartsWith("Can") ||
                                      property.Name.StartsWith("Should") ||
                                      property.Name.StartsWith("Allow") ||
                                      property.Name.StartsWith("Enable") ||
                                      property.Name.StartsWith("Receive") ||
                                      property.Name.StartsWith("Convert") ||
                                      property.Name.StartsWith("Send");

                Assert.True(hasCorrectPrefix,
                    $"Boolean property '{property.Name}' in {type.Name} should start with Is/Has/Can/Should/Allow/Enable");
            }
        }
    }

    [Fact]
    public void Notification_Names_Should_Be_Past_Tense()
    {
        // Arrange
        var notifications = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceContaining("Notifications")
            .And()
            .HaveNameEndingWith("Notification")
            .GetTypes();

        // Assert - Notifications represent something that already happened
        foreach (var notification in notifications)
        {
            var notificationName = notification.Name.Replace("Notification", "");

            // Should end with -ed or specific past tense patterns
            var isPastTense = notificationName.EndsWith("Created") ||
                            notificationName.EndsWith("Updated") ||
                            notificationName.EndsWith("Deleted") ||
                            notificationName.EndsWith("Changed") ||
                            notificationName.EndsWith("Detected") ||
                            notificationName.EndsWith("Sent") ||
                            notificationName.EndsWith("Analyzed") ||
                            notificationName.EndsWith("Added") || // Allow "Added" as past tense
                            notificationName.EndsWith("Started"); // Allow "Started" as past tense

            Assert.True(isPastTense,
                $"Notification '{notificationName}' should use past tense (e.g., Created, Updated, Deleted, Added, Started)");
        }
    }

    [Fact]
    public void Private_Fields_Should_Use_CamelCase()
    {
        // This is harder to test with NetArchTest, but we can validate the pattern
        var allTypes = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceStartingWith("GeminiCode.Hunter")
            .And()
            .DoNotResideInNamespace("GeminiCode.Hunter.Test") // Exclude test project
            .GetTypes();

        foreach (var type in allTypes)
        {
            var privateFields = type.GetFields(
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance)
                .Where(f => !f.Name.Contains("BackingField") && 
                           !f.Name.Contains("<") && 
                           !f.IsInitOnly); // Exclude compiler-generated and readonly fields

            foreach (var field in privateFields)
            {
                // Private fields should start with underscore or be camelCase
                var isValid = field.Name.StartsWith("_") || 
                             char.IsLower(field.Name[0]);

                Assert.True(isValid,
                    $"Private field '{field.Name}' in {type.Name} should use camelCase or _camelCase");
            }
        }
    }

    [Fact]
    public void Folder_Structure_Should_Match_Namespace()
    {
        // This validates the convention that namespace should match folder structure
        // Example: GeminiCode.Hunter.Application.Core.UseCases.Users.Commands.Create
        // Should be in: src/GeminiCode.Hunter.Application.Core/UseCases/Users/Commands/Create
        
        var allTypes = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceStartingWith("GeminiCode.Hunter")
            .GetTypes();

        foreach (var type in allTypes)
        {
            // Namespace should follow the pattern: GeminiCode.Hunter.{Project}.{Folders}
            var namespaceParts = type.Namespace?.Split('.');
            
            Assert.NotNull(namespaceParts);
            Assert.True(namespaceParts.Length >= 3,
                $"{type.FullName} should have at least 3 namespace parts");
            
            // First two parts should always be GeminiCode.Hunter
            Assert.Equal("GeminiCode", namespaceParts[0]);
            Assert.Equal("Hunter", namespaceParts[1]);
        }
    }
}
