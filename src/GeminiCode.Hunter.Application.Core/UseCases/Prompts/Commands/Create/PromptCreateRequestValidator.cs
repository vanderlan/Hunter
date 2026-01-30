using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.Prompts.Commands.Create;

public class PromptCreateRequestValidator : AbstractValidator<PromptCreateRequest>
{
    public PromptCreateRequestValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .WithMessage("Key is required")
            .MaximumLength(100)
            .WithMessage("Key cannot exceed 100 characters")
            .Matches("^[A-Z_]+$")
            .WithMessage("Key must contain only uppercase letters and underscores");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Text is required")
            .MinimumLength(10)
            .WithMessage("Text must be at least 10 characters");
    }
}
