namespace Core.Entity;

public class UserCredential
{
    public int Id { get; set; } // Można rozważyć użycie UserId jako PK, ale zostawiamy Id na razie
    public int UserId { get; set; }
    public string PasswordHash { get; set; } = default!;
    public virtual User User { get; set; } = default!;
}