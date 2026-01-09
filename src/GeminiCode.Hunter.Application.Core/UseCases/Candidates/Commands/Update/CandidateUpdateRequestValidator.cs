using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.Update;

public class CandidateUpdateRequestValidator : AbstractValidator<CandidateUpdateRequest>
{
    public CandidateUpdateRequestValidator()
    {
        RuleFor(c => c.PublicId)
            .NotEmpty().WithMessage("PublicId is required");

        RuleFor(c => c.Email)
            .EmailAddress().When(c => !string.IsNullOrWhiteSpace(c.Email))
            .WithMessage("Invalid email format");

        RuleFor(c => c.PhoneNumber)
            .MaximumLength(50).WithMessage("Phone number cannot exceed 50 characters");

        RuleFor(c => c.Name)
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");
    }
}
