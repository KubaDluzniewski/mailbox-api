using Core.Entity;

namespace Application.Interfaces;

public interface IMessageService
{
    Task SendMessageAsync(Message message);
    Task<List<Message>> GetMessagesForUserAsync(int userId); // nowa metoda
    
    Task<List<Message>> GetMessagesSentByUserAsync(int userId); // nowa metoda
}