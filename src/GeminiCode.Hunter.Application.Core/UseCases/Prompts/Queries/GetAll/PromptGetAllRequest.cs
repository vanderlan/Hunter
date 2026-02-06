using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Prompts.Queries.GetAll;

public class PromptGetAllRequest : IRequest<List<PromptGetAllResponse>>
{
}
