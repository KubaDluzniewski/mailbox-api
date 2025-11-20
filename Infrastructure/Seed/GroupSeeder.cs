using Core.Entity;
using Infrastructure.Persistence;

namespace Infrastructure.Seed;

public static class GroupSeeder
{
    public static async Task SeedAsync(MailboxDbContext context, List<User> users)
    {
        var seedGroups = new List<Group>
        {
            new Group
            {
                Name = "INF2025Z",
            },
            new Group
            {
                Name = "INF2022D",
            }
        };


        context.Groups.AddRange(seedGroups);
        await context.SaveChangesAsync();
    }
}
