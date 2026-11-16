using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.ConfirmPublicCandidateEmail;

public class ConfirmPublicCandidateEmailRequestValidator : AbstractValidator<ConfirmPublicCandidateEmailRequest>
{
    public ConfirmPublicCandidateEmailRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("Confirmation token is required").MaximumLength(64);
    }
}
