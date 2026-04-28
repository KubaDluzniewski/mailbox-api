using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Core.Entity;

namespace Application.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GroupService(IGroupRepository groupRepository, IUserService userService, IMapper mapper)
        {
            _groupRepository = groupRepository;
            _userService = userService;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public Task<List<User>> GetUsersFromGroup(int id)
            => _groupRepository.GetAllUsersAsyncByGroup(id);

        /// <inheritdoc/>
        public Task<List<Group>> SearchAsync(string term, int limit = 10)
            => _groupRepository.SearchAsync(term, limit);

        /// <inheritdoc/>
        public Task<Group?> GetByIdAsync(int id)
            => _groupRepository.GetByIdAsync(id);

        /// <inheritdoc/>
        public Task<List<Group>> GetSuggestionsAsync(string term, int limit = 10)
            => _groupRepository.SearchAsync(term, limit);

        /// <inheritdoc/>
        public Task<List<Group>> GetAllAsync()
            => _groupRepository.GetAllWithUsersAsync();

        /// <inheritdoc/>
        public Task<Group?> GetByIdWithUsersAsync(int id)
            => _groupRepository.GetByIdWithUsersAsync(id);

        /// <inheritdoc/>
        public async Task<List<GroupDetailDto>> GetAllDetailAsync()
        {
            var groups = await _groupRepository.GetAllWithUsersAsync();
            return groups.Select(MapToDetail).ToList();
        }

        /// <inheritdoc/>
        public async Task<GroupDetailDto?> GetDetailByIdAsync(int id)
        {
            var group = await _groupRepository.GetByIdWithUsersAsync(id);
            return group == null ? null : MapToDetail(group);
        }

        /// <inheritdoc/>
        public async Task<GroupDetailDto?> UpdateDetailAsync(int id, string name, List<int> userIds)
        {
            var group = await UpdateAsync(id, name, userIds);
            return group == null ? null : MapToDetail(group);
        }

        /// <inheritdoc/>
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

        private GroupDetailDto MapToDetail(Group group) => new()
        {
            Id = group.Id,
            Name = group.Name,
            Members = _mapper.Map<List<UserDto>>(group.Users),
            MemberCount = group.Users.Count
        };
    }
}
