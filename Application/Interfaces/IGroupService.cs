using Application.DTOs;
using Core.Entity;

namespace Application.Interfaces;

public interface IGroupService
{
    /// <summary>
    ///     Pobiera użytkowników należących do grupy o podanym Id.
    /// </summary>
    /// <param name="id">Id grupy.</param>
    Task<List<User>> GetUsersFromGroup(int id);

    /// <summary>
    ///     Wyszukuje grupy po nazwie (pełnotekstowe).
    /// </summary>
    /// <param name="term">Fraza wyszukiwania.</param>
    /// <param name="limit">Maksymalna liczba wyników.</param>
    Task<List<Group>> SearchAsync(string term, int limit = 10);

    /// <summary>
    ///     Pobiera grupę po Id (bez użytkowników).
    /// </summary>
    /// <param name="id">Id grupy.</param>
    Task<Group?> GetByIdAsync(int id);

    /// <summary>
    ///     Wyszukuje grupy na potrzeby podpowiedzi (suggestions).
    ///     Dostępne wyłącznie dla ADMIN i LECTURER.
    /// </summary>
    /// <param name="term">Fraza wyszukiwania.</param>
    /// <param name="limit">Maksymalna liczba wyników.</param>
    Task<List<Group>> GetSuggestionsAsync(string term, int limit = 10);

    /// <summary>
    ///     Pobiera wszystkie grupy wraz z listą użytkowników.
    /// </summary>
    Task<List<Group>> GetAllAsync();

    /// <summary>
    ///     Pobiera grupę wraz z użytkownikami po Id.
    /// </summary>
    /// <param name="id">Id grupy.</param>
    Task<Group?> GetByIdWithUsersAsync(int id);

    /// <summary>
    ///     Pobiera wszystkie grupy zmapowane do <see cref="GroupDetailDto"/> (z listą użytkowników).
    /// </summary>
    Task<List<GroupDetailDto>> GetAllDetailAsync();

    /// <summary>
    ///     Pobiera szczegóły grupy po Id zmapowane do <see cref="GroupDetailDto"/>.
    ///     Zwraca null jeśli grupa nie istnieje.
    /// </summary>
    /// <param name="id">Id grupy.</param>
    Task<GroupDetailDto?> GetDetailByIdAsync(int id);

    /// <summary>
    ///     Aktualizuje nazwę i członków grupy, zwraca zaktualizowany <see cref="GroupDetailDto"/>.
    ///     Zwraca null jeśli grupa nie istnieje lub nazwa jest pusta.
    /// </summary>
    /// <param name="id">Id grupy.</param>
    /// <param name="name">Nowa nazwa grupy.</param>
    /// <param name="userIds">Lista Id użytkowników, którzy mają być członkami grupy.</param>
    Task<GroupDetailDto?> UpdateDetailAsync(int id, string name, List<int> userIds);

    /// <summary>
    ///     Aktualizuje grupę i zwraca encję <see cref="Group"/>.
    ///     Zwraca null jeśli grupa nie istnieje lub nazwa jest pusta.
    /// </summary>
    /// <param name="id">Id grupy.</param>
    /// <param name="name">Nowa nazwa grupy.</param>
    /// <param name="userIds">Lista Id użytkowników, którzy mają być członkami grupy.</param>
    Task<Group?> UpdateAsync(int id, string name, List<int> userIds);
}
