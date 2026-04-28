namespace Application.DTOs;

/// <summary>
///     Szczegółowe informacje o grupie wraz z pełną listą członków.
///     Używany w endpointach administracyjnych.
/// </summary>
public class GroupDetailDto
{
    /// <summary>Id grupy.</summary>
    public int Id { get; set; }

    /// <summary>Nazwa grupy.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Lista użytkowników należących do grupy.</summary>
    public List<UserDto> Members { get; set; } = new();

    /// <summary>Liczba członków grupy (odpowiada <c>Members.Count</c>).</summary>
    public int MemberCount { get; set; }
}