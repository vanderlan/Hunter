using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace GeminiCode.Hunter.Api.Attributes;

/// <summary>
/// Attribute to apply rate limiting to controller actions based on IP address
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RateLimitAttribute : Attribute, IAsyncActionFilter
{
    private static readonly Dictionary<string, Queue<DateTime>> _requestHistory = new();
    private static readonly object _lock = new();
    
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;

    /// <summary>
    /// Creates a new rate limit attribute
    /// </summary>
    /// <param name="maxRequests">Maximum number of requests allowed</param>
    /// <param name="windowInSeconds">Time window in seconds</param>
    public RateLimitAttribute(int maxRequests, int windowInSeconds)
    {
        _maxRequests = maxRequests;
        _timeWindow = TimeSpan.FromSeconds(windowInSeconds);
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var ipAddress = GetClientIpAddress(context.HttpContext);
        
        if (string.IsNullOrEmpty(ipAddress))
        {
            await next();
            return;
        }

        var key = $"{ipAddress}:{context.ActionDescriptor.DisplayName}";
        var now = DateTime.UtcNow;

        lock (_lock)
        {
            if (!_requestHistory.ContainsKey(key))
            {
                _requestHistory[key] = new Queue<DateTime>();
            }

            var requestTimes = _requestHistory[key];

            // Remove old requests outside the time window
            while (requestTimes.Count > 0 && (now - requestTimes.Peek()) > _timeWindow)
            {
                requestTimes.Dequeue();
            }

            // Check if limit is exceeded
            if (requestTimes.Count >= _maxRequests)
            {
                context.Result = new ContentResult
                {
                    StatusCode = (int)HttpStatusCode.TooManyRequests,
                    Content = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        title = "Rate limit exceeded",
                        errors = new[] { $"Too many requests. Maximum {_maxRequests} requests per {_timeWindow.TotalSeconds} seconds allowed." },
                        type = "Error"
                    }),
                    ContentType = "application/json"
                };
                return;
            }

            // Add current request
            requestTimes.Enqueue(now);
        }

        await next();
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Try to get IP from X-Forwarded-For header (for reverse proxy scenarios)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',');
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        // Try X-Real-IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to RemoteIpAddress
        return context.Connection.RemoteIpAddress?.ToString();
    }
}
