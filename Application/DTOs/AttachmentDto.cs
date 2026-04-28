namespace Application.DTOs;

/// <summary>
///     Metadane załącznika wiadomości zwracane do klienta (bez danych binarnych).
/// </summary>
public class AttachmentDto
{
    /// <summary>Id załącznika.</summary>
    public int Id { get; set; }

    /// <summary>Oryginalna nazwa pliku.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Typ MIME pliku, np. <c>application/pdf</c>.</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Rozmiar pliku w bajtach.</summary>
    public long FileSize { get; set; }
}
