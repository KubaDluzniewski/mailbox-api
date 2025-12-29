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
    Task<bool> ConfirmAsync(string email, string token);
}