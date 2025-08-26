namespace Application.DTOs;

public class MessageDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Body { get; set; }
    public int SenderId { get; set; }
    public DateTime SentDate { get; set; }
    public List<int> RecipientIds { get; set; } = new();
}

