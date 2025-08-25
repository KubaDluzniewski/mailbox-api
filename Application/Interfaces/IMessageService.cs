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
    Task<List<Message>> GetMessagesSentByUserAsync(int userId); // nowa metoda
}