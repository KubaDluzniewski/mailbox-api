using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/groups")]
public class GroupController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly IMapper _mapper;

    public GroupController(IGroupService groupService, IMapper mapper)
    {
        _groupService = groupService;
        _mapper = mapper;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGroupDetails(int id)
    {
        var group = await _groupService.GetByIdAsync(id);
        if (group == null)
        {
            return NotFound();
        }

        var users = await _groupService.GetUsersFromGroup(id);

        var groupInfo = new GroupInfoDto
        {
            Id = group.Id,
            Name = group.Name,
            Members = _mapper.Map<List<UserDto>>(users),
            MemberCount = users.Count
        };

        return Ok(groupInfo);
    }
}
