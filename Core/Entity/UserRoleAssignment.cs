namespace Core.Entity;

/// <summary>
///     Przypisanie roli do użytkownika (tabela pośrednia many-to-many)
/// </summary>
public class UserRoleAssignment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public UserRole Role { get; set; }

    public virtual User User { get; set; } = default!;
}
