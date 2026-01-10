using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Commands.Start;

public class StartHiringProcessRequestValidator : AbstractValidator<StartHiringProcessRequest>
{
    public StartHiringProcessRequestValidator()
    {
        RuleFor(x => x.CandidateId)
            .NotEmpty()
            .WithMessage("Candidate ID is required");

        RuleFor(x => x.JobPositionId)
            .NotEmpty()
            .WithMessage("Job Position ID is required");
    }
}
