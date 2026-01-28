using GeminiCode.Hunter.Api.Controllers.Base;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace GeminiCode.Hunter.Api.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class DiagnosticsController : ApiController
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(
        IMediator mediator,
        IConfiguration configuration, 
        ILogger<DiagnosticsController> logger) : base(mediator)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Test email SMTP connection and authentication
    /// </summary>
    [HttpPost("test-email-connection")]
    public async Task<IActionResult> TestEmailConnection()
    {
        try
        {
            var emailConfig = _configuration.GetSection("Email");
            var smtpHost = emailConfig["SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(emailConfig["SmtpPort"] ?? "587");
            var fromEmail = emailConfig["FromEmail"];
            var smtpLogin = emailConfig["SmtpLogin"] ?? fromEmail;
            var password = emailConfig["Password"];

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(password))
            {
                return BadRequest(new { error = "Email configuration is missing FromEmail or Password" });
            }

            var diagnostics = new
            {
                timestamp = DateTime.UtcNow,
                configuration = new
                {
                    smtpHost,
                    smtpPort,
                    fromEmail,
                    smtpLogin,
                    passwordConfigured = !string.IsNullOrEmpty(password)
                },
                tests = new List<object>()
            };

            var tests = (List<object>)diagnostics.tests;

            // Test 1: Connection
            try
            {
                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                tests.Add(new
                {
                    test = "SMTP Connection",
                    status = "? SUCCESS",
                    message = $"Successfully connected to {smtpHost}:{smtpPort}"
                });

                // Test 2: Authentication
                try
                {
                    await smtp.AuthenticateAsync(smtpLogin, password);
                    tests.Add(new
                    {
                        test = "SMTP Authentication",
                        status = "? SUCCESS",
                        message = $"Successfully authenticated with login: {smtpLogin}"
                    });

                    // Test 3: Send test email
                    try
                    {
                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress("Hunter Diagnostics", fromEmail));
                        message.To.Add(new MailboxAddress("", fromEmail)); // Send to self
                        message.Subject = "Hunter Email Test - " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                        var bodyBuilder = new BodyBuilder
                        {
                            TextBody = "This is a test email from Hunter application diagnostics. If you received this, email sending is working correctly!"
                        };
                        message.Body = bodyBuilder.ToMessageBody();

                        await smtp.SendAsync(message);
                        tests.Add(new
                        {
                            test = "Send Test Email",
                            status = "? SUCCESS",
                            message = $"Test email sent successfully to {fromEmail}"
                        });
                    }
                    catch (Exception ex)
                    {
                        tests.Add(new
                        {
                            test = "Send Test Email",
                            status = "? FAILED",
                            message = ex.Message,
                            details = ex.InnerException?.Message
                        });
                    }

                    await smtp.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    tests.Add(new
                    {
                        test = "SMTP Authentication",
                        status = "? FAILED",
                        message = ex.Message,
                        details = ex.InnerException?.Message,
                        possibleCauses = new[]
                        {
                            "Invalid App Password",
                            "Gmail account blocked/rate limited",
                            "2-Step Verification not enabled",
                            "App Password revoked or expired"
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                tests.Add(new
                {
                    test = "SMTP Connection",
                    status = "? FAILED",
                    message = ex.Message,
                    details = ex.InnerException?.Message,
                    possibleCauses = new[]
                    {
                        "Network connectivity issue",
                        "Firewall blocking port 587",
                        "SMTP host incorrect"
                    }
                });
            }

            return Ok(diagnostics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email diagnostic test failed");
            return StatusCode(500, new
            {
                error = "Diagnostic test failed",
                message = ex.Message,
                details = ex.InnerException?.Message
            });
        }
    }
}
