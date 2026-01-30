using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.Prompts.Commands.Update;

public class PromptUpdateRequestValidator : AbstractValidator<PromptUpdateRequest>
{
    public PromptUpdateRequestValidator()
    {
        RuleFor(x => x.PublicId)
            .NotEmpty()
            .WithMessage("PublicId is required");

        RuleFor(x => x.Name)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Text)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.Text))
            .WithMessage("Text cannot be empty when provided");
    }
}
