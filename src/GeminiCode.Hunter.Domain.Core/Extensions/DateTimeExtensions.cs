using System;
using GeminiCode.Hunter.Domain.Core.Configuration;
using GeminiCode.Hunter.Domain.Core.Constants;

namespace GeminiCode.Hunter.Domain.Core.Extensions;

/// <summary>
/// Extension methods for DateTime timezone conversion
/// </summary>
public static class DateTimeExtensions
{
    private static ApplicationSettings? _applicationSettings;
    
    /// <summary>
    /// Initialize the ApplicationSettings for timezone conversion
    /// This should be called during application startup
    /// </summary>
    public static void Initialize(ApplicationSettings applicationSettings)
    {
        _applicationSettings = applicationSettings;
    }
    
    /// <summary>
    /// Converts UTC DateTime to Brazil timezone (UTC-3)
    /// </summary>
    /// <param name="utcDateTime">DateTime in UTC</param>
    /// <returns>DateTime in Brazil timezone</returns>
    public static DateTime ToBrazilTime(this DateTime utcDateTime)
    {
        return TimezoneConstants.ConvertUtcToBrazilTime(utcDateTime);
    }
    
    /// <summary>
    /// Converts UTC DateTime to the configured application timezone
    /// Uses ApplicationSettings.TimeZoneId if configured, otherwise defaults to Brazil time
    /// </summary>
    /// <param name="utcDateTime">DateTime in UTC</param>
    /// <returns>DateTime in configured timezone</returns>
    public static DateTime ToApplicationTime(this DateTime utcDateTime)
    {
        if (_applicationSettings?.ConvertTimezoneInResponses != true)
        {
            return utcDateTime;
        }
        
        var timeZoneId = _applicationSettings?.TimeZoneId;
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return ToBrazilTime(utcDateTime);
        }
        
        var timeZone = TimezoneConstants.GetTimeZone(timeZoneId);
        return TimezoneConstants.ConvertFromUtc(utcDateTime, timeZone);
    }
    
    /// <summary>
    /// Converts UTC DateTime to a specific timezone
    /// </summary>
    /// <param name="utcDateTime">DateTime in UTC</param>
    /// <param name="timeZoneId">Target timezone ID (e.g., "E. South America Standard Time")</param>
    /// <returns>DateTime in specified timezone</returns>
    public static DateTime ToTimeZone(this DateTime utcDateTime, string timeZoneId)
    {
        var timeZone = TimezoneConstants.GetTimeZone(timeZoneId);
        return TimezoneConstants.ConvertFromUtc(utcDateTime, timeZone);
    }
}
