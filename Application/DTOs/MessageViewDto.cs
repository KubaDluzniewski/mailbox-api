namespace Application.DTOs;

/// <summary>
///     Pełna reprezentacja wiadomości zwracana do klienta.
///     Używana w skrzynce odbiorczej, wysłanych, wersji roboczych i widoku admina.
/// </summary>
public class MessageViewDto
{
    /// <summary>Id wiadomości.</summary>
    public int Id { get; set; }

    /// <summary>Temat wiadomości.</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Treść wiadomości (opcjonalna, np. pusta wersja robocza).</summary>
    public string? Body { get; set; }

    /// <summary>Dane nadawcy. Null dla wersji roboczych.</summary>
    public UserSummaryDto? Sender { get; set; }

    /// <summary>Data i czas wysłania (UTC). Null dla wersji roboczych.</summary>
    public DateTime? SentDate { get; set; }

    /// <summary>Data i czas utworzenia. Ustawiana dla wersji roboczych.</summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    ///     Lista odbiorców. W skrzynce odbiorczej zawiera tylko nadawcę.
    ///     W wysłanych i wersji roboczej — faktycznych odbiorców.
    /// </summary>
    public List<RecipientDto> Recipients { get; set; } = new();

    /// <summary>
    ///     Czy wiadomość została przeczytana przez aktualnego użytkownika.
    ///     Null dla wersji roboczych i widoku admina.
    /// </summary>
    public bool? IsRead { get; set; }

    /// <summary>Data i czas odczytania wiadomości przez użytkownika. Null jeśli nieprzeczytana.</summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>Lista metadanych załączników (bez danych binarnych).</summary>
    public List<AttachmentDto> Attachments { get; set; } = new();

    /// <summary>Łączna liczba odbiorców. Wypełniane tylko w widoku admina.</summary>
    public int? RecipientCount { get; set; }
}
