namespace Core.Entity;

/// <summary>
///     Encja użytkownika
/// </summary>
public class User
{
    /// <summary>
    ///     Id
    /// </summary>
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required string Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public virtual UserCredential? UserCredential { get; set; }
    
    public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
    
    public virtual ICollection<MessageRecipient> ReceivedMessages { get; set; } = new List<MessageRecipient>();
}
