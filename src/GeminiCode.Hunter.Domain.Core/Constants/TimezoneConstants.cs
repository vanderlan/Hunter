using System;

namespace GeminiCode.Hunter.Domain.Core.Constants;

/// <summary>
/// Constants related to timezone handling
/// </summary>
public static class TimezoneConstants
{
    /// <summary>
    /// Brazil timezone ID for Windows (UTC-3)
    /// </summary>
    public const string BrazilTimeZoneIdWindows = "E. South America Standard Time";
    
    /// <summary>
    /// Brazil timezone ID for Linux/Unix (UTC-3)
    /// </summary>
    public const string BrazilTimeZoneIdLinux = "America/Sao_Paulo";
    
    /// <summary>
    /// Gets the Brazil TimeZoneInfo, attempting both Windows and Linux formats
    /// </summary>
    /// <returns>TimeZoneInfo for Brazil (UTC-3)</returns>
    public static TimeZoneInfo GetBrazilTimeZone()
    {
        try
        {
            // Try Windows format first
            return TimeZoneInfo.FindSystemTimeZoneById(BrazilTimeZoneIdWindows);
        }
        catch
        {
            try
            {
                // Fallback to Linux format
                return TimeZoneInfo.FindSystemTimeZoneById(BrazilTimeZoneIdLinux);
            }
            catch
            {
                // If both fail, create a custom timezone for UTC-3
                return TimeZoneInfo.CreateCustomTimeZone(
                    "Brazil Custom",
                    TimeSpan.FromHours(-3),
                    "Brazil Time",
                    "BRT");
            }
        }
    }
    
    /// <summary>
    /// Gets a TimeZoneInfo by ID, with fallback handling for cross-platform compatibility
    /// </summary>
    /// <param name="timeZoneId">Timezone ID (Windows or Linux format)</param>
    /// <returns>TimeZoneInfo for the specified ID</returns>
    public static TimeZoneInfo GetTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch
        {
            // If the provided ID fails, try Brazil timezone as default
            return GetBrazilTimeZone();
        }
    }
    
    /// <summary>
    /// Converts UTC DateTime to a specified timezone
    /// </summary>
    /// <param name="utcDateTime">DateTime in UTC</param>
    /// <param name="timeZoneInfo">Target timezone</param>
    /// <returns>DateTime converted to target timezone</returns>
    public static DateTime ConvertFromUtc(DateTime utcDateTime, TimeZoneInfo timeZoneInfo)
    {
        // Ensure DateTime is treated as UTC or Unspecified
        if (utcDateTime.Kind == DateTimeKind.Local)
        {
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        }
        else if (utcDateTime.Kind == DateTimeKind.Unspecified)
        {
            // Treat Unspecified as UTC (which is how PostgreSQL stores timestamps)
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        }
        
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);
    }
    
    /// <summary>
    /// Converts UTC DateTime to Brazil timezone (UTC-3)
    /// </summary>
    /// <param name="utcDateTime">DateTime in UTC</param>
    /// <returns>DateTime in Brazil timezone</returns>
    public static DateTime ConvertUtcToBrazilTime(DateTime utcDateTime)
    {
        var brazilTimeZone = GetBrazilTimeZone();
        return ConvertFromUtc(utcDateTime, brazilTimeZone);
    }
}
