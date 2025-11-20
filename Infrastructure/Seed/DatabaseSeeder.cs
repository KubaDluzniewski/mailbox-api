using Core.Entity;
using Infrastructure.Persistence;

namespace Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(MailboxDbContext context)
    {
        var users = await UserSeeder.SeedAsync(context);
        await MessageSeeder.SeedAsync(context, users);
        await MessageRecipientSeeder.SeedAsync(context, users);
        await GroupSeeder.SeedAsync(context, users);
        // Opcjonalnie: dodatkowe seedery w przyszłości
    }
}
