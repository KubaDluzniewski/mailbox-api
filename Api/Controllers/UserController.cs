using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public UserController(IUserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();
        return Ok(_mapper.Map<UserDto>(user));
    }
    
    [HttpGet("getSuggestion")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllOrSearch([FromQuery] string? name)
    {
        var list = await _userService.SearchBySurnameAsync(name, 10);
        return Ok(list.Select(u => _mapper.Map<UserDto>(u)));
    }
}
