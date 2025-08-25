using Application.Interfaces;
using Core.Entity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;
    public MessageController(IMessageService messageService)
    {
        _messageService = messageService;
    }
    
    [Authorize]
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendMessageDto dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        var message = new Message
        {
            Subject = dto.Subject,
            Body = dto.Body,
            SenderId = int.Parse(userId),
            SentDate = DateTime.UtcNow,
            Recipients = dto.Recipients.Select(r => new MessageRecipient { UserId = r.UserId }).ToList()
        };
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