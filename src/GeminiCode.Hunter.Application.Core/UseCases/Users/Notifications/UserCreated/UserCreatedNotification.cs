using GeminiCode.Hunter.Domain.Core.Entities;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Users.Notifications.UserCreated
{
    public class UserCreatedNotification(User user, string plainPassword, bool sendWelcomeEmail) : INotification
    {
        public User Entity { get; set; } = user;
        public string PlainPassword { get; set; } = plainPassword;
        public bool SendWelcomeEmail { get; set; } = sendWelcomeEmail;
    }
}
