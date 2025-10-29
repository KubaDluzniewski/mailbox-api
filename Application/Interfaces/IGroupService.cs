using Core.Entity;

namespace Application.Interfaces;

public interface IGroupService
{
    public Task<List<User>> GetUsersFromGroup(int id);
}
