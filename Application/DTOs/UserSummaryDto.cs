namespace Application.DTOs;

/// <summary>
///     Skrócone dane nadawcy wiadomości zwracane w widoku skrzynki.
/// </summary>
public class UserSummaryDto
{
    /// <summary>Id użytkownika.</summary>
    public int Id { get; set; }

    /// <summary>Imię użytkownika.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Nazwisko użytkownika.</summary>
    public string Surname { get; set; } = string.Empty;

    /// <summary>Adres e-mail użytkownika.</summary>
    public string Email { get; set; } = string.Empty;
}
