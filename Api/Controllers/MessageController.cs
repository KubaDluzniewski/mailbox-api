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
    private readonly IGroupRepository _groupRepository;

    public MessageController(IMessageService messageService,
                             ISesEmailService sesEmailService,
                             IUserService userService,
                             IMapper mapper,
                             IGroupRepository groupRepository)
    {
        _messageService = messageService;
        _sesEmailService = sesEmailService;
        _userService = userService;
        _mapper = mapper;
        _groupRepository = groupRepository;
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
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var list = await _messageService.GetMessagesForUserAsync(userId.Value);

        var userIds = list.SelectMany(m => m.Recipients.Select(r => r.RecipientEntityId)).Distinct().ToList();
        var users = await _userService.GetByIdsAsync(userIds);

        var result = list.Select(m => new {
            m.Id,
            m.Subject,
            m.Body,
            Sender = new { m.Sender?.Id, m.Sender?.Name, m.Sender?.Surname, m.Sender?.Email },
            m.SentDate,
            Recipients = m.Recipients.Select(r => {
                var user = users.FirstOrDefault(u => u.Id == r.RecipientEntityId);
                return new RecipientDto {
                    Id = r.RecipientEntityId,
                    Type = "user",
                    DisplayName = user?.FullName() ?? "Unknown",
                    Subtitle = user?.Email ?? ""
                };
            }).ToList()
        }).ToList();

        return Ok(result);
    }

    [Authorize]
    [HttpGet("sent")]
    public async Task<IActionResult> GetSentByCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var list = await _messageService.GetMessagesSentByUserAsync(userId.Value);

        var userIds = list.SelectMany(m => m.Recipients.Select(r => r.RecipientEntityId)).Distinct().ToList();
        var users = await _userService.GetByIdsAsync(userIds);

        var result = list.Select(m => new {
            m.Id,
            m.Subject,
            m.Body,
            Sender = new { m.Sender?.Id, m.Sender?.Name, m.Sender?.Surname, m.Sender?.Email },
            m.SentDate,
            Recipients = m.Recipients.Select(r => {
                var user = users.FirstOrDefault(u => u.Id == r.RecipientEntityId);
                return new RecipientDto {
                    Id = r.RecipientEntityId,
                    Type = "user",
                    DisplayName = user?.FullName() ?? "Unknown",
                    Subtitle = user?.Email ?? ""
                };
            }).ToList()
        }).ToList();

        return Ok(result);
    }

    [Authorize]
    [HttpPost("draft")]
    public async Task<IActionResult> SaveDraft([FromBody] SendMessageDto? dto, CancellationToken cancellationToken)
    {
        if (dto == null) return BadRequest("Brak danych.");

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var draft = await _messageService.SaveDraftAsync(dto, userId.Value, cancellationToken);

        var response = new
        {
            draft.Id,
            draft.Subject,
            draft.Body,
            draft.CreatedAt,
            Recipients = dto.Recipients
        };

        return Ok(response);
    }

    [Authorize]
    [HttpGet("drafts")]
    public async Task<IActionResult> GetDrafts()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var list = await _messageService.GetDraftsForUserAsync(userId.Value);

        var userIds = list.SelectMany(m => m.Recipients.Where(r => r.RecipientType == RecipientType.User).Select(r => r.RecipientEntityId)).Distinct().ToList();
        var groupIds = list.SelectMany(m => m.Recipients.Where(r => r.RecipientType == RecipientType.Group).Select(r => r.RecipientEntityId)).Distinct().ToList();

        var users = await _userService.GetByIdsAsync(userIds);
        var groups = await _groupRepository.GetByIdsAsync(groupIds);

        var result = list.Select(m => new {
            m.Id,
            m.Subject,
            m.Body,
            m.CreatedAt,
            Recipients = m.Recipients.Select(r => {
                if (r.RecipientType == RecipientType.User)
                {
                    var user = users.FirstOrDefault(u => u.Id == r.RecipientEntityId);
                    return new RecipientDto { Id = r.RecipientEntityId, Type = "user", DisplayName = user?.FullName() ?? "Unknown", Subtitle = user?.Email ?? "" };
                }
                else // Group
                {
                    var group = groups.FirstOrDefault(g => g.Id == r.RecipientEntityId);
                    return new RecipientDto { Id = r.RecipientEntityId, Type = "group", DisplayName = group?.Name ?? "Unknown", Subtitle = "Group" };
                }
            }).ToList()
        }).ToList();

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("draft/{id}")]
    public async Task<IActionResult> DeleteDraft(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();
        var result = await _messageService.DeleteDraftAsync(id, userId.Value);
        if (!result) return NotFound();
        return Ok();
    }

    [Authorize]
    [HttpPut("draft/{id}")]
    public async Task<IActionResult> UpdateDraft(int id, [FromBody] SendMessageDto? dto, CancellationToken cancellationToken)
    {
        if (dto == null) return BadRequest("Brak danych.");

        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();
        var draft = await _messageService.UpdateDraftAsync(id, dto, userId.Value, cancellationToken);
        if (draft == null) return NotFound();

        var response = new
        {
            draft.Id,
            draft.Subject,
            draft.Body,
            Recipients = dto.Recipients
        };

        return Ok(response);
    }

    private int? GetCurrentUserId()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out var userId))
            return userId;
        return null;
    }
}
