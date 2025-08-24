namespace Core.Entity;

/// <summary>
///     Encja grupy użytkowników
/// </summary>
public class Group
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Nawigacja do użytkowników w grupie
    public ICollection<User> Users { get; set; } = new List<User>();
}
