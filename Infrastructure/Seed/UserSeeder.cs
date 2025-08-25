using Core.Entity;
using Infrastructure.Persistence;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Infrastructure.Seed;

public static class UserSeeder
{
    public static async Task<List<User>> SeedAsync(MailboxDbContext context)
    {
        if (context.Users.Any())
            return context.Users.ToList();

        var users = new List<User>
        {
            new()
            {
                Email = "alice@example.com",
                Name = "Alice",
                Surname = "Smith",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new()
            {
                Email = "bob@example.com",
                Name = "Bob",
                Surname = "Brown",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        // Hasła testowe
        var credentials = new List<UserCredential>();
        foreach (var user in users)
        {
            credentials.Add(new UserCredential
            {
                UserId = user.Id,
                PasswordHash = BCryptNet.HashPassword("Password123!")
            });
        }
        await context.UserCredentials.AddRangeAsync(credentials);
        await context.SaveChangesAsync();

        return users;
    }
}
