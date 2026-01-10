using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.UpdateStatus;

public class UpdateHiringProcessStatusRequestValidator : AbstractValidator<UpdateHiringProcessStatusRequest>
{
    public UpdateHiringProcessStatusRequestValidator()
    {
        RuleFor(x => x.PublicId)
            .NotEmpty()
            .WithMessage("Public ID is required");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("Invalid status");
    }
}
