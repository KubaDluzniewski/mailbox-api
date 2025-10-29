using Application.Interfaces;
using Core.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class GroupRepository : BaseRepository<Group>, IGroupRepository
    {
        public GroupRepository(MailboxDbContext context) : base(context)
        {
        }


        public async Task<List<Group>> GetByIdsAsync(IEnumerable<int> ids)
        {
            return await Context.Groups
                .Where(g => ids.Contains(g.Id))
                .Include(g => g.Users)
                .ToListAsync();
        }

        public async Task<List<User>> GetAllUsersAsyncByGroup(int id)
        {
            return await Context.Groups
                .Where(g => g.Id == id)
                .SelectMany(g => g.Users).ToListAsync();
        }
    }
}
