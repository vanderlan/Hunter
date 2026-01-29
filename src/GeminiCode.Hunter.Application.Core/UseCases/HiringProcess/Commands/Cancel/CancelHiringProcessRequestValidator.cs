using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.Cancel;

public class CancelHiringProcessRequestValidator : AbstractValidator<CancelHiringProcessRequest>
{
    public CancelHiringProcessRequestValidator()
    {
        RuleFor(x => x.PublicId)
            .NotEmpty()
            .WithMessage("Hiring Process ID is required");

        RuleFor(x => x.CancelReason)
            .NotEmpty()
            .WithMessage("Cancel reason is required")
            .MaximumLength(1000)
            .WithMessage("Cancel reason must not exceed 1000 characters");
    }
}
