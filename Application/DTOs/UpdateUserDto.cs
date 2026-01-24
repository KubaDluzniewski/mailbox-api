using Core.Entity;

namespace Application.DTOs;

/// <summary>
///     DTO do aktualizacji użytkownika
/// </summary>
public class UpdateUserDto
{
    /// <summary>
    ///     Imię użytkownika
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Nazwisko użytkownika
    /// </summary>
    public string? Surname { get; set; }

    /// <summary>
    ///     Email użytkownika
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    ///     Role użytkownika
    /// </summary>
    public List<UserRole>? Roles { get; set; }

    /// <summary>
    ///     Czy użytkownik jest aktywny
    /// </summary>
    public bool? IsActive { get; set; }
}
