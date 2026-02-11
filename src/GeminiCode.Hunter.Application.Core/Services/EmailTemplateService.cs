using System.Reflection;
using System.Text;
using GeminiCode.Hunter.Domain.Core.Services;
using Microsoft.Extensions.Logging;

namespace GeminiCode.Hunter.Application.Core.Services;

public class EmailTemplateService(ILogger<EmailTemplateService> logger) : IEmailTemplateService
{
    private const string TemplatesFolder = "EmailTemplates";

    public string RenderJobChangesTemplate(Dictionary<string, object> data)
    {
        logger.LogInformation("Rendering job changes template with data keys: {Keys}", 
            string.Join(", ", data.Keys));
        logger.LogInformation("HasNewJobs: {HasNewJobs}, HasClosedJobs: {HasClosedJobs}", 
            data.GetValueOrDefault("HasNewJobs"), 
            data.GetValueOrDefault("HasClosedJobs"));
        logger.LogInformation("NewJobs count: {Count}", 
            (data.GetValueOrDefault("NewJobs") as IEnumerable<object>)?.Count() ?? 0);
        logger.LogInformation("ClosedJobs count: {Count}", 
            (data.GetValueOrDefault("ClosedJobs") as IEnumerable<object>)?.Count() ?? 0);
        
        var template = LoadTemplate("JobChangesTemplate.html");
        logger.LogInformation("Template loaded, length: {Length}", template.Length);
        
        var result = RenderTemplate(template, data);
        logger.LogInformation("Template rendered, result length: {Length}", result.Length);

        return result;
    }

    public string RenderCandidateMatchesTemplate(Dictionary<string, object> data)
    {
        logger.LogInformation("Rendering candidate matches template");
        var template = LoadTemplate("CandidateMatchesTemplate.html");
        var result = RenderTemplate(template, data);
        logger.LogInformation("Template rendered, length: {Length}", result.Length);
        return result;
    }

    public string RenderWelcomeEmailTemplate(Dictionary<string, object> data)
    {
        logger.LogInformation("Rendering welcome email template");
        var template = LoadTemplate("WelcomeEmailTemplate.html");
        var result = RenderTemplate(template, data);
        logger.LogInformation("Template rendered, length: {Length}", result.Length);
        return result;
    }

    public string RenderCandidateReferralTemplate(Dictionary<string, object> data)
    {
        logger.LogInformation("Rendering candidate referral template");
        var template = LoadTemplate("CandidateReferralTemplate.html");
        var result = RenderTemplate(template, data);
        logger.LogInformation("Template rendered, length: {Length}", result.Length);
        return result;
    }

    public string RenderStatusChangeTemplate(Dictionary<string, object> data)
    {
        logger.LogInformation("Rendering status change template");
        var template = LoadTemplate("StatusChangeTemplate.html");
        var result = RenderTemplate(template, data);
        logger.LogInformation("Template rendered, length: {Length}", result.Length);
        return result;
    }

    public string RenderNoteAddedTemplate(Dictionary<string, object> data)
    {
        logger.LogInformation("Rendering note added template");
        var template = LoadTemplate("NoteAddedTemplate.html");
        var result = RenderTemplate(template, data);
        logger.LogInformation("Template rendered, length: {Length}", result.Length);
        return result;
    }

    public string RenderHiringProcessCancelledTemplate(Dictionary<string, object> data)
    {
        logger.LogInformation("Rendering hiring process cancelled template");
        var template = LoadTemplate("HiringProcessCancelledTemplate.html");
        var result = RenderTemplate(template, data);
        logger.LogInformation("Template rendered, length: {Length}", result.Length);
        return result;
    }

    private string LoadTemplate(string templateName)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = $"GeminiCode.Hunter.Application.Core.{TemplatesFolder}.{templateName}";

            using var stream = assembly.GetManifestResourceStream(resourcePath);
            if (stream == null)
            {
                // Fallback to file system
                var filePath = Path.Combine(AppContext.BaseDirectory, TemplatesFolder, templateName);
                if (File.Exists(filePath))
                {
                    logger.LogInformation("Loading template from file: {FilePath}", filePath);
                    return File.ReadAllText(filePath);
                }

                throw new FileNotFoundException($"Email template '{templateName}' not found");
            }

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading email template: {TemplateName}", templateName);
            throw;
        }
    }

    private string RenderTemplate(string template, Dictionary<string, object> data)
    {
        var result = template;

        // IMPORTANT: Process conditionals BEFORE loops
        // This ensures {{#if}} blocks are evaluated first, then loops inside them can be processed
        result = ProcessConditionals(result, data);
        
        // Process all loops (including nested ones) recursively
        result = ProcessLoopsRecursive(result, data, isTopLevel: true);
        
        // Replace simple variables (after loops and conditionals)
        result = ReplaceVariables(result, data);

        return result;
    }

    private string ProcessLoopsRecursive(string template, Dictionary<string, object> data, bool isTopLevel)
    {
        var result = template;
        var startTag = "{{#each ";
        var endTag = "{{/each}}";

        // Process loops from innermost to outermost to handle nesting properly
        var loopsToProcess = new List<(int startIndex, int endIndex, string collectionName, string itemTemplate)>();

        // Find all loop blocks at this level
        var searchIndex = 0;
        while (searchIndex < result.Length)
        {
            var startIndex = result.IndexOf(startTag, searchIndex, StringComparison.Ordinal);
            if (startIndex == -1) break;

            var collectionNameStart = startIndex + startTag.Length;
            var collectionNameEnd = result.IndexOf("}}", collectionNameStart, StringComparison.Ordinal);
            if (collectionNameEnd == -1) break;

            var collectionName = result.Substring(collectionNameStart, collectionNameEnd - collectionNameStart).Trim();
            var contentStart = collectionNameEnd + 2;
            
            // Find matching end tag, accounting for nested loops
            var endIndex = FindMatchingEndTag(result, contentStart, startTag, endTag);
            if (endIndex == -1) break;

            var itemTemplate = result.Substring(contentStart, endIndex - contentStart);
            
            loopsToProcess.Add((startIndex, endIndex + endTag.Length, collectionName, itemTemplate));
            
            searchIndex = endIndex + endTag.Length;
        }

        // Process each loop
        foreach (var (startIndex, endIndex, collectionName, itemTemplate) in loopsToProcess)
        {
            var loopBlock = result.Substring(startIndex, endIndex - startIndex);
            
            logger.LogInformation("{Level} loop: {CollectionName}", isTopLevel ? "Processing" : "Processing nested", collectionName);

            var rendered = new StringBuilder();

            if (data.TryGetValue(collectionName, out var collectionObj) && collectionObj is IEnumerable<object> collection)
            {
                var items = collection.ToList();
                logger.LogInformation("Collection '{CollectionName}' has {Count} items", collectionName, items.Count);

                foreach (var item in items)
                {
                    var itemHtml = itemTemplate;

                    if (item is string str)
                    {
                        itemHtml = itemHtml.Replace("{{this}}", str);
                    }
                    else if (item is Dictionary<string, object> dict)
                    {
                        // Process nested loops recursively with the item's context
                        itemHtml = ProcessLoopsRecursive(itemHtml, dict, isTopLevel: false);
                        
                        // Process conditionals for this item
                        itemHtml = ProcessConditionals(itemHtml, dict);
                        
                        // Replace variables
                        itemHtml = ReplaceVariables(itemHtml, dict);
                    }
                    else if (item != null)
                    {
                        var itemDict = ObjectToDictionary(item);

                        // Process nested loops recursively with the item's context
                        itemHtml = ProcessLoopsRecursive(itemHtml, itemDict, isTopLevel: false);
                        
                        // Process conditionals for this item
                        itemHtml = ProcessConditionals(itemHtml, itemDict);
                        
                        // Replace variables
                        itemHtml = ReplaceVariables(itemHtml, itemDict);
                    }

                    rendered.Append(itemHtml);
                }
            }
            else
            {
                logger.LogWarning("Collection '{CollectionName}' not found or empty", collectionName);
            }

            result = result.Replace(loopBlock, rendered.ToString());
        }

        return result;
    }

    private int FindMatchingEndTag(string template, int startSearch, string startTag, string endTag)
    {
        var depth = 1;
        var index = startSearch;

        while (depth > 0 && index < template.Length)
        {
            var nextStart = template.IndexOf(startTag, index, StringComparison.Ordinal);
            var nextEnd = template.IndexOf(endTag, index, StringComparison.Ordinal);

            if (nextEnd == -1) return -1;

            if (nextStart != -1 && nextStart < nextEnd)
            {
                depth++;
                index = nextStart + startTag.Length;
            }
            else
            {
                depth--;
                if (depth == 0) return nextEnd;
                index = nextEnd + endTag.Length;
            }
        }

        return -1;
    }

    private string ProcessConditionals(string template, Dictionary<string, object> data)
    {
        var result = template;

        // Process {{#if condition}} ... {{/if}}
        result = ProcessConditionalBlock(result, data, "{{#if ", "{{/if}}", shouldShow: true);

        // Process {{#unless condition}} ... {{/unless}}
        result = ProcessConditionalBlock(result, data, "{{#unless ", "{{/unless}}", shouldShow: false);

        return result;
    }

    private string ProcessConditionalBlock(string template, Dictionary<string, object> data, string startTag, string endTag, bool shouldShow)
    {
        var result = template;

        while (result.Contains(startTag))
        {
            var startIndex = result.IndexOf(startTag, StringComparison.Ordinal);
            if (startIndex == -1) break;

            var conditionStart = startIndex + startTag.Length;
            var conditionEnd = result.IndexOf("}}", conditionStart, StringComparison.Ordinal);
            if (conditionEnd == -1) break;

            var condition = result.Substring(conditionStart, conditionEnd - conditionStart).Trim();
            var contentStart = conditionEnd + 2;
            var contentEnd = result.IndexOf(endTag, contentStart, StringComparison.Ordinal);
            if (contentEnd == -1) break;

            var content = result.Substring(contentStart, contentEnd - contentStart);
            var block = result.Substring(startIndex, contentEnd + endTag.Length - startIndex);

            var hasCondition = data.TryGetValue(condition, out var value);
            var isTrue = hasCondition && value is bool boolValue && boolValue;

            var display = shouldShow ? isTrue : !isTrue;

            result = result.Replace(block, display ? content : string.Empty);
        }

        return result;
    }

    private string ReplaceVariables(string template, Dictionary<string, object> data)
    {
        var result = template;

        foreach (var kvp in data)
        {
            var placeholder = $"{{{{{kvp.Key}}}}}";
            if (result.Contains(placeholder))
            {
                var value = kvp.Value?.ToString() ?? string.Empty;
                result = result.Replace(placeholder, value);
            }
        }

        return result;
    }

    private Dictionary<string, object> ObjectToDictionary(object obj)
    {
        var dict = new Dictionary<string, object>();
        var properties = obj.GetType().GetProperties();

        foreach (var prop in properties)
        {
            try
            {
                var value = prop.GetValue(obj);
                dict[prop.Name] = value ?? string.Empty;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error getting property {PropertyName}", prop.Name);
                dict[prop.Name] = string.Empty;
            }
        }

        return dict;
    }
}
