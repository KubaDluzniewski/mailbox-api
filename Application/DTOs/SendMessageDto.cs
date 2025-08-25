namespace Application.DTOs;

public class SendMessageDto
{
    /// <summary>
    ///     Tytuł wiadomości
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    ///     Zawartość wiadomości
    /// </summary>
    public string? Body { get; set; }
    
    /// <summary>
    ///     Odbiorcy wiadomości
    /// </summary>
    public List<RecipientDto> Recipients { get; set; } = new();
}

public class RecipientDto
{
    /// <summary>
    ///     Id użytkownika-odbiorcy
    /// </summary>
    public int UserId { get; set; }
}
