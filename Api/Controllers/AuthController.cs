using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> IsActive([FromQuery] string email)
    {
        var isActive = await _authService.IsActiveAsync(email);
        return Ok(isActive);
    }

    [HttpPost("activate")]
    public async Task<IActionResult> Activate([FromBody] ActivateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Brak adresu email.");

        try
        {
            var userId = GetCurrentUserId();
            bool result;
            if (dto.IsEmailChange)
            {
                if (!userId.HasValue) return Unauthorized("You must be logged in to change your email.");
                result = await _authService.InitiateEmailChangeAsync(userId.Value, dto.Email);
            }
            else
            {
                result = await _authService.ActivateAsync(dto.Email);
            }

            if (result)
                return Ok("Email aktywacyjny/zmieniający adres został wysłany.");

            return BadRequest("Błąd wysyłania emaila.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmDto dto)
    {
        var result = await _authService.ConfirmAsync(dto.Email, dto.Token);
        if (result != null)
            return Ok(new { message = "Success", type = result });
        return BadRequest("Błąd aktywacji/zmiany emaila.");
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var link = await _authService.ForgotPasswordAsync(dto.Email);
        // Note: Returning link strictly for debugging purposes since email service might not be configured
        return Ok(new { message = "Jeśli konto istnieje, wysłaliśmy link do resetu hasła." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var result = await _authService.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword);
        if (result)
            return Ok("Hasło zostało zresetowane.");
        return BadRequest("Błąd resetowania hasła (nieprawidłowy token lub email).");
    }

    private int? GetCurrentUserId()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out var userId))
            return userId;
        return null;
    }
}
