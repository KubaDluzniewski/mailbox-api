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
                Email = "kubadluzniewskix@gmail.com",
                Name = "Kuba",
                Surname = "Dłużniewski",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new()
            {
                Email = "kubagierki123@gmail.com",
                Name = "Jakub",
                Surname = "Niedłużniewski",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

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

        // Create role assignments
        var roleAssignments = new List<UserRoleAssignment>
        {
            new() { UserId = users[0].Id, Role = UserRole.ADMIN },
            new() { UserId = users[1].Id, Role = UserRole.LECTURER }
        };
        await context.UserRoleAssignments.AddRangeAsync(roleAssignments);
        await context.SaveChangesAsync();

        return users;
    }
}
