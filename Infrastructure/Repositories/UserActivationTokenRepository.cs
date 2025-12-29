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
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token && x.Type == type);
    }

    public async Task<List<UserActivationToken>> GetByUserIdAsync(int userId, string type)
    {
        return await Context.Set<UserActivationToken>()
            .Where(x => x.UserId == userId && x.Type == type)
            .ToListAsync();
    }
}