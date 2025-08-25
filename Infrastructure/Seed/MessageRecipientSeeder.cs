using Core.Entity;
using Infrastructure.Persistence;

namespace Infrastructure.Seed;

public static class MessageRecipientSeeder
{
    public static async Task SeedAsync(MailboxDbContext context, List<User> users)
    {
        if (context.MessageRecipients.Any())
            return;

        var messages = context.Messages.ToList();
        if (!messages.Any() || users.Count < 2)
            return;

        var recipients = new List<MessageRecipient>
        {
            new MessageRecipient
            {
                MessageId = messages[0].Id,
                UserId = users[1].Id,
            },
            new MessageRecipient
            {
                MessageId = messages[1].Id,
                UserId = users[0].Id,
            }
        };

        context.MessageRecipients.AddRange(recipients);
        await context.SaveChangesAsync();
    }
}

