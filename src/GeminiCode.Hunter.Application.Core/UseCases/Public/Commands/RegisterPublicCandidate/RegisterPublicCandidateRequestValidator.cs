using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.RegisterPublicCandidate;

public class RegisterPublicCandidateRequestValidator : AbstractValidator<RegisterPublicCandidateRequest>
{
    public RegisterPublicCandidateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required").MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required").EmailAddress().WithMessage("Please enter a valid email").MaximumLength(200);
        RuleFor(x => x.PhoneNumber).MaximumLength(50);
        RuleFor(x => x.HasAcceptedTerms).Equal(true).WithMessage("You need to accept the terms to save your profile");
    }
}
