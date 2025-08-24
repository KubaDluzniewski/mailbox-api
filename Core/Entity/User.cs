namespace Core.Entity;

/// <summary>
///     Encja użytkownika
/// </summary>
public class User
{
    /// <summary>
    ///     Id
    /// </summary>
    public int Id { get; set; }
    
    public required string Email { get; set; }

    /// <summary>
    ///     Imię
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Nazwisko
    /// </summary>
    public required string Surname { get; set; }

    /// <summary>
    ///     Ostatnia data logowania
    /// </summary>
    public required DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Czy konto już było zalogowane
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Grupy, do których należy użytkownik
    /// </summary>
    public ICollection<Group> Groups { get; set; } = new List<Group>();
    
    public virtual UserCredential Credential { get; set; } = default!;
}
