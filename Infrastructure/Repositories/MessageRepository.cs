using Application.Interfaces;
using Core.Entity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MessageRepository : BaseRepository<Message>, IMessageRepository
{
    public MessageRepository(MailboxDbContext context) : base(context) { }

    public async Task<List<Message>> GetMessagesForUserAsync(int userId)
    {
        return await Context.Set<Message>()
            .AsNoTracking()
            .Include(m => m.Sender)
            .Include(m => m.Recipients)
                .ThenInclude(r => r.User)
            .Where(m => m.SenderId == userId || m.Recipients.Any(r => r.UserId == userId))
            .OrderByDescending(m => m.SentDate)
            .ToListAsync();
    }
    
    public async Task<List<Message>> GetMessagesSentByUserAsync(int userId)
    {
        return await Context.Set<Message>()
            .AsNoTracking()
            .Include(m => m.Recipients)
                .ThenInclude(r => r.User)
            .Include(m => m.Sender)
            .Where(m => m.SenderId == userId)
            .OrderByDescending(m => m.SentDate)
            .ToListAsync();
    }
}
