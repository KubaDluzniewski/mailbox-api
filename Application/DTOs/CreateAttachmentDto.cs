namespace Application.DTOs;

/// <summary>
///     Dane załącznika używane wewnętrznie przy tworzeniu/zapisywaniu wiadomości.
///     Zawiera pełne dane binarne pliku — nie jest zwracany do klienta.
/// </summary>
public class CreateAttachmentDto
{
    /// <summary>Oryginalna nazwa pliku (po sanityzacji).</summary>
    public required string FileName { get; set; }

    /// <summary>Typ MIME pliku, np. <c>image/png</c>.</summary>
    public required string ContentType { get; set; }

    /// <summary>Rozmiar pliku w bajtach.</summary>
    public long FileSize { get; set; }

    /// <summary>Surowe dane binarne pliku.</summary>
    public required byte[] Data { get; set; }
}
