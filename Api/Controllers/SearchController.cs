using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IGroupService _groupService;
    private readonly IMapper _mapper;

    public SearchController(IUserService userService, IGroupService groupService, IMapper mapper)
    {
        _userService = userService;
        _groupService = groupService;
        _mapper = mapper;
    }

    [HttpGet("recipients")]
    public async Task<IActionResult> SearchRecipients([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(new List<RecipientDto>());
        }

        var users = await _userService.SearchAsync(q);
        var groups = await _groupService.SearchAsync(q);

        var result = new List<RecipientDto>();
        result.AddRange(users.Select(u => new RecipientDto
        {
            Id = u.Id,
            Type = "user",
            DisplayName = u.Email,
            Subtitle = $"{u.Name} {u.Surname}",
            Email = u.Email
        }));
        
        result.AddRange(groups.Select(g => new RecipientDto
        {
            Id = g.Id,
            Type = "group",
            DisplayName = g.Name,
            Subtitle = $"Grupa - {g.Users.Count} użytkowników"
        }));

        return Ok(result);
    }
}
