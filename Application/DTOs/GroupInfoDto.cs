namespace Application.DTOs;

/// <summary>
///     Informacje o grupie z listą członków.
/// </summary>
public class GroupInfoDto
{
    /// <summary>Id grupy.</summary>
    public int Id { get; set; }

    /// <summary>Nazwa grupy.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Lista użytkowników należących do grupy.</summary>
    public List<UserDto> Members { get; set; } = new();

    /// <summary>Liczba członków grupy.</summary>
    public int MemberCount { get; set; }
}
