namespace Core.Entity;

public class Message
{
    public int Id { get; set; }
    public required string Subject { get; set; }
    public string? Body { get; set; }
    public int SenderId { get; set; }
    public DateTime? SentDate { get; set; }
    public bool IsDraft { get; set; } = false;
    
    public virtual User? Sender { get; set; }
    public virtual ICollection<MessageRecipient> Recipients { get; set; } = new List<MessageRecipient>();
}