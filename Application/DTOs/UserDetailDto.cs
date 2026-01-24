namespace Application.DTOs;

/// <summary>
///     DTO szczegółowych informacji o użytkowniku
/// </summary>
public class UserDetailDto
{
    /// <summary>
    ///     Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Email użytkownika
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Imię użytkownika
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Nazwisko użytkownika
    /// </summary>
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    ///     Role użytkownika
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    ///     Czy użytkownik jest aktywny
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Data utworzenia konta
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
