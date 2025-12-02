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
                Users = new List<User>
                {
                    users[0], users[1]
                }
            },
            new Group
            {
                Name = "INF2022D",
                Users = new List<User>
                {
                    users[1]
                }
            }
        };


        context.Groups.AddRange(seedGroups);
        await context.SaveChangesAsync();
    }
}
