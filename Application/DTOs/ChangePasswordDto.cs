namespace Application.DTOs;

/// <summary>
///     Żądanie zmiany hasła zalogowanego użytkownika.
/// </summary>
public class ChangePasswordDto
{
    /// <summary>Aktualne hasło użytkownika (wymagane do weryfikacji).</summary>
    public string OldPassword { get; set; } = string.Empty;

    /// <summary>Nowe hasło użytkownika.</summary>
    public string NewPassword { get; set; } = string.Empty;
}
