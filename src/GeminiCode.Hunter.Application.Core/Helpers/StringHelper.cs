using System.Globalization;
using System.Text;

namespace GeminiCode.Hunter.Application.Core.Helpers;

public static class StringHelper
{
    /// <summary>
    /// Converts a string to Title Case (each word capitalized)
    /// Example: "JOHN DOE" -> "John Doe", "john doe" -> "John Doe"
    /// </summary>
    public static string ToTitleCase(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input ?? string.Empty;

        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(input.ToLower());
    }

    /// <summary>
    /// Converts a full name to proper Title Case
    /// Handles multiple spaces and special cases
    /// </summary>
    public static string FormatName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        // Remove extra spaces and convert to title case
        var words = name.Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var formattedWords = words.Select(word => 
            CultureInfo.CurrentCulture.TextInfo.ToTitleCase(word.ToLower()));

        return string.Join(" ", formattedWords);
    }
}
