using Core.Entity;

namespace Application.Interfaces;

public interface IGroupRepository : IRepository<Group>
{
    Task<List<Group>> GetByIdsAsync(IEnumerable<int> ids);

    Task<List<User>> GetAllUsersAsyncByGroup(int id);

    Task<List<Group>> SearchAsync(string term, int limit = 10);
}
