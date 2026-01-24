using Core.Entity;

namespace Application.Interfaces;

public interface IUserService
{
    /// <summary>
    ///     Pobieranie użytkownika po Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<User?> GetByIdAsync(int id);

    /// <summary>
    ///     Pobieranie wszystkich użytkowników
    /// </summary>
    /// <returns></returns>
    Task<List<User>> GetAllAsync();

    /// <summary>
    ///     Pobieranie użytkownika po emailu
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    ///     Pobieranie użytkownika po emailu wraz z rolami
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<User?> GetByEmailWithRolesAsync(string email);

    /// <summary>
    ///     Pobieranie danych uwierzytelniających użytkownika po Id użytkownika
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<UserCredential?> GetCredentialByUserIdAsync(int userId);

    /// <summary>
    ///     Pobieranie użytkowników po liście Id
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<List<User>> GetByIdsAsync(IEnumerable<int> ids);

    /// <summary>
    ///     Wyszukiwanie użytkowników po nazwisku
    /// </summary>
    /// <param name="term"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    Task<List<User>> SearchBySurnameAsync(string term, int limit = 20, List<UserRole>? requiredRoles = null);

    /// <summary>
    ///     Wyszukiwanie użytkowników po imieniu, nazwisku lub emailu
    /// </summary>
    /// <param name="term"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    Task<List<User>> SearchAsync(string term, int limit = 10);

    /// <summary>
    ///     Zmiana hasła użytkownika
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="oldPassword"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);

    /// <summary>
    ///     Ustawienie nowego hasła użytkownika (bez weryfikacji starego - dla resetu hasła)
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    Task<bool> SetPasswordAsync(int userId, string newPassword);

    /// <summary>
    ///     Zmiana adresu e-mail użytkownika
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="newEmail"></param>
    /// <param name="currentPassword"></param>
    /// <returns></returns>
    Task<bool> ChangeEmailAsync(int userId, string newEmail, string currentPassword);

    /// <summary>
    ///     Tworzenie nowego użytkownika (admin)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="surname"></param>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <param name="role"></param>
    /// <param name="isActive"></param>
    /// <returns></returns>
    Task<User?> CreateUserAsync(string name, string surname, string email, string password, List<UserRole> roles, bool isActive = true);

    /// <summary>
    ///     Aktualizacja użytkownika (admin)
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="name"></param>
    /// <param name="surname"></param>
    /// <param name="email"></param>
    /// <param name="role"></param>
    /// <param name="isActive"></param>
    /// <returns></returns>
    Task<User?> UpdateUserAsync(int userId, string? name, string? surname, string? email, List<UserRole>? roles, bool? isActive);

    /// <summary>
    ///     Usuwanie użytkownika (admin)
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<bool> DeleteUserAsync(int userId);

    /// <summary>
    ///     Przełączanie statusu aktywności użytkownika (admin)
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<bool> ToggleUserStatusAsync(int userId);
}
