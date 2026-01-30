using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Parameters.Queries.GetByKey;

public class ParameterGetByKeyRequest : IRequest<ParameterGetByKeyResponse?>
{
    public string Key { get; set; } = string.Empty;
}
