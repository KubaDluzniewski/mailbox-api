namespace Application.DTOs;

/// <summary>
///     Dane logowania użytkownika.
/// </summary>
public class AuthDto
{
    /// <summary>Adres e-mail użytkownika.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Hasło użytkownika.</summary>
    public string Password { get; set; } = string.Empty;
}