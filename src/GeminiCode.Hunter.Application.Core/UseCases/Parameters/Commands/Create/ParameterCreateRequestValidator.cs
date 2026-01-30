using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.Parameters.Commands.Create;

public class ParameterCreateRequestValidator : AbstractValidator<ParameterCreateRequest>
{
    public ParameterCreateRequestValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .WithMessage("Key is required")
            .MaximumLength(100)
            .WithMessage("Key cannot exceed 100 characters")
            .Matches("^[A-Z_]+$")
            .WithMessage("Key must contain only uppercase letters and underscores");

        RuleFor(x => x.Value)
            .NotEmpty()
            .WithMessage("Value is required")
            .MaximumLength(500)
            .WithMessage("Value cannot exceed 500 characters");
    }
}
