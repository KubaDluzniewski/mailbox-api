using System.Collections.Generic;

namespace Application.DTOs;

public class SendMessageDto
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<RecipientDto> Recipients { get; set; } = new();
}