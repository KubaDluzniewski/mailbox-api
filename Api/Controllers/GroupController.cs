using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
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
    [Microsoft.AspNetCore.Authorization.Authorize]
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
}
