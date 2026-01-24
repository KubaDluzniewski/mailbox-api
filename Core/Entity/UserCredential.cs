namespace Core.Entity;

public class UserCredential
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string PasswordHash { get; set; } = default!;
    public DateTime? PasswordChangedAt { get; set; }
    public virtual User User { get; set; } = default!;
}
