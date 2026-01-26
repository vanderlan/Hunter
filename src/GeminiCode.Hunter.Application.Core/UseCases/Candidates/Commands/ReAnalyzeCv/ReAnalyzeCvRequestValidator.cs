using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.ReAnalyzeCv;

public class ReAnalyzeCvRequestValidator : AbstractValidator<ReAnalyzeCvRequest>
{
    public ReAnalyzeCvRequestValidator()
    {
        RuleFor(x => x.CandidateId)
            .NotEmpty()
            .WithMessage("Candidate ID is required");
    }
}
