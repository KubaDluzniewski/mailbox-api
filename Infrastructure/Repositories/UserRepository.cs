using Application.Interfaces;
using Core.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(MailboxDbContext context) : base(context) { }

    public async Task<List<User>> SearchBySurnameAsync(string term, int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(term)) return new List<User>();
        term = term.Trim();
        // Npgsql: użyj ILike dla case-insensitive
        return await Context.Users
            .Where(u => EF.Functions.ILike(u.Surname, $"%{term}%"))
            .OrderBy(u => u.Surname)
            .ThenBy(u => u.Name)
            .Take(limit)
            .ToListAsync();
    }
}

