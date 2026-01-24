using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Core.Entity;
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

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var user = await _userService.GetByIdAsync(userId);
        if (user == null) return NotFound();

        // Use UserDetailDto to include roles and active status
        return Ok(_mapper.Map<UserDetailDto>(user));
    }

    [HttpGet("getSuggestion")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllOrSearch([FromQuery] string? name)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var rolesClaims = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
        var userRoles = new List<UserRole>();
        foreach (var roleStr in rolesClaims)
        {
            if (Enum.TryParse<Core.Entity.UserRole>(roleStr, out var roleEnum))
            {
                userRoles.Add(roleEnum);
            }
        }

        List<Core.Entity.UserRole>? requiredRoles = null;
        bool isAdmin = userRoles.Contains(Core.Entity.UserRole.ADMIN);
        bool isLecturer = userRoles.Contains(Core.Entity.UserRole.LECTURER);

        if (!isAdmin && !isLecturer)
        {
            requiredRoles = new List<Core.Entity.UserRole> { Core.Entity.UserRole.ADMIN, Core.Entity.UserRole.LECTURER };
        }

        var list = await _userService.SearchBySurnameAsync(name, 10, requiredRoles);
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

    [Authorize]
    [HttpGet("password-changed-at")]
    public async Task<IActionResult> GetPasswordChangedAt()
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var credential = await _userService.GetCredentialByUserIdAsync(userId);
        if (credential == null) return NotFound();

        return Ok(new { passwordChangedAt = credential.PasswordChangedAt });
    }

    // Admin endpoints
    [Authorize(Roles = "ADMIN")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users.Select(u => _mapper.Map<UserDetailDto>(u)));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var user = await _userService.CreateUserAsync(
            dto.Name,
            dto.Surname,
            dto.Email,
            dto.Password,
            dto.Roles,
            dto.IsActive
        );

        if (user == null)
            return BadRequest("User with this email already exists.");

        return CreatedAtAction(nameof(Get), new { id = user.Id }, _mapper.Map<UserDetailDto>(user));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var user = await _userService.UpdateUserAsync(
            id,
            dto.Name,
            dto.Surname,
            dto.Email,
            dto.Roles,
            dto.IsActive
        );

        if (user == null)
            return BadRequest("Failed to update user. User not found or email already exists.");

        return Ok(_mapper.Map<UserDetailDto>(user));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result)
            return NotFound("User not found.");

        return NoContent();
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await _userService.ToggleUserStatusAsync(id);
        if (!result)
            return NotFound("User not found.");

        return Ok("User status toggled successfully.");
    }
}
