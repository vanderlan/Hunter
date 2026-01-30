using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.Parameters.Commands.Update;

public class ParameterUpdateRequestValidator : AbstractValidator<ParameterUpdateRequest>
{
    public ParameterUpdateRequestValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty()
            .WithMessage("Value is required")
            .MaximumLength(500)
            .WithMessage("Value cannot exceed 500 characters");
    }
}
