namespace Core.Entity;

/// <summary>
///     Rola użytkownika w systemie
/// </summary>
public enum UserRole
{
    /// <summary>
    ///     Administrator - pełny dostęp do systemu
    /// </summary>
    ADMIN,

    /// <summary>
    ///     Wykładowca - może wysyłać wiadomości do studentów i grup
    /// </summary>
    LECTURER,

    /// <summary>
    ///     Student - podstawowy użytkownik systemu
    /// </summary>
    STUDENT
}
