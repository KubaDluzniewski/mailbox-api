using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

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

    [HttpGet("suggestions")]
    [Authorize]
    public async Task<IActionResult> GetSuggestions([FromQuery] string name)
    {
        var rolesClaims = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);
        bool canAccess = rolesClaims.Any(r => r == "ADMIN" || r == "LECTURER");

        if (!canAccess)
        {
            return Ok(new List<GroupDto>());
        }

        var groups = await _groupService.GetSuggestionsAsync(name);
        var groupDtos = _mapper.Map<List<GroupDto>>(groups);
        return Ok(groupDtos);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var groups = await _groupService.GetAllAsync();
        var response = groups.Select(g => new GroupDetailDto
        {
            Id = g.Id,
            Name = g.Name,
            Members = _mapper.Map<List<UserDto>>(g.Users),
            MemberCount = g.Users.Count
        }).ToList();

        return Ok(response);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var group = await _groupService.GetByIdWithUsersAsync(id);
        if (group == null)
            return NotFound();

        var response = new GroupDetailDto
        {
            Id = group.Id,
            Name = group.Name,
            Members = _mapper.Map<List<UserDto>>(group.Users),
            MemberCount = group.Users.Count
        };

        return Ok(response);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGroupDto dto)
    {
        var group = await _groupService.UpdateAsync(id, dto.Name, dto.UserIds);
        if (group == null)
            return BadRequest("Failed to update group.");

        var response = new GroupDetailDto
        {
            Id = group.Id,
            Name = group.Name,
            Members = _mapper.Map<List<UserDto>>(group.Users),
            MemberCount = group.Users.Count
        };

        return Ok(response);
    }
}
