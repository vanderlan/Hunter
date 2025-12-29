using GeminiCode.Hunter.Domain.Core.Entities;
using MediatR;

namespace GeminiCode.Hunter.Application.Core.UseCases.Users.Notifications.UserCreated
{
    public class UserCreatedNotification(User user) : INotification
    {
        public User Entity { get; set; } = user;
    }
}
