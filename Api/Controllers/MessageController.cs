using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    [DisableRequestSizeLimit]
    public async Task<IActionResult> Send(
        [FromForm] string? subject,
        [FromForm] string? body,
        [FromForm] string? recipients,
        [FromForm] int? id,
        IFormFileCollection? attachments,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var response = await _messageService.SendFromFormAsync(subject, body, recipients, id, attachments, userId.Value, cancellationToken);
            if (!response) return StatusCode(500, "Nie udało się wysłać wiadomości");
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetForCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _messageService.GetInboxViewAsync(userId.Value);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("sent")]
    public async Task<IActionResult> GetSentByCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _messageService.GetSentViewAsync(userId.Value);
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

        try
        {
            var response = await _messageService.SaveDraftFromFormAsync(subject, body, recipients, attachments, userId.Value, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("drafts")]
    public async Task<IActionResult> GetDrafts()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _messageService.GetDraftsViewAsync(userId.Value);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("draft/{id}")]
    public async Task<IActionResult> GetDraft(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _messageService.GetDraftViewAsync(id, userId.Value);
        if (result == null) return NotFound();

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
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _messageService.DeleteSentMessageAsync(id, userId.Value);
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

        try
        {
            var response = await _messageService.UpdateDraftFromFormAsync(id, subject, body, recipients, attachments, userId.Value, cancellationToken);
            if (response == null) return NotFound();
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
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
        var result = await _messageService.GetAdminMessagesViewAsync();
        return Ok(result);
    }

    private int? GetCurrentUserId()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out var userId))
            return userId;
        return null;
    }
}
