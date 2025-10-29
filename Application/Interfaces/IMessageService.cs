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
}