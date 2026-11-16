using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.ConfirmPublicCandidateEmail;

public class ConfirmPublicCandidateEmailRequest : IRequest<ConfirmPublicCandidateEmailResponse>
{
    public required string Token { get; set; }
}
