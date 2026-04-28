using Core.Entity;

namespace Application.DTOs;

/// <summary>
///     Żądanie aktualizacji danych użytkownika przez administratora.
///     Wszystkie pola są opcjonalne — null oznacza brak zmiany.
/// </summary>
public class UpdateUserDto
{
    /// <summary>Nowe imię użytkownika. Null = bez zmiany.</summary>
    public string? Name { get; set; }

    /// <summary>Nowe nazwisko użytkownika. Null = bez zmiany.</summary>
    public string? Surname { get; set; }

    /// <summary>Nowy adres e-mail użytkownika (musi być unikalny). Null = bez zmiany.</summary>
    public string? Email { get; set; }

    /// <summary>Nowa lista ról użytkownika (zastępuje poprzednią). Null = bez zmiany.</summary>
    public List<UserRole>? Roles { get; set; }

    /// <summary>Nowy status aktywności konta. Null = bez zmiany.</summary>
    public bool? IsActive { get; set; }
}
