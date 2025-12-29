namespace Application.DTOs;

public class AuthDto
{
    /// <summary>
    ///     Email użytkownika
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    ///     Hasło użytkownika
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

public class ConfirmDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class ActivateDto
{
    public string Email { get; set; } = string.Empty;
}