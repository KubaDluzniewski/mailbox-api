using Core.Entity;

namespace Application.DTOs;

/// <summary>
///     DTO do tworzenia nowego użytkownika
/// </summary>
public class CreateUserDto
{
    /// <summary>
    ///     Imię użytkownika
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Nazwisko użytkownika
    /// </summary>
    public required string Surname { get; set; }

    /// <summary>
    ///     Email użytkownika
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    ///     Hasło użytkownika
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    ///     Rola użytkownika
    /// </summary>
    public UserRole Role { get; set; } = UserRole.STUDENT;

    /// <summary>
    ///     Czy użytkownik jest aktywny
    /// </summary>
    public bool IsActive { get; set; } = true;
}
