using Application.Interfaces;
using Core.Entity;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserCredential> _userCredentialRepository;

    public UserService(IRepository<User> userRepository, IRepository<UserCredential> userCredentialRepository)
    {
        _userRepository = userRepository;
        _userCredentialRepository = userCredentialRepository;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _userRepository.FindSingleAsync(u => u.Email == email);
    }

    public async Task<UserCredential?> GetCredentialByUserIdAsync(int userId)
    {
        return await _userCredentialRepository.FindSingleAsync(uc => uc.UserId == userId);
    }
}
