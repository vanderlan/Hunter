using GeminiCode.Hunter.Crosscutting.Resources;
using GeminiCode.Hunter.Crosscutting.Resources.Messages;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Commands.Create;

public class JobPositionCreateRequestValidator : AbstractValidator<JobPositionCreateRequest>
{
    public JobPositionCreateRequestValidator(IStringLocalizer<HunterResources> stringLocalizer)
    {
        RuleFor(c => c)
            .NotNull()
            .WithMessage(stringLocalizer[MessagesKeys.JobPositionMessages.NullEntity]);

        RuleFor(c => c.Title)
            .NotEmpty().WithMessage(stringLocalizer[MessagesKeys.JobPositionMessages.EmptyTitle]);

        RuleFor(c => c.ExternalUrl)
            .NotEmpty().WithMessage(stringLocalizer[MessagesKeys.JobPositionMessages.EmptyExternalUrl]);
    }
}
