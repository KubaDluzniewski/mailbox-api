namespace Application.DTOs;

public class SendMessageDto
{
    public string Subject { get; set; } = string.Empty;
    public string? Body { get; set; }
    public List<RecipientDto> Recipients { get; set; } = new();
}

public class RecipientDto
{
    public int UserId { get; set; }
}
