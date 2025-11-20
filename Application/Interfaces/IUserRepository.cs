using Core.Entity;

namespace Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<List<User>> SearchBySurnameAsync(string term, int limit = 20);
    Task<List<User>> SearchAsync(string term, int limit = 10);
}
