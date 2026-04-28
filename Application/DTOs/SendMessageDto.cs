namespace Application.DTOs;

/// <summary>
///     Dane wiadomości przekazywane do warstwy serwisu przy wysyłaniu lub tworzeniu wersji roboczej.
/// </summary>
public class SendMessageDto
{
    /// <summary>
    ///     Id wersji roboczej, z której pochodzi wiadomość.
    ///     Jeśli podane, wersja robocza zostanie usunięta po wysłaniu.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>Temat wiadomości.</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Treść wiadomości (HTML lub plain text).</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>Lista odbiorców (użytkownicy i/lub grupy).</summary>
    public List<RecipientDto> Recipients { get; set; } = new();
}
