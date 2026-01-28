namespace GeminiCode.Hunter.Domain.Core.Filters;

public abstract class BaseFilter
{
    public int Page { get; init; } = 1;
    public int Quantity { get; init; } = 25;
    public string Query { get; init; }
}
