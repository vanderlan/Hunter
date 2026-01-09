using System.Globalization;

namespace GeminiCode.Hunter.Application.Core.Helpers;

public static class StringExtensions
{
    /// <summary>
    /// Converts a full name to proper Title Case
    /// Handles multiple spaces and special cases
    /// </summary>
    public static string FormatName(this string name)
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
