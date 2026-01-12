using GeminiCode.Hunter.Api;
using GeminiCode.Hunter.Api.Configuration;
using GeminiCode.Hunter.Api.Filters;
using GeminiCode.Hunter.Api.Middleware;
using FluentValidation;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<AutoValidationActionFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AutoValidationActionFilter>();
});

// Configure request timeout middleware for long-running operations
builder.Services.AddRequestTimeouts(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Http.Timeouts.RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromMinutes(5)
    };
});

//TODO: Temp disabled due server resource limits
//builder.Configuration.ConfigureLogging();
//builder.Host.UseSerilog();

builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

// app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionMiddleware>();

// Add request timeout middleware
app.UseRequestTimeouts();

app.ConfigureApp();

app.RunMigrations();

await app.RunAsync();
