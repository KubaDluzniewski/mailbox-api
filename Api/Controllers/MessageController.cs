using Application.DTOs;
using Application.Interfaces;
using Core.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AutoMapper;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly ISesEmailService _sesEmailService;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public MessageController(IMessageService messageService,
                             ISesEmailService sesEmailService,
                             IUserService userService,
                             IMapper mapper)
    {
        _messageService = messageService;
        _sesEmailService = sesEmailService;
        _userService = userService;
        _mapper = mapper;
    }

    [Authorize]
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendMessageDto? dto, CancellationToken cancellationToken)
    {
        if (dto == null) return BadRequest("Brak danych.");
        if (dto.Recipients.Count == 0) return BadRequest("Brak odbiorców.");

        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out var senderId)) return Unauthorized();

        var fromEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrWhiteSpace(fromEmail))
        {
            var sender = await _userService.GetByIdAsync(senderId);
            fromEmail = sender?.Email;
        }
        if (string.IsNullOrWhiteSpace(fromEmail))
            return BadRequest("Brak zdefiniowanego adresu nadawcy dla użytkownika.");

        var userDb = await _userService.GetByIdAsync(senderId);
        var fromDisplayName = $"{userDb?.Name} {userDb?.Surname}".Trim();

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

        var recipientIds = dto.Recipients.Select(r => r.UserId);
        var users = await _userService.GetByIdsAsync(recipientIds);

        try
        {
            foreach (var user in users)
            {
                if (string.IsNullOrWhiteSpace(user.Email)) continue;

                await _sesEmailService.SendEmailAsync(
                    fromDisplayName,
                    fromEmail,
                    user.Email!,
                    dto.Subject,
                    dto.Body,
                    cancellationToken
                );
            }
        }
        catch
        {
            return StatusCode(502, "Wysyłka e‑maili przez SES nie powiodła się. Wiadomość nie została zapisana.");
        }

        await _messageService.SendMessageAsync(message);
        return Ok();
    }
    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetForCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();
        var list = await _messageService.GetMessagesForUserAsync(int.Parse(userId));
        var dto = list.Select(m => _mapper.Map<MessageDto>(m)).ToList();
        return Ok(dto);
    }

    [Authorize]
    [HttpGet("sent")]
    public async Task<IActionResult> GetSentByCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();
        var list = await _messageService.GetMessagesSentByUserAsync(int.Parse(userId));
        var dto = list.Select(m => _mapper.Map<MessageDto>(m)).ToList();
        return Ok(dto);
    }
}