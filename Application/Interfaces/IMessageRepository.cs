using Core.Entity;

namespace Application.Interfaces;

public interface IMessageRepository : IRepository<Message>
{
    Task<List<Message>> GetMessagesForUserAsync(int userId);
    
    Task<List<Message>> GetMessagesSentByUserAsync(int userId);
}
