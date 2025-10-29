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

        var response = await _messageService.SendMessages(dto, senderId, cancellationToken);
        if(!response) return StatusCode(500, "Nie udało się wysłać wiadomości");
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