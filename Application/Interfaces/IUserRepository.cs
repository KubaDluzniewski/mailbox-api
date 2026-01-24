using Core.Entity;

namespace Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<List<User>> SearchBySurnameAsync(string term, int limit = 20, List<UserRole>? requiredRoles = null);
    Task<List<User>> SearchAsync(string term, int limit = 10);
    Task<User?> GetByEmailWithRolesAsync(string email);
    Task<List<User>> GetAllWithRolesAsync();
    Task<User?> GetByIdWithRolesAsync(int id);
}
