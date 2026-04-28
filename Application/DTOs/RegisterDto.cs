namespace Application.DTOs;

/// <summary>
///     Dane rejestracji nowego użytkownika (samoobsługowa rejestracja z kodem).
///     Konto jest tworzone jako nieaktywne — wymaga potwierdzenia e-mailowego.
/// </summary>
public class RegisterDto
{
    /// <summary>Imię użytkownika.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Nazwisko użytkownika.</summary>
    public string Surname { get; set; } = string.Empty;

    /// <summary>Adres e-mail (musi być unikalny).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Hasło (zostanie zahaszowane).</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Kod rejestracyjny skonfigurowany po stronie serwera (zapobiega otwartej rejestracji).</summary>
    public string RegistrationCode { get; set; } = string.Empty;
}
