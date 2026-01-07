using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Commands.AnalyzeCv;

public class AnalyzeCvRequestValidator : AbstractValidator<AnalyzeCvRequest>
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    public AnalyzeCvRequestValidator()
    {
        RuleFor(x => x.CvFile)
            .NotNull()
            .WithMessage("CV file is required");

        RuleFor(x => x.CvFile.Length)
            .GreaterThan(0)
            .WithMessage("CV file cannot be empty")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage($"CV file size cannot exceed {MaxFileSizeBytes / 1024 / 1024}MB");

        RuleFor(x => x.CvFile.ContentType)
            .Equal("application/pdf")
            .WithMessage("Only PDF files are supported");

        RuleFor(x => x.CvFile.FileName)
            .Must(fileName => fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            .WithMessage("File must have .pdf extension");
    }
}
