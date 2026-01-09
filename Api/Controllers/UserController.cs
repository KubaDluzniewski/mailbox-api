using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var result = await _userService.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword);
        if (result)
            return Ok("Password changed successfully.");
        return BadRequest("Failed to change password.");
    }

    [Authorize]
    [HttpPost("change-email")]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDto dto)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var result = await _userService.ChangeEmailAsync(userId, dto.NewEmail, dto.CurrentPassword);
        if (result)
            return Ok("Email changed successfully.");
        return BadRequest("Failed to change email.");
    }
}
