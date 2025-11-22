using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces;
using Core.Entity;

namespace Application.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;

        public GroupService(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
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
    }
}
