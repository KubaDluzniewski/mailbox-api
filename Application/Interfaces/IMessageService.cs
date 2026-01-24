using Application.DTOs;
using Core.Entity;

namespace Application.Interfaces;

public interface IMessageService
{
    /// <summary>
    ///     Wysyłanie wiadomości
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task SendMessageAsync(Message message);

    /// <summary>
    ///     Pobranie wiadomości dla użytkownika odebranych
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<Message>> GetMessagesForUserAsync(int userId);

    /// <summary>
    ///     Pobranie wiadomości wysłanych przez użytkownika
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<Message>> GetMessagesSentByUserAsync(int userId);

    /// <summary>
    ///     Wysłanie wiadomości ses + baza
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="senderId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> SendMessages(SendMessageDto dto, int senderId, CancellationToken cancellationToken);

    Task<Message?> UpdateDraftAsync(int draftId, SendMessageDto dto, int senderId, CancellationToken cancellationToken);
    Task<Message> SaveDraftAsync(SendMessageDto dto, int senderId, CancellationToken cancellationToken);
    Task<List<Message>> GetDraftsForUserAsync(int userId);
    Task<bool> DeleteDraftAsync(int draftId, int userId);

    /// <summary>
    ///     Oznacza wiadomość jako przeczytaną
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<bool> MarkAsReadAsync(int messageId, int userId);

    /// <summary>
    ///     Oznacza wiadomość jako nieprzeczytaną
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<bool> MarkAsUnreadAsync(int messageId, int userId);

    /// <summary>
    ///     Pobiera liczbę nieprzeczytanych wiadomości
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<int> GetUnreadCountAsync(int userId);

    /// <summary>
    ///     Pobiera wszystkie wiadomości w systemie - tylko dla administratorów
    /// </summary>
    /// <returns></returns>
    Task<List<Message>> GetAllMessagesAsync();
}
