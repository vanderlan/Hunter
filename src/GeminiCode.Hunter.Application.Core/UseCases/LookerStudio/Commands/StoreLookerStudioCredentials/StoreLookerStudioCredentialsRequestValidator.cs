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

        RuleFor(x => x.ExpiresInDays)
            .GreaterThan(0)
            .WithMessage("Expiration days must be greater than 0")
            .LessThanOrEqualTo(30)
            .WithMessage("Maximum expiration is 30 days");
    }
}
