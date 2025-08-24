namespace Core.Entity;

public enum FolderType
{
    Inbox,
    Sent,
    Drafts,
    Trash,
}

public enum RecipientType
{
    To,
    Cc,
    Bcc
}

/// Powiązanie między wiadomością a odbiorcą, wraz z typem odbiorcy.
public class MessageRecipient
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public int UserId { get; set; }
    public RecipientType Type { get; set; }
    public FolderType Folder { get; set; } // Folder przypisany do odbiorcy
    public DateTime? ReadDate { get; set; } // Data przeczytania przez odbiorcę
    public Message Message { get; set; } = null!;
    public User User { get; set; } = null!;
}