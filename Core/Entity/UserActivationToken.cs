using System;

namespace Core.Entity;

public class UserActivationToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Type { get; set; } = "activation"; // np. activation, reset, change-email

    public virtual User User { get; set; } = null!;
}