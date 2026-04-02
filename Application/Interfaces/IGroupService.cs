using Core.Entity;

namespace Application.Interfaces;

public interface IGroupService
{
    Task<List<User>> GetUsersFromGroup(int id);
    Task<List<Group>> SearchAsync(string term, int limit = 10);
    Task<Group?> GetByIdAsync(int id);
    Task<List<Group>> GetSuggestionsAsync(string term, int limit = 10);
    Task<List<Group>> GetAllAsync();
    Task<Group?> GetByIdWithUsersAsync(int id);
    Task<Group?> UpdateAsync(int id, string name, List<int> userIds);
}
