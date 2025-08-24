namespace Core.Entity;

public class Message
{
    public int Id { get; set; }
    public required string Subject { get; set; }
    public string? Body { get; set; }
    public int SenderId { get; set; }
    public ICollection<MessageRecipient> Recipients { get; set; } = new List<MessageRecipient>();
    public DateTime SentDate { get; set; }
    public User Sender { get; set; } = null!;
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public int? ParentMessageId { get; set; }
    public Message? ParentMessage { get; set; }
    public ICollection<Message> Replies { get; set; } = new List<Message>();
}