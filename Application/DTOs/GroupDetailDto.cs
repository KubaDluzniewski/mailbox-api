namespace Application.DTOs;

public class GroupDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<UserDto> Members { get; set; } = new();
    public int MemberCount { get; set; }
}