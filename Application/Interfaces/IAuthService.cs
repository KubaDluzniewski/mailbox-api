using Application.DTOs;

namespace Application.Interfaces;

public interface IAuthService
{
    /// <summary>
    ///     Logowanie użytkownika. Zwraca token JWT lub null gdy dane są nieprawidłowe.
    ///     Rzuca <see cref="UnauthorizedAccessException"/> gdy konto jest nieaktywne.
    /// </summary>
    /// <param name="email">Adres e-mail użytkownika.</param>
    /// <param name="password">Hasło użytkownika.</param>
    Task<string?> LoginAsync(string email, string password);

    /// <summary>
    ///     Wylogowanie użytkownika (obecnie no-op, token jest bezstanowy).
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    ///     Pobiera dane aktualnie zalogowanego użytkownika.
    /// </summary>
    Task<UserDto?> GetCurrentUserAsync();

    /// <summary>
    ///     Sprawdza czy konto przypisane do danego adresu e-mail jest aktywne.
    /// </summary>
    /// <param name="email">Adres e-mail użytkownika.</param>
    Task<bool> IsActiveAsync(string email);

    /// <summary>
    ///     Inicjuje aktywację konta — generuje token i wysyła e-mail z linkiem aktywacyjnym.
    ///     Zwraca false jeśli użytkownik nie istnieje lub jest już aktywny.
    /// </summary>
    /// <param name="email">Adres e-mail użytkownika.</param>
    Task<bool> ActivateAsync(string email);

    /// <summary>
    ///     Potwierdza aktywację konta lub zmianę adresu e-mail na podstawie tokenu.
    ///     Zwraca typ potwierdzenia ("activation" lub "email_change") lub null przy błędzie.
    /// </summary>
    /// <param name="email">Adres e-mail powiązany z tokenem.</param>
    /// <param name="token">Token potwierdzający.</param>
    Task<string?> ConfirmAsync(string email, string token);

    /// <summary>
    ///     Inicjuje zmianę adresu e-mail zalogowanego użytkownika.
    ///     Wysyła e-mail potwierdzający na nowy adres.
    ///     Rzuca wyjątek jeśli nowy e-mail jest zajęty lub taki sam jak aktualny (i konto aktywne).
    /// </summary>
    /// <param name="userId">Id zalogowanego użytkownika.</param>
    /// <param name="newEmail">Nowy adres e-mail.</param>
    Task<bool> InitiateEmailChangeAsync(int userId, string newEmail);

    /// <summary>
    ///     Inicjuje reset hasła — generuje token i wysyła e-mail z linkiem resetującym.
    ///     Zwraca null jeśli użytkownik nie istnieje.
    /// </summary>
    /// <param name="email">Adres e-mail użytkownika.</param>
    Task<string?> ForgotPasswordAsync(string email);

    /// <summary>
    ///     Resetuje hasło użytkownika na podstawie tokenu z e-maila.
    ///     Zwraca false jeśli token jest nieprawidłowy, wygasły lub e-mail się nie zgadza.
    /// </summary>
    /// <param name="email">Adres e-mail użytkownika.</param>
    /// <param name="token">Token resetujący hasło.</param>
    /// <param name="newPassword">Nowe hasło.</param>
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);

    /// <summary>
    ///     Rejestruje nowego użytkownika (jako STUDENT, nieaktywny) i wysyła e-mail aktywacyjny.
    ///     Wymaga poprawnego kodu rejestracyjnego skonfigurowanego po stronie serwera.
    ///     Zwraca false jeśli kod jest nieprawidłowy lub e-mail jest już zajęty.
    /// </summary>
    /// <param name="name">Imię użytkownika.</param>
    /// <param name="surname">Nazwisko użytkownika.</param>
    /// <param name="email">Adres e-mail.</param>
    /// <param name="password">Hasło.</param>
    /// <param name="registrationCode">Kod rejestracyjny.</param>
    Task<bool> RegisterAsync(string name, string surname, string email, string password, string registrationCode);
}
