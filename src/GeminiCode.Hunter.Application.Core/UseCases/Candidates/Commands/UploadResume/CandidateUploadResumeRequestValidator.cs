using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.UploadResume;

public class CandidateUploadResumeRequestValidator : AbstractValidator<CandidateUploadResumeRequest>
{
    public CandidateUploadResumeRequestValidator()
    {
        RuleFor(x => x.CandidateId)
            .NotEmpty()
            .WithMessage("Candidate ID is required");

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        RuleFor(x => x.File)
            .Must(file => file != null && file.Length > 0)
            .WithMessage("File cannot be empty")
            .When(x => x.File != null);

        RuleFor(x => x.File)
            .Must(file => file.ContentType == "application/pdf")
            .WithMessage("Only PDF files are allowed")
            .When(x => x.File != null);

        RuleFor(x => x.File)
            .Must(file => file.Length <= 10 * 1024 * 1024) // 10 MB
            .WithMessage("File size must not exceed 10 MB")
            .When(x => x.File != null);
    }
}
