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
    }
}
