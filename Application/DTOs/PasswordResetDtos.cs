namespace Application.DTOs;

/// <summary>
///     Żądanie wysłania e-maila z linkiem do resetu hasła.
/// </summary>
public class ForgotPasswordDto
{
    /// <summary>Adres e-mail konta, dla którego inicjowany jest reset hasła.</summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
///     Żądanie ustawienia nowego hasła na podstawie tokenu z e-maila.
/// </summary>
public class ResetPasswordDto
{
    /// <summary>Adres e-mail konta (musi zgadzać się z tokenem).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Jednorazowy token resetujący hasło z e-maila.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Nowe hasło użytkownika (zostanie zahaszowane).</summary>
    public string NewPassword { get; set; } = string.Empty;
}
