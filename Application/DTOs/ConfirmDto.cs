namespace Application.DTOs;

/// <summary>
///     Żądanie potwierdzenia aktywacji konta lub zmiany adresu e-mail
///     na podstawie tokenu przesłanego e-mailem.
/// </summary>
public class ConfirmDto
{
    /// <summary>Adres e-mail powiązany z tokenem (aktywowany lub zmieniany).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Jednorazowy token potwierdzający z e-maila.</summary>
    public string Token { get; set; } = string.Empty;
}
