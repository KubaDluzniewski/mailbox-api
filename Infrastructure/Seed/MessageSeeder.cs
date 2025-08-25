using Core.Entity;
using Infrastructure.Persistence;

namespace Infrastructure.Seed;

public static class MessageSeeder
{
    public static async Task SeedAsync(MailboxDbContext context, List<User> users)
    {
        if (context.Messages.Any())
            return;

        var messages = new List<Message>
        {
            new Message
            {
                Subject = "Hello Bob",
                Body = "Cześć Bob!",
                SenderId = users[0].Id, // int
                SentDate = DateTime.UtcNow,
                Recipients = new List<MessageRecipient>
                {
                    new MessageRecipient
                    {
                        UserId = users[1].Id, // int
                    }
                }
            },
            new Message
            {
                Subject = "Re: Hello Bob",
                Body = "Cześć Alice!",
                SenderId = users[1].Id, // int
                SentDate = DateTime.UtcNow,
                Recipients = new List<MessageRecipient>
                {
                    new MessageRecipient
                    {
                        UserId = users[0].Id, // int
                    }
                }
            },
            new Message
            {
                Subject = "Załącznik dla Boba",
                Body = "Bob, przesyłam Ci ważny plik w załączniku.",
                SenderId = users[0].Id,
                SentDate = DateTime.UtcNow.AddMinutes(-30),
                Recipients = new List<MessageRecipient>
                {
                    new MessageRecipient
                    {
                        UserId = users[1].Id,
                    }
                },
            },
            new Message
            {
                Subject = "Spotkanie zespołu",
                Body = "Cześć, przypominam o jutrzejszym spotkaniu zespołu o 10:00.",
                SenderId = users[1].Id,
                SentDate = DateTime.UtcNow.AddHours(-2),
                Recipients = new List<MessageRecipient>
                {
                    new MessageRecipient
                    {
                        UserId = users[0].Id,
                    }
                }
            },
            new Message
            {
                Subject = "Notatka do siebie",
                Body = "Nie zapomnij wysłać raportu do końca tygodnia.",
                SenderId = users[0].Id,
                SentDate = DateTime.UtcNow.AddDays(-1),
                Recipients = new List<MessageRecipient>
                {
                    new MessageRecipient
                    {
                        UserId = users[0].Id,
                    }
                }
            }
        };

        context.Messages.AddRange(messages);
        await context.SaveChangesAsync();
    }
}
