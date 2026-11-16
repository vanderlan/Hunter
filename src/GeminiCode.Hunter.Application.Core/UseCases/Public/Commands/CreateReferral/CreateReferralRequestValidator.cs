using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.CreateReferral;

public class CreateReferralRequestValidator : AbstractValidator<CreateReferralRequest>
{
    public CreateReferralRequestValidator()
    {
        RuleFor(x => x.JobCode).NotEmpty().WithMessage("Job code is required").MaximumLength(20);
        RuleFor(x => x.Message).MaximumLength(1000).WithMessage("Message cannot exceed 1000 characters");
    }
}
