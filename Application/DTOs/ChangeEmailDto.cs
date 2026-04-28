namespace Application.DTOs;

/// <summary>
///     Żądanie zmiany adresu e-mail zalogowanego użytkownika.
///     Wymaga potwierdzenia aktualnym hasłem.
/// </summary>
public class ChangeEmailDto
{
    /// <summary>Nowy adres e-mail.</summary>
    public string NewEmail { get; set; } = string.Empty;

    /// <summary>Aktualne hasło użytkownika (wymagane do autoryzacji zmiany).</summary>
    public string CurrentPassword { get; set; } = string.Empty;
}
