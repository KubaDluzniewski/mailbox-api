namespace Core.Entity;

public class Attachment {
    /// <summary>
    ///  Id załącznika
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///  Nazwa pliku
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    ///  Typ pliku
    /// </summary>
    public required string ContentType { get; set; }

    /// <summary>
    ///  Rozmiar pliku
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    ///  Dane pliku
    /// </summary>
    public byte[] Data { get; set; } = null!;
}
