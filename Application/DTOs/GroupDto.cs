namespace Application.DTOs;

/// <summary>
///     Uproszczona reprezentacja grupy używana w podpowiedziach (suggestions)
///     i wynikach wyszukiwania.
/// </summary>
public class GroupDto
{
    /// <summary>Id grupy.</summary>
    public int Id { get; set; }

    /// <summary>Nazwa grupy.</summary>
    public string Name { get; set; } = string.Empty;
}
