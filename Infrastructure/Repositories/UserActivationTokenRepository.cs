using Application.Interfaces;
using Core.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserActivationTokenRepository : BaseRepository<UserActivationToken>, IUserActivationTokenRepository
{
    public UserActivationTokenRepository(MailboxDbContext context) : base(context) { }

    public async Task<UserActivationToken?> GetByTokenAsync(string token, string type)
    {
        return await Context.Set<UserActivationToken>()
            .FirstOrDefaultAsync(x => x.Token == token && x.Type == type);
    }

    public async Task<UserActivationToken?> GetByTokenAsync(string token)
    {
        return await Context.Set<UserActivationToken>()
            .FirstOrDefaultAsync(x => x.Token == token);
    }

    public async Task<List<UserActivationToken>> GetByUserIdAsync(int userId, string type)
    {
        return await Context.Set<UserActivationToken>()
            .Where(x => x.UserId == userId && x.Type == type)
            .ToListAsync();
    }
}