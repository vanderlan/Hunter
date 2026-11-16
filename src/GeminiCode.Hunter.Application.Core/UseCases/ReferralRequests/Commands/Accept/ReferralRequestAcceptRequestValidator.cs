using FluentValidation;

namespace GeminiCode.Hunter.Application.Core.UseCases.ReferralRequests.Commands.Accept;

public class ReferralRequestAcceptRequestValidator : AbstractValidator<ReferralRequestAcceptRequest>
{
    public ReferralRequestAcceptRequestValidator()
    {
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}
