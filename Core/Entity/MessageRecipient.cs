namespace Core.Entity;

/// Powiązanie między wiadomością a odbiorcą, wraz z typem odbiorcy.
public class MessageRecipient
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public int UserId { get; set; }
    public Message? Message { get; set; }
    public User? User { get; set; }
}