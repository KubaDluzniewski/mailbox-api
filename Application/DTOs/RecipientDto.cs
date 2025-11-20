namespace Application.DTOs;

public class RecipientDto
{
    public string Type { get; set; }
    public int Id { get; set; }
    public string DisplayName { get; set; }
    public string? Subtitle { get; set; }
    public string? Email { get; set; }
}