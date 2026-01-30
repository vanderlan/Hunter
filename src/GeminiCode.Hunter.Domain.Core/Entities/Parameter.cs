namespace GeminiCode.Hunter.Domain.Core.Entities;

public class Parameter : BaseEntity
{
    public long ParameterId { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
}
