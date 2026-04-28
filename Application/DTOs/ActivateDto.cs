namespace Application.DTOs;

/// <summary>
///     Żądanie wysłania e-maila aktywacyjnego lub inicjacji zmiany adresu e-mail.
/// </summary>
public class ActivateDto
{
    /// <summary>
    ///     Adres e-mail, na który zostanie wysłany link aktywacyjny lub potwierdzający zmianę.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Gdy true, inicjuje procedurę zmiany adresu e-mail (wymaga zalogowania).
    ///     Gdy false, inicjuje aktywację konta.
    /// </summary>
    public bool IsEmailChange { get; set; }
}
