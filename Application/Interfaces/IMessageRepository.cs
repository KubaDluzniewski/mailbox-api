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
}
