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
            .Where(m => m.Recipients.Any(r => r.RecipientEntityId == userId && r.RecipientType == RecipientType.User))
            .OrderByDescending(m => m.SentDate)
            .ToListAsync();
    }

    public async Task<List<Message>> GetMessagesSentByUserAsync(int userId)
    {
        return await Context.Set<Message>()
            .AsNoTracking()
            .Include(m => m.Recipients)
            .Include(m => m.Sender)
            .Where(m => m.SenderId == userId && !m.IsDraft) // Only sent messages, not drafts
            .OrderByDescending(m => m.SentDate)
            .ToListAsync();
    }

    public async Task<Message?> GetDraftWithRecipientsAsync(int draftId)
    {
        return await Context.Set<Message>()
            .Include(m => m.Recipients)
            .FirstOrDefaultAsync(m => m.Id == draftId);
    }

    public async Task<List<Message>> GetDraftsForUserWithRecipientsAsync(int userId)
    {
        return await Context.Set<Message>()
            .Include(m => m.Recipients)
            .Where(m => m.SenderId == userId && m.IsDraft)
            .ToListAsync();
    }

    public async Task<bool> MarkAsReadAsync(int messageId, int userId)
    {
        var recipient = await Context.Set<MessageRecipient>()
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.RecipientEntityId == userId && r.RecipientType == RecipientType.User);

        if (recipient == null) return false;

        recipient.IsRead = true;
        recipient.ReadAt = DateTime.UtcNow;
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkAsUnreadAsync(int messageId, int userId)
    {
        var recipient = await Context.Set<MessageRecipient>()
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.RecipientEntityId == userId && r.RecipientType == RecipientType.User);

        if (recipient == null) return false;

        recipient.IsRead = false;
        recipient.ReadAt = null;
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetUnreadCountForUserAsync(int userId)
    {
        return await Context.Set<MessageRecipient>()
            .Where(r => r.RecipientEntityId == userId && r.RecipientType == RecipientType.User && !r.IsRead)
            .CountAsync();
    }

    public async Task<List<Message>> GetAllBroadcastMessagesAsync()
    {
        return await Context.Set<Message>()
            .AsNoTracking()
            .Include(m => m.Sender)
            .Include(m => m.Recipients)
            .Where(m => !m.IsDraft && m.Recipients.Count >= 3) // Broadcast = 3 or more recipients
            .OrderByDescending(m => m.SentDate)
            .ToListAsync();
    }
}
