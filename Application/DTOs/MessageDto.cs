namespace Application.DTOs;

/// <summary>
///     Uproszczona reprezentacja wiadomości z listą Id odbiorców.
///     Używana wewnętrznie — do widoku klienta służy <see cref="MessageViewDto"/>.
/// </summary>
public class MessageDto
{
    /// <summary>Id wiadomości.</summary>
    public int Id { get; set; }

    /// <summary>Temat wiadomości.</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Treść wiadomości (opcjonalna).</summary>
    public string? Body { get; set; }

    /// <summary>Id nadawcy.</summary>
    public int SenderId { get; set; }

    /// <summary>Data i czas wysłania (UTC).</summary>
    public DateTime SentDate { get; set; }

    /// <summary>Lista Id użytkowników-odbiorców.</summary>
    public List<int> RecipientIds { get; set; } = new();
}
