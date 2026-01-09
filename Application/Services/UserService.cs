using Application.Interfaces;
using Core.Entity;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository<UserCredential> _userCredentialRepository;

    public UserService(IUserRepository userRepository, IRepository<UserCredential> userCredentialRepository)
    {
        _userRepository = userRepository;
        _userCredentialRepository = userCredentialRepository;
    }

    public async Task<bool> ChangeEmailAsync(int userId, string newEmail, string currentPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        var credential = await _userCredentialRepository.FindSingleAsync(c => c.UserId == userId);
        if (credential == null) return false;

        var passwordValid = BCrypt.Net.BCrypt.Verify(currentPassword, credential.PasswordHash);
        if (!passwordValid) return false;

        var existingUser = await _userRepository.FindSingleAsync(u => u.Email == newEmail);
        if (existingUser != null) return false;

        user.Email = newEmail;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<User?> GetByIdAsync(int id) => await _userRepository.GetByIdAsync(id);

    public async Task<List<User>> GetAllAsync() => await _userRepository.GetAllAsync();

    public async Task<User?> GetByEmailAsync(string email) => await _userRepository.FindSingleAsync(u => u.Email == email);

    public async Task<UserCredential?> GetCredentialByUserIdAsync(int userId) => await _userCredentialRepository.FindSingleAsync(uc => uc.UserId == userId);

    public async Task<List<User>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.Distinct().ToList();
        return await _userRepository.FindAsync(u => idList.Contains(u.Id));
    }

    public Task<List<User>> SearchBySurnameAsync(string term, int limit = 20) => _userRepository.SearchBySurnameAsync(term, limit);

    public Task<List<User>> SearchAsync(string term, int limit = 10) => _userRepository.SearchAsync(term, limit);

    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var credential = await _userCredentialRepository.FindSingleAsync(c => c.UserId == userId);
        if (credential == null) return false;

        var passwordValid = BCrypt.Net.BCrypt.Verify(oldPassword, credential.PasswordHash);
        if (!passwordValid) return false;

        credential.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _userCredentialRepository.Update(credential);
        await _userCredentialRepository.SaveChangesAsync();
        return true;
    }
}
