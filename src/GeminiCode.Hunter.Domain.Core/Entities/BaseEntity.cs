using System;

namespace GeminiCode.Hunter.Domain.Core.Entities;

public abstract class BaseEntity
{
    public string PublicId { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime CreatedAt { get; set; }
}