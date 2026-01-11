using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.AddNote;

public class AddHiringProcessNoteRequestValidator : AbstractValidator<AddHiringProcessNoteRequest>
{
    public AddHiringProcessNoteRequestValidator()
    {
        RuleFor(x => x.Note)
            .NotEmpty()
            .WithMessage("Note is required")
            .MaximumLength(2000)
            .WithMessage("Note must not exceed 2000 characters");
    }
}
