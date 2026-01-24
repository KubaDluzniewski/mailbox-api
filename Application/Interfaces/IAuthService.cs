using Application.DTOs;

namespace Application.Interfaces;

public interface IAuthService
{
    /// <summary>
    ///     Logowanie użytkownika
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<string?> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<UserDto?> GetCurrentUserAsync();
    Task<bool> IsActiveAsync(string email);
    Task<bool> ActivateAsync(string email);
    Task<string?> ConfirmAsync(string email, string token);
    Task<bool> InitiateEmailChangeAsync(int userId, string newEmail);
    Task<string?> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    Task<bool> RegisterAsync(string name, string surname, string email, string password, string registrationCode);
}
