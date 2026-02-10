using System;

namespace GeminiCode.Hunter.Domain.Core.Exceptions;

[Serializable]
public class ForbiddenException : BusinessException
{
    public ForbiddenException(string message, string title) : base(message, title)
    {

    }

    public ForbiddenException(string message) : base(message)
    {
    }

    public ForbiddenException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
