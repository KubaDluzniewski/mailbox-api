using Application.Interfaces;
using Core.Entity;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository; // changed type
    private readonly IRepository<UserCredential> _userCredentialRepository;

    public UserService(IUserRepository userRepository, IRepository<UserCredential> userCredentialRepository)
    {
        _userRepository = userRepository;
        _userCredentialRepository = userCredentialRepository;
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
}
