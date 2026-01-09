namespace Application.DTOs;

public class ActivateDto
{
    public string Email { get; set; } = string.Empty;
    public bool IsEmailChange { get; set; }
}
