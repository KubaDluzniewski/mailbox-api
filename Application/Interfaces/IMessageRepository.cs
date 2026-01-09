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
}
