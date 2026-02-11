using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.LookerStudio.Commands.StoreLookerStudioCredentials;

public class StoreLookerStudioCredentialsRequestValidator : AbstractValidator<StoreLookerStudioCredentialsRequest>
{
    public StoreLookerStudioCredentialsRequestValidator()
    {
        RuleFor(x => x.CurlCommand)
            .NotEmpty()
            .WithMessage("Curl command is required")
            .Must(x => x.Contains("lookerstudio.google.com"))
            .WithMessage("Invalid curl command. Must be a Looker Studio export request");

        RuleFor(x => x.ExpiresInHours)
            .GreaterThan(0)
            .WithMessage("Expiration hours must be greater than 0")
            .LessThanOrEqualTo(24)
            .WithMessage("Maximum expiration is 24 hours. Google sessions typically expire after 1 hour, but proactive token refresh may extend this.");
    }
}
