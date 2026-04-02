namespace Application.DTOs;

public class UpdateGroupDto
{
    public string Name { get; set; } = string.Empty;
    public List<int> UserIds { get; set; } = new();
}