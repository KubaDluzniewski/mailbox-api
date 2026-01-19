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

    public async Task<User?> CreateUserAsync(string name, string surname, string email, string password, UserRole role, bool isActive = true)
    {
        // Check if user with this email already exists
        var existingUser = await _userRepository.FindSingleAsync(u => u.Email == email);
        if (existingUser != null) return null;

        var user = new User
        {
            Name = name,
            Surname = surname,
            Email = email,
            IsActive = isActive,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Create credentials
        var credential = new UserCredential
        {
            UserId = user.Id,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        await _userCredentialRepository.AddAsync(credential);
        await _userCredentialRepository.SaveChangesAsync();

        return user;
    }

    public async Task<User?> UpdateUserAsync(int userId, string? name, string? surname, string? email, UserRole? role, bool? isActive)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        if (!string.IsNullOrWhiteSpace(name))
            user.Name = name;

        if (!string.IsNullOrWhiteSpace(surname))
            user.Surname = surname;

        if (!string.IsNullOrWhiteSpace(email))
        {
            // Check if email is already taken by another user
            var existingUser = await _userRepository.FindSingleAsync(u => u.Email == email && u.Id != userId);
            if (existingUser != null) return null;
            user.Email = email;
        }

        if (role.HasValue)
            user.Role = role.Value;

        if (isActive.HasValue)
            user.IsActive = isActive.Value;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return user;
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        // Delete credentials first
        var credential = await _userCredentialRepository.FindSingleAsync(c => c.UserId == userId);
        if (credential != null)
        {
            _userCredentialRepository.Remove(credential);
            await _userCredentialRepository.SaveChangesAsync();
        }

        _userRepository.Remove(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ToggleUserStatusAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }
}
