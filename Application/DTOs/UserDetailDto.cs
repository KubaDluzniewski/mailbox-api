namespace Application.DTOs;

/// <summary>
///     Szczegółowe informacje o użytkowniku zwracane przez endpointy administracyjne
///     i endpoint <c>GET /api/users/me</c>.
/// </summary>
public class UserDetailDto
{
    /// <summary>Id użytkownika.</summary>
    public int Id { get; set; }

    /// <summary>Adres e-mail użytkownika.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Imię użytkownika.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Nazwisko użytkownika.</summary>
    public string Surname { get; set; } = string.Empty;

    /// <summary>Lista ról użytkownika jako ciągi tekstowe, np. <c>"ADMIN"</c>, <c>"LECTURER"</c>.</summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>Czy konto jest aktywne.</summary>
    public bool IsActive { get; set; }

    /// <summary>Data i czas utworzenia konta (UTC).</summary>
    public DateTime CreatedAt { get; set; }
}
