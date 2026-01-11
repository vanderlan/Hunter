using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Concurrent;
using System.Net;

namespace GeminiCode.Hunter.Api.Attributes;

/// <summary>
/// Attribute to apply rate limiting to controller actions based on IP address
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RateLimitAttribute : Attribute, IAsyncActionFilter
{
    private static readonly ConcurrentDictionary<string, Queue<DateTime>> _requestHistory = new();
    private static DateTime _lastCleanup = DateTime.UtcNow;
    private static readonly object _cleanupLock = new();
    
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

        // Periodic cleanup of stale entries (every 5 minutes)
        CleanupStaleEntries();

        var key = $"{ipAddress}:{context.ActionDescriptor.DisplayName}";
        var now = DateTime.UtcNow;

        // Get or create the queue for this key
        var requestTimes = _requestHistory.GetOrAdd(key, _ => new Queue<DateTime>());

        lock (requestTimes)
        {
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

    private void CleanupStaleEntries()
    {
        var now = DateTime.UtcNow;
        
        // Only cleanup if 5 minutes have passed since last cleanup
        if ((now - _lastCleanup).TotalMinutes < 5)
        {
            return;
        }

        lock (_cleanupLock)
        {
            // Double-check after acquiring lock
            if ((now - _lastCleanup).TotalMinutes < 5)
            {
                return;
            }

            // Remove entries that have no requests in the last hour
            var keysToRemove = new List<string>();
            foreach (var kvp in _requestHistory)
            {
                lock (kvp.Value)
                {
                    // If queue is empty or all requests are older than 1 hour, mark for removal
                    if (kvp.Value.Count == 0 || 
                        (kvp.Value.Count > 0 && (now - kvp.Value.Last()) > TimeSpan.FromHours(1)))
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                _requestHistory.TryRemove(key, out _);
            }

            _lastCleanup = now;
        }
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
