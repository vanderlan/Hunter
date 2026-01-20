using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using System;

namespace GeminiCode.Hunter.Domain.Core.Entities;

public class User : BaseEntity
{
    public long UserId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required UserProfile Profile { get; set; }
    public string Password { get; set; }
    public bool ReceiveJobChangeNotifications { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
}
