using Core.Entity;

namespace Application.Interfaces;

public interface IUserService
{
    Task<User?> GetByIdAsync(int id);

    Task<List<User>> GetAllAsync();

    Task<User?> GetByEmailAsync(string email);

    Task<UserCredential?> GetCredentialByUserIdAsync(int userId);
}
