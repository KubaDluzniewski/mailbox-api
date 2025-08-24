using Core.Entity;
using Infrastructure.Persistence;

namespace Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(MailboxDbContext context)
    {
        await UserSeeder.SeedAsync(context);
        await MessageSeeder.SeedAsync(context, await UserSeeder.SeedAsync(context));
    }
}
