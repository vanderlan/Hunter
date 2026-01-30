using MediatR;
using System.Collections.Generic;

namespace GeminiCode.Hunter.Application.Core.UseCases.Parameters.Queries.GetAll;

public class ParameterGetAllRequest : IRequest<List<ParameterGetAllResponse>>
{
}
