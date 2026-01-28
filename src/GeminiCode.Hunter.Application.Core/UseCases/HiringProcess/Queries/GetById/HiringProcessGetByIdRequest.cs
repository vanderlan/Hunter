using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetById;

public class HiringProcessGetByIdRequest(string publicId) : IRequest<HiringProcessGetByIdResponse>
{
    public string PublicId { get; } = publicId;
}
