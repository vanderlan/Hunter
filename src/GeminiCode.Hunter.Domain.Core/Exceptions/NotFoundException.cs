using System;

namespace GeminiCode.Hunter.Domain.Core.Exceptions;

[Serializable]
public class NotFoundException : BusinessException
{
    public NotFoundException(string id) : base($"Resource Not Found with id: {id}")
    {
        
    }

    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// A 404 whose message is shown to end users as-is (no "Resource Not Found with id" prefix).
    /// Used by the public endpoints, where the visitor never sees an id.
    /// </summary>
    public static NotFoundException WithMessage(string message) => new(message, null);
}
