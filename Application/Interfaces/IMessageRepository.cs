using Core.Entity;

namespace Application.Interfaces;

public interface IMessageRepository : IRepository<Message>
{
    /// <summary>
    ///     Pobiera wiadomości odebrane dla użytkownika
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<Message>> GetMessagesForUserAsync(int userId);

    /// <summary>
    ///     Pobiera wiadomości wysłane przez użytkownika
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<Message>> GetMessagesSentByUserAsync(int userId);

    /// <summary>
    ///     Pobiera draft z odbiorcami
    /// </summary>
    /// <param name="draftId"></param>
    /// <returns></returns>
    Task<Message?> GetDraftWithRecipientsAsync(int draftId);

    /// <summary>
    ///     Pobiera drafty użytkownika z odbiorcami
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<Message>> GetDraftsForUserWithRecipientsAsync(int userId);

    /// <summary>
    ///     Oznacza wiadomość jako przeczytaną dla użytkownika
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<bool> MarkAsReadAsync(int messageId, int userId);

    /// <summary>
    ///     Oznacza wiadomość jako nieprzeczytaną dla użytkownika
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<bool> MarkAsUnreadAsync(int messageId, int userId);

    /// <summary>
    ///     Pobiera liczbę nieprzeczytanych wiadomości dla użytkownika
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<int> GetUnreadCountForUserAsync(int userId);

    /// <summary>
    ///     Pobiera wszystkie wiadomości w systemie (tylko dla administratorów)
    /// </summary>
    /// <returns></returns>
    Task<List<Message>> GetAllMessagesAsync();
}
