using Core.Entity;
using Infrastructure.Persistence;

namespace Infrastructure.Seed;

public static class UserSeeder
{
    public static async Task<List<User>> SeedAsync(MailboxDbContext context)
    {
        if (context.Users.Any())
            return context.Users.ToList();

        var users = new List<User>
        {
            new User
            {
                Id = 0,
                Email = "alice@example.com",
                Name = "Alice",
                Surname = "Smith",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new User
            {
                Id = 1,
                Email = "bob@example.com",
                Name = "Bob",
                Surname = "Brown",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
        
        var allUsers = context.Users.ToList();
        foreach (var user in allUsers)
        {
            var credential = new UserCredential
            {
                Id = user.Id,
                UserId = user.Id,
                Email = user.Email,
                Password = "password" + user.Id
            };
            context.UserCredentials.Add(credential);
        }
        
        await context.SaveChangesAsync();
        return allUsers;
    }
}
