using System.Collections.Generic;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Filters;
using GeminiCode.Hunter.Domain.Core.Repositories.Base;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;

namespace GeminiCode.Hunter.Domain.Core.Repositories;

public interface IUserRepository : IBaseEntityRepository<User>
{
    Task<User> LoginAsync(string email, string password);
    Task<PagedList<User>> ListPaginateAsync(UserFilter filter);
    Task<User> FindByEmailAsync(string email);
    Task<List<User>> GetUsersWithJobNotificationsEnabledAsync();
    Task<List<User>> GetAdminUsersWithJobNotificationsEnabledAsync();
}
