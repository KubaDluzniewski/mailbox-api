using Application.Interfaces;
using Core.Entity;

namespace Application.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    public MessageService(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }
    
    public async Task SendMessageAsync(Message message)
    {
        if (message.SentDate == default)
            message.SentDate = DateTime.UtcNow;
        await _messageRepository.AddAsync(message);
        await _messageRepository.SaveChangesAsync();
    }

    public Task<List<Message>> GetMessagesForUserAsync(int userId)
        => _messageRepository.GetMessagesForUserAsync(userId);
    
    public Task<List<Message>> GetMessagesSentByUserAsync(int userId)
        => _messageRepository.GetMessagesSentByUserAsync(userId);
}