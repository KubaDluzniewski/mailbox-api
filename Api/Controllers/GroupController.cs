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
            return Ok(new List<GroupDto>());

        var groups = await _groupService.GetSuggestionsAsync(name);
        return Ok(_mapper.Map<List<GroupDto>>(groups));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _groupService.GetAllDetailAsync();
        return Ok(result);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _groupService.GetDetailByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGroupDto dto)
    {
        var result = await _groupService.UpdateDetailAsync(id, dto.Name, dto.UserIds);
        if (result == null) return BadRequest("Failed to update group.");
        return Ok(result);
    }
}
