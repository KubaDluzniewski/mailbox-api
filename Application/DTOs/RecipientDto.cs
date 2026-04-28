namespace Application.DTOs;

/// <summary>
///     Reprezentacja odbiorcy wiadomości — może być użytkownikiem lub grupą.
///     Używany zarówno w żądaniach (wybór odbiorców) jak i w odpowiedziach (lista odbiorców z metadanymi).
/// </summary>
public class RecipientDto
{
    /// <summary>Typ odbiorcy: <c>"user"</c> lub <c>"group"</c>.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Id użytkownika lub grupy.</summary>
    public int Id { get; set; }

    /// <summary>Wyświetlana nazwa odbiorcy (imię nazwisko lub nazwa grupy).</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Dodatkowy opis: e-mail dla użytkowników, opis dla grup (np. "Grupa - 5 użytkowników").</summary>
    public string? Subtitle { get; set; }

    /// <summary>Adres e-mail odbiorcy (wypełniany w wynikach wyszukiwania).</summary>
    public string? Email { get; set; }

    /// <summary>
    ///     Czy odbiorca przeczytał wiadomość.
    ///     Wypełniany tylko w widoku wysłanych i widoku admina.
    /// </summary>
    public bool? IsRead { get; set; }

    /// <summary>
    ///     Data i czas przeczytania wiadomości przez odbiorcę.
    ///     Wypełniany tylko w widoku wysłanych i widoku admina.
    /// </summary>
    public DateTime? ReadAt { get; set; }
}