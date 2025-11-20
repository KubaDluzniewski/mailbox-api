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
    Task<List<User>> SearchBySurnameAsync(string term, int limit = 20);

    /// <summary>
    ///     Wyszukiwanie użytkowników po imieniu, nazwisku lub emailu
    /// </summary>
    /// <param name="term"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    Task<List<User>> SearchAsync(string term, int limit = 10);
}
