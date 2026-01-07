using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Candidates.Queries.GetById;

public record CandidateGetByIdRequest(string PublicId) : IRequest<CandidateGetByIdResponse>;
