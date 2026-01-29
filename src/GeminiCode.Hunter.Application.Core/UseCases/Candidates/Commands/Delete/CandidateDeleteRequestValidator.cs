using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.Delete;

public class CandidateDeleteRequestValidator : AbstractValidator<CandidateDeleteRequest>
{
    public CandidateDeleteRequestValidator()
    {
        RuleFor(c => c.PublicId)
            .NotEmpty()
            .WithMessage("PublicId is required");
    }
}
