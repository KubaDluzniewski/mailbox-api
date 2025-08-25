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
}