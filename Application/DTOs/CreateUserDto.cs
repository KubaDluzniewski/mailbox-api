using Core.Entity;

namespace Application.DTOs;

/// <summary>
///     Żądanie utworzenia nowego użytkownika przez administratora.
/// </summary>
public class CreateUserDto
{
    /// <summary>Imię użytkownika.</summary>
    public required string Name { get; set; }

    /// <summary>Nazwisko użytkownika.</summary>
    public required string Surname { get; set; }

    /// <summary>Adres e-mail użytkownika (musi być unikalny).</summary>
    public required string Email { get; set; }

    /// <summary>Hasło użytkownika (zostanie zahaszowane).</summary>
    public required string Password { get; set; }

    /// <summary>Lista ról przypisanych do użytkownika.</summary>
    public List<UserRole> Roles { get; set; } = new() { UserRole.STUDENT };

    /// <summary>
    ///     Czy konto jest aktywne od razu po utworzeniu.
    ///     Domyślnie true (tworzone przez admina konta są aktywne bez e-maila aktywacyjnego).
    /// </summary>
    public bool IsActive { get; set; } = true;
}
