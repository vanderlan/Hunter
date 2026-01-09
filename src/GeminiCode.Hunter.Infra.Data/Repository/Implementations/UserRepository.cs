using Microsoft.EntityFrameworkCore;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeminiCode.Hunter.Domain.Core.Filters;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using GeminiCode.Hunter.Infra.Data.Context;
using GeminiCode.Hunter.Infra.Data.Repository.Generic;

namespace GeminiCode.Hunter.Infra.Data.Repository.Implementations;

public sealed class UserRepository(DataContext context) : BaseEntityRepository<User>(context), IUserRepository
{
    public async Task<User> LoginAsync(string email, string password)
    {
        var user = await DataContext.Users.AsNoTracking()
            .Where(x => x.Email.Equals(email) && x.Password.Equals(password))
            .FirstOrDefaultAsync();

        return user;
    }

    private static IQueryable<User> ApplyFilters(UserFilter filter, IQueryable<User> query)
    {
        if (!string.IsNullOrWhiteSpace(filter.Query))
            query = query.Where(x => x.Name.Contains(filter.Query));

        return query;
    }

    public async Task<User> FindByEmailAsync(string email)
    {
        return await DataContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email.Equals(email));
    }
    
    public async Task<PagedList<User>> ListPaginateAsync(UserFilter filter)
    {
        IQueryable<User> query = DataContext.Set<User>();

        query = ApplyFilters(filter, query);

        var pagination = (filter.Page * filter.Quantity) - filter.Quantity;

        var entityList = await query.OrderBy(x => x.CreatedAt)
            .AsNoTracking()
            .Skip(pagination)
            .Take(filter.Quantity)
            .ToListAsync();

        return new PagedList<User>(entityList, await query.CountAsync());
    }
    
    public async Task<List<User>> GetUsersWithJobNotificationsEnabledAsync()
    {
        return await DataContext.Users
            .AsNoTracking()
            .Where(u => u.ReceiveJobChangeNotifications)
            .ToListAsync();
    }
}
