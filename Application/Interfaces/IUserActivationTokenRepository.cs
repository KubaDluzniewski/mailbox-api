using Core.Entity;

namespace Application.Interfaces;

public interface IUserActivationTokenRepository : IRepository<UserActivationToken>
{
    Task<UserActivationToken?> GetByTokenAsync(string token, string type);
    Task<List<UserActivationToken>> GetByUserIdAsync(int userId, string type);
}