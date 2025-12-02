using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Core.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserService _userService;
        private readonly ISesEmailService _sesEmailService;
        private readonly IGroupRepository _groupRepository;

        public MessageService(IMessageRepository messageRepository, IUserService userService, ISesEmailService sesEmailService, IGroupRepository groupRepository)
        {
            _messageRepository = messageRepository;
            _userService = userService;
            _sesEmailService = sesEmailService;
            _groupRepository = groupRepository;
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

        public async Task<bool> SendMessages(SendMessageDto dto, int senderId, CancellationToken cancellationToken)
        {
            var userDb = await _userService.GetByIdAsync(senderId);
            if (userDb == null)
                return false;

            var groupIds = dto.Recipients.Where(r => r.Type == "group").Select(r => r.Id).ToList();
            var groupUsers = new List<int>();
            if (groupIds.Any())
            {
                var groups = await _groupRepository.GetByIdsAsync(groupIds);
                groupUsers = groups.SelectMany(g => g.Users.Select(u => u.Id)).ToList();
            }

            var recipientIds = dto.Recipients.Where(r => r.Type == "user").Select(r => r.Id).Concat(groupUsers).Distinct().ToList();

            var message = new Message
            {
                Subject = dto.Subject,
                Body = dto.Body,
                SenderId = senderId,
                SentDate = DateTime.UtcNow,
                Recipients = recipientIds
                    .Select(r => new MessageRecipient { UserId = r })
                    .ToList()
            };

            var users = await _userService.GetByIdsAsync(recipientIds);

            try
            {
                foreach (var user in users)
                {
                    if (string.IsNullOrWhiteSpace(user.Email)) continue;

                    await _sesEmailService.SendEmailAsync(
                        userDb.FullName(),
                        userDb.Email,
                        user.Email!,
                        dto.Subject,
                        dto.Body + @"<br/><hr style=""border:none; border-top:1px solid #e0e0e0; margin:20px 0;"">
<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""font-family:Arial, Helvetica, sans-serif; font-size:12px; color:#666; width:100%;"">
  <tr>
    <td align=""center"" style=""padding-top:4px;"">
      Wysłano przy użyciu aplikacji <strong style=""color:#1a73e8;"">Mailbox</strong>
    </td>
  </tr>
</table>",
                        cancellationToken
                    );
                }
            }
            catch
            {
                return false;
            }

            await SendMessageAsync(message);
            return true;
        }
    }
}
