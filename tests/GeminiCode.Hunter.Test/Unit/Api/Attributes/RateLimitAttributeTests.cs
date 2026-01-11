using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;
using GeminiCode.Hunter.Api.Attributes;

namespace GeminiCode.Hunter.Test.Unit.Api.Attributes;

public class RateLimitAttributeTests
{
    [Fact]
    public async Task OnActionExecutionAsync_WhenUnderLimit_AllowsRequest()
    {
        // Arrange
        var attribute = new RateLimitAttribute(5, 60);
        var httpContext = CreateHttpContext("192.168.1.1");
        var actionContext = CreateActionContext(httpContext);
        var actionExecutingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            new object());
        
        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object()));
        };

        // Act
        await attribute.OnActionExecutionAsync(actionExecutingContext, next);

        // Assert
        Assert.True(nextCalled);
        Assert.Null(actionExecutingContext.Result);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenOverLimit_BlocksRequest()
    {
        // Arrange
        var attribute = new RateLimitAttribute(2, 60);
        var httpContext = CreateHttpContext("192.168.1.2");
        var actionContext = CreateActionContext(httpContext);
        
        var nextCalled = 0;
        ActionExecutionDelegate next = () =>
        {
            nextCalled++;
            return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object()));
        };

        // Act - Make 3 requests (limit is 2)
        var context1 = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), new object());
        await attribute.OnActionExecutionAsync(context1, next);

        var context2 = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), new object());
        await attribute.OnActionExecutionAsync(context2, next);

        var context3 = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), new object());
        await attribute.OnActionExecutionAsync(context3, next);

        // Assert
        Assert.Equal(2, nextCalled); // Only first 2 should succeed
        Assert.NotNull(context3.Result);
        Assert.IsType<ContentResult>(context3.Result);
        var result = (ContentResult)context3.Result;
        Assert.Equal((int)HttpStatusCode.TooManyRequests, result.StatusCode);
    }

    [Fact]
    public async Task OnActionExecutionAsync_DifferentIPs_AllowedSeparately()
    {
        // Arrange
        var attribute = new RateLimitAttribute(2, 60);
        
        var httpContext1 = CreateHttpContext("192.168.1.3");
        var actionContext1 = CreateActionContext(httpContext1);
        
        var httpContext2 = CreateHttpContext("192.168.1.4");
        var actionContext2 = CreateActionContext(httpContext2);
        
        var nextCalled = 0;
        ActionExecutionDelegate next = () =>
        {
            nextCalled++;
            return Task.FromResult(new ActionExecutedContext(actionContext1, new List<IFilterMetadata>(), new object()));
        };

        // Act - Make 2 requests from each IP
        var context1 = new ActionExecutingContext(actionContext1, new List<IFilterMetadata>(), new Dictionary<string, object>(), new object());
        await attribute.OnActionExecutionAsync(context1, next);

        var context2 = new ActionExecutingContext(actionContext1, new List<IFilterMetadata>(), new Dictionary<string, object>(), new object());
        await attribute.OnActionExecutionAsync(context2, next);

        var context3 = new ActionExecutingContext(actionContext2, new List<IFilterMetadata>(), new Dictionary<string, object>(), new object());
        await attribute.OnActionExecutionAsync(context3, next);

        var context4 = new ActionExecutingContext(actionContext2, new List<IFilterMetadata>(), new Dictionary<string, object>(), new object());
        await attribute.OnActionExecutionAsync(context4, next);

        // Assert - All 4 should succeed as they're from different IPs
        Assert.Equal(4, nextCalled);
        Assert.Null(context1.Result);
        Assert.Null(context2.Result);
        Assert.Null(context3.Result);
        Assert.Null(context4.Result);
    }

    [Fact]
    public async Task OnActionExecutionAsync_ReadsXForwardedForHeader()
    {
        // Arrange
        var attribute = new RateLimitAttribute(1, 60);
        var httpContext = CreateHttpContext(null);
        httpContext.Request.Headers["X-Forwarded-For"] = "10.0.0.1, 10.0.0.2";
        var actionContext = CreateActionContext(httpContext);
        
        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object()));
        };

        var context1 = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), new object());

        // Act
        await attribute.OnActionExecutionAsync(context1, next);

        // Assert
        Assert.True(nextCalled);
        Assert.Null(context1.Result);
    }

    [Fact]
    public async Task OnActionExecutionAsync_ReadsXRealIPHeader()
    {
        // Arrange
        var attribute = new RateLimitAttribute(1, 60);
        var httpContext = CreateHttpContext(null);
        httpContext.Request.Headers["X-Real-IP"] = "10.0.0.5";
        var actionContext = CreateActionContext(httpContext);
        
        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object()));
        };

        var context1 = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), new object());

        // Act
        await attribute.OnActionExecutionAsync(context1, next);

        // Assert
        Assert.True(nextCalled);
        Assert.Null(context1.Result);
    }

    private static HttpContext CreateHttpContext(string ipAddress)
    {
        var context = new DefaultHttpContext();
        
        if (!string.IsNullOrEmpty(ipAddress))
        {
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(ipAddress);
        }
        
        return context;
    }

    private static ActionContext CreateActionContext(HttpContext httpContext)
    {
        return new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor { DisplayName = "TestAction" });
    }
}
