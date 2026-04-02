using Application.DTOs;
using Application.Interfaces;
using Core.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using AutoMapper;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IEmailService _sesEmailService;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IGroupRepository _groupRepository;

    public MessageController(IMessageService messageService,
                             IEmailService sesEmailService,
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
    [DisableRequestSizeLimit]
    public async Task<IActionResult> Send(
        [FromForm] string? subject,
        [FromForm] string? body,
        [FromForm] string? recipients,
        [FromForm] int? id,
        IFormFileCollection? attachments,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subject)) return BadRequest("Brak tematu.");

        List<RecipientDto> recipientList;
        try
        {
            recipientList = string.IsNullOrWhiteSpace(recipients)
                ? []
                : JsonSerializer.Deserialize<List<RecipientDto>>(recipients, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch
        {
            return BadRequest("Nieprawidłowy format odbiorców.");
        }

        if (recipientList.Count == 0) return BadRequest("Brak odbiorców.");

        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out var senderId)) return Unauthorized();

        var dto = new SendMessageDto
        {
            Id = id,
            Subject = subject,
            Body = body ?? string.Empty,
            Recipients = recipientList
        };

        var attachmentDtos = await ReadAttachmentsAsync(attachments);

        var response = await _messageService.SendMessages(dto, senderId, cancellationToken, attachmentDtos);
        if (!response) return StatusCode(500, "Nie udało się wysłać wiadomości");
        return Ok();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetForCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var list = await _messageService.GetMessagesForUserAsync(userId.Value);

        var result = list.Select(m => {
            var currentUserRecipient = m.Recipients.FirstOrDefault(r => r.RecipientEntityId == userId.Value && r.RecipientType == RecipientType.User);

            return new {
                m.Id,
                m.Subject,
                m.Body,
                Sender = new { m.Sender?.Id, m.Sender?.Name, m.Sender?.Surname, m.Sender?.Email },
                m.SentDate,
                CreatedAt = m.SentDate,
                Recipients = new[] {
                    new RecipientDto {
                        Id = m.Sender?.Id ?? 0,
                        Type = "user",
                        DisplayName = m.Sender?.FullName() ?? "Unknown",
                        Subtitle = m.Sender?.Email ?? ""
                    }
                }.ToList(),
                IsRead = currentUserRecipient?.IsRead ?? false,
                ReadAt = currentUserRecipient?.ReadAt,
                Attachments = m.Attachments.Select(a => new AttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    FileSize = a.FileSize
                }).ToList()
            };
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
            }).ToList(),
            Attachments = m.Attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize
            }).ToList()
        }).ToList();

        return Ok(result);
    }

    [Authorize]
    [HttpPost("draft")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> SaveDraft(
        [FromForm] string? subject,
        [FromForm] string? body,
        [FromForm] string? recipients,
        IFormFileCollection? attachments,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        List<RecipientDto> recipientList;
        try
        {
            recipientList = string.IsNullOrWhiteSpace(recipients)
                ? []
                : JsonSerializer.Deserialize<List<RecipientDto>>(recipients, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch
        {
            return BadRequest("Nieprawidłowy format odbiorców.");
        }

        var dto = new SendMessageDto
        {
            Subject = subject ?? string.Empty,
            Body = body ?? string.Empty,
            Recipients = recipientList
        };

        var attachmentDtos = await ReadAttachmentsAsync(attachments);

        var draft = await _messageService.SaveDraftAsync(dto, userId.Value, cancellationToken, attachmentDtos);

        var response = new
        {
            draft.Id,
            draft.Subject,
            draft.Body,
            draft.CreatedAt,
            Recipients = dto.Recipients,
            Attachments = draft.Attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize
            }).ToList()
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
            }).ToList(),
            Attachments = m.Attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize
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
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UpdateDraft(
        int id,
        [FromForm] string? subject,
        [FromForm] string? body,
        [FromForm] string? recipients,
        IFormFileCollection? attachments,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        List<RecipientDto> recipientList;
        try
        {
            recipientList = string.IsNullOrWhiteSpace(recipients)
                ? []
                : JsonSerializer.Deserialize<List<RecipientDto>>(recipients, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch
        {
            return BadRequest("Nieprawidłowy format odbiorców.");
        }

        var dto = new SendMessageDto
        {
            Subject = subject ?? string.Empty,
            Body = body ?? string.Empty,
            Recipients = recipientList
        };

        var attachmentDtos = await ReadAttachmentsAsync(attachments);

        var draft = await _messageService.UpdateDraftAsync(id, dto, userId.Value, cancellationToken, attachmentDtos.Count > 0 ? attachmentDtos : null);
        if (draft == null) return NotFound();

        var response = new
        {
            draft.Id,
            draft.Subject,
            draft.Body,
            Recipients = dto.Recipients,
            Attachments = draft.Attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize
            }).ToList()
        };

        return Ok(response);
    }

    [Authorize]
    [HttpGet("{messageId}/attachments/{attachmentId}")]
    public async Task<IActionResult> DownloadAttachment(int messageId, int attachmentId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var attachment = await _messageService.GetAttachmentAsync(messageId, attachmentId, userId.Value);
        if (attachment == null) return NotFound();

        return File(attachment.Data, attachment.ContentType, attachment.FileName);
    }

    [Authorize]
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _messageService.MarkAsReadAsync(id, userId.Value);
        if (!result) return NotFound();

        return Ok();
    }

    [Authorize]
    [HttpPut("{id}/unread")]
    public async Task<IActionResult> MarkAsUnread(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _messageService.MarkAsUnreadAsync(id, userId.Value);
        if (!result) return NotFound();

        return Ok();
    }

    [Authorize]
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var count = await _messageService.GetUnreadCountAsync(userId.Value);
        return Ok(new { count });
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllMessages()
    {
        var list = await _messageService.GetAllMessagesAsync();

        var userIds = list.SelectMany(m => m.Recipients.Select(r => r.RecipientEntityId)).Distinct().ToList();
        var users = await _userService.GetByIdsAsync(userIds);

        var result = list.Select(m => new {
            m.Id,
            m.Subject,
            m.Body,
            Sender = new { m.Sender?.Id, m.Sender?.Name, m.Sender?.Surname, m.Sender?.Email },
            m.SentDate,
            RecipientCount = m.Recipients.Count,
            Recipients = m.Recipients.Select(r => {
                var user = users.FirstOrDefault(u => u.Id == r.RecipientEntityId);
                return new RecipientDto {
                    Id = r.RecipientEntityId,
                    Type = "user",
                    DisplayName = user?.FullName() ?? "Unknown",
                    Subtitle = user?.Email ?? ""
                };
            }).ToList(),
            Attachments = m.Attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize
            }).ToList()
        }).ToList();

        return Ok(result);
    }

    private int? GetCurrentUserId()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out var userId))
            return userId;
        return null;
    }

    private static async Task<List<CreateAttachmentDto>> ReadAttachmentsAsync(IFormFileCollection? files)
    {
        if (files == null || files.Count == 0)
            return [];

        const long maxFileSizeBytes = 10 * 1024 * 1024; // 10 MB per file

        var result = new List<CreateAttachmentDto>();
        foreach (var file in files)
        {
            if (file.Length == 0) continue;
            if (file.Length > maxFileSizeBytes) continue;

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            result.Add(new CreateAttachmentDto
            {
                FileName = SanitizeFileName(file.FileName),
                ContentType = file.ContentType,
                FileSize = file.Length,
                Data = ms.ToArray()
            });
        }
        return result;
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        foreach (var invalid in Path.GetInvalidFileNameChars())
            name = name.Replace(invalid, '_');
        return name;
    }
}

