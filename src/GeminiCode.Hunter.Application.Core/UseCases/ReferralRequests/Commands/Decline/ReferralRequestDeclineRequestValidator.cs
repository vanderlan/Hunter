using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.ReferralRequests.Commands.Decline;

public class ReferralRequestDeclineRequestValidator : AbstractValidator<ReferralRequestDeclineRequest>
{
    public ReferralRequestDeclineRequestValidator()
    {
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}
