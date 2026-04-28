namespace Application.DTOs;

/// <summary>
///     Żądanie aktualizacji grupy przez administratora.
/// </summary>
public class UpdateGroupDto
{
    /// <summary>Nowa nazwa grupy.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Lista Id użytkowników, którzy mają być członkami grupy po aktualizacji.
    ///     Zastępuje całkowicie poprzednią listę członków.
    /// </summary>
    public List<int> UserIds { get; set; } = new();
}