using Application.DTOs;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<string?> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<UserDto?> GetCurrentUserAsync();
}