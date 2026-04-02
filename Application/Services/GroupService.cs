using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces;
using Core.Entity;

namespace Application.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IUserService _userService;

        public GroupService(IGroupRepository groupRepository, IUserService userService)
        {
            _groupRepository = groupRepository;
            _userService = userService;
        }

        public Task<List<User>> GetUsersFromGroup(int id)
        {
            return _groupRepository.GetAllUsersAsyncByGroup(id);
        }

        public Task<List<Group>> SearchAsync(string term, int limit = 10)
        {
            return _groupRepository.SearchAsync(term, limit);
        }

        public Task<Group?> GetByIdAsync(int id)
        {
            return _groupRepository.GetByIdAsync(id);
        }

        public Task<List<Group>> GetSuggestionsAsync(string term, int limit = 10)
        {
            return _groupRepository.SearchAsync(term, limit);
        }

        public Task<List<Group>> GetAllAsync()
        {
            return _groupRepository.GetAllWithUsersAsync();
        }

        public Task<Group?> GetByIdWithUsersAsync(int id)
        {
            return _groupRepository.GetByIdWithUsersAsync(id);
        }

        public async Task<Group?> UpdateAsync(int id, string name, List<int> userIds)
        {
            var group = await _groupRepository.GetByIdWithUsersAsync(id);
            if (group == null)
                return null;

            var trimmedName = (name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
                return null;

            group.Name = trimmedName;

            var distinctUserIds = userIds.Distinct().ToList();
            var users = distinctUserIds.Count == 0
                ? new List<User>()
                : await _userService.GetByIdsAsync(distinctUserIds);

            group.Users = users;

            _groupRepository.Update(group);
            await _groupRepository.SaveChangesAsync();
            return group;
        }
    }
}
