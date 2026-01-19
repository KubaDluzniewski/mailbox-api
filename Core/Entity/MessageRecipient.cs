namespace Core.Entity;

/// <summary>
/// Represents the link between a message and a recipient, which can be a user or a group.
/// </summary>
public class MessageRecipient
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public int RecipientEntityId { get; set; }
    public RecipientType RecipientType { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    public virtual Message? Message { get; set; }
}
