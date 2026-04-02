namespace Application.DTOs;

public class CreateAttachmentDto
{
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSize { get; set; }
    public required byte[] Data { get; set; }
}
