using Application.Interfaces;
using Core.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class GroupRepository : BaseRepository<Group>, IGroupRepository
{
    public GroupRepository(MailboxDbContext context) : base(context) { }

    public async Task<List<Group>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.Distinct().ToList();
        return await Context.Groups
            .Include(g => g.Users)
            .Where(g => idList.Contains(g.Id))
            .ToListAsync();
    }

    public async Task<List<User>> GetAllUsersAsyncByGroup(int id)
    {
        var group = await Context.Groups
            .Include(g => g.Users)
            .SingleOrDefaultAsync(g => g.Id == id);
        return group?.Users.ToList() ?? new List<User>();
    }

    public async Task<List<Group>> SearchAsync(string term, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(term)) return new List<Group>();
        term = term.Trim();
        return await Context.Groups
            .Include(g => g.Users)
            .Where(g => EF.Functions.ILike(g.Name, $"%{term}%"))
            .OrderBy(g => g.Name)
            .Take(limit)
            .ToListAsync();
    }
}