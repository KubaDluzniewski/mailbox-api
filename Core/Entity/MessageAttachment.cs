namespace Core.Entity;

public class MessageAttachment
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSize { get; set; }
    public required byte[] Data { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public virtual Message? Message { get; set; }
}
