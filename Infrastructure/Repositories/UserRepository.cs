using Application.Interfaces;
using Core.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(MailboxDbContext context) : base(context) { }

    public async Task<List<User>> SearchBySurnameAsync(string term, int limit = 20, List<UserRole>? requiredRoles = null)
    {
        if (string.IsNullOrWhiteSpace(term)) return new List<User>();
        term = term.Trim();
        var query = Context.Users
            .Include(u => u.Roles)
            .Where(u => EF.Functions.ILike(u.Surname, $"%{term}%"));

        if (requiredRoles != null && requiredRoles.Count > 0)
        {
            query = query.Where(u => u.Roles.Any(r => requiredRoles.Contains(r.Role)));
        }

        return await query
            .OrderBy(u => u.Surname)
            .ThenBy(u => u.Name)
            .Take(limit)
            .ToListAsync();
    }

    public Task<List<User>> SearchAsync(string term, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(term)) return Task.FromResult(new List<User>());
        term = term.Trim();
        return Context.Users
            .Where(u =>
                EF.Functions.ILike(u.Surname, $"%{term}%") ||
                EF.Functions.ILike(u.Name, $"%{term}%") ||
                EF.Functions.ILike(u.Email, $"%{term}%"))
            .OrderBy(u => u.Surname)
            .ThenBy(u => u.Name)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<User?> GetByEmailWithRolesAsync(string email)
    {
        return await Context.Users
            .Include(u => u.Roles)
            .SingleOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<User>> GetAllWithRolesAsync()
    {
        return await Context.Users
            .Include(u => u.Roles)
            .ToListAsync();
    }

    public async Task<User?> GetByIdWithRolesAsync(int id)
    {
        return await Context.Users
            .Include(u => u.Roles)
            .SingleOrDefaultAsync(u => u.Id == id);
    }
}
