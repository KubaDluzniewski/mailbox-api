using Application.DTOs;
using Application.Interfaces;
using Core.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly ISesEmailService _sesEmailService;
    private readonly IUserService _userService;

    public MessageController(IMessageService messageService,
                             ISesEmailService sesEmailService,
                             IUserService userService)
    {
        _messageService = messageService;
        _sesEmailService = sesEmailService;
        _userService = userService;
    }

    [Authorize]
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendMessageDto? dto, CancellationToken cancellationToken)
    {
        if (dto == null) return BadRequest("Brak danych.");
        if (dto.Recipients == null || dto.Recipients.Count == 0) return BadRequest("Brak odbiorców.");

        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdStr == null) return Unauthorized();
        var senderId = int.Parse(userIdStr);

        var fromEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrWhiteSpace(fromEmail))
        {
            var sender = await _userService.GetByIdAsync(senderId);
            fromEmail = sender?.Email;
        }
        if (string.IsNullOrWhiteSpace(fromEmail))
            return BadRequest("Brak zdefiniowanego adresu nadawcy dla użytkownika.");
        
        var userDb = await _userService.GetByIdAsync(senderId);
        var name = $"{userDb?.Name} {userDb?.Surname}" ;
        var message = new Message
        {
            Subject = dto.Subject,
            Body = dto.Body,
            SenderId = senderId,
            SentDate = DateTime.UtcNow,
            Recipients = dto.Recipients
                .Select(r => new MessageRecipient { UserId = r.UserId })
                .ToList()
        };

        await _messageService.SendMessageAsync(message);

        var recipientIds = dto.Recipients.Select(r => r.UserId);
        var users = await _userService.GetByIdsAsync(recipientIds);
        foreach (var user in users)
        {
            if (string.IsNullOrWhiteSpace(user.Email)) continue;
            await _sesEmailService.SendEmailAsync(name, fromEmail, user.Email!, dto.Subject, dto.Body, cancellationToken);
        }

        return Ok();
    }
    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetForCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();
        var list = await _messageService.GetMessagesForUserAsync(int.Parse(userId));
        return Ok(list);
    }

    [Authorize]
    [HttpGet("sent")]
    public async Task<IActionResult> GetSentByCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();
        var list = await _messageService.GetMessagesSentByUserAsync(int.Parse(userId));
        return Ok(list);
    }
}