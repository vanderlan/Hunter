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

//TODO: Temp disabled due server resource limits
//builder.Configuration.ConfigureLogging();
//builder.Host.UseSerilog();

builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

// app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionMiddleware>();

app.UseRateLimiter();

app.ConfigureApp();

app.RunMigrations();

await app.RunAsync();
