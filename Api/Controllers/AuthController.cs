using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthDto dto)
    {
        var token = await _authService.LoginAsync(dto.Email, dto.Password);

        if (token == null)
            return BadRequest("Błąd logowania.");
        
        return Ok(new { token });
    }

    [HttpPut("isActive")]
    public async Task<IActionResult> IsActive([FromBody] ActivateDto dto)
    {
        var isActive = await _authService.IsActiveAsync(dto.Email);
        return Ok(isActive);
    }

    [HttpPost("activate")]
    public async Task<IActionResult> Activate([FromBody] ActivateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Brak adresu email.");
        var result = await _authService.ActivateAsync(dto.Email);
        if (result)
            return Ok("Email aktywacyjny został wysłany.");
        return BadRequest("Błąd wysyłania emaila.");
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmDto dto)
    {
        var result = await _authService.ConfirmAsync(dto.Email, dto.Token);
        if (result)
            return Ok("Konto zostało aktywowane.");
        return BadRequest("Błąd aktywacji.");
    }
}