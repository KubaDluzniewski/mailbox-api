namespace Core.Entity;

public class UserCredential
{
    public int Id { get; set; }
    public int UserId { get; set; }
    
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    
    public virtual User User { get; set; } = default!;
}