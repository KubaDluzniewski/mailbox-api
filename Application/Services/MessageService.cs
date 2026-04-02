using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Email;
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
        private readonly IEmailService _sesEmailService;
        private readonly IGroupRepository _groupRepository;

        public MessageService(IMessageRepository messageRepository, IUserService userService, IEmailService sesEmailService, IGroupRepository groupRepository)
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

        public async Task<bool> SendMessages(SendMessageDto dto, int senderId, CancellationToken cancellationToken, IList<CreateAttachmentDto>? attachments = null)
        {
            var userDb = await _userService.GetByIdAsync(senderId);
            if (userDb == null || !userDb.IsActive)
                return false;

            var groupIds = dto.Recipients.Where(r => r.Type == "group").Select(r => r.Id).ToList();
            var groupUsers = new List<int>();
            if (groupIds.Any())
            {
                var groups = await _groupRepository.GetByIdsAsync(groupIds);
                groupUsers = groups.SelectMany(g => g.Users.Select(u => u.Id)).ToList();
            }

            var userIds = dto.Recipients.Where(r => r.Type == "user").Select(r => r.Id).ToList();
            var allRecipientUserIds = userIds.Concat(groupUsers).Distinct().ToList();

            var users = await _userService.GetByIdsAsync(allRecipientUserIds);

            try
            {
                foreach (var user in users)
                {
                    if (string.IsNullOrWhiteSpace(user.Email)) continue;

                    var subject = $"Nowa wiadomość od {userDb.FullName()}: {dto.Subject}";
                    var htmlBody = EmailTemplates.NewMessageNotification(
                        user.Name,
                        userDb.FullName(),
                        dto.Subject,
                        dto.Body);
                    await _sesEmailService.SendEmailAsync(
                        user.Name,
                        user.Email!,
                        subject,
                        htmlBody,
                        cancellationToken
                    );
                }
            }
            catch
            {
                return false;
            }

            var message = new Message
            {
                Subject = dto.Subject,
                Body = dto.Body,
                SenderId = senderId,
                SentDate = DateTime.UtcNow,
                Recipients = allRecipientUserIds.Select(id => new MessageRecipient { RecipientEntityId = id, RecipientType = RecipientType.User }).ToList(),
                Attachments = (attachments ?? []).Select(a => new MessageAttachment
                {
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    FileSize = a.FileSize,
                    Data = a.Data
                }).ToList()
            };
            await _messageRepository.AddAsync(message);
            await _messageRepository.SaveChangesAsync();

            if (dto.Id.HasValue)
            {
                await DeleteDraftAsync(dto.Id.Value, senderId);
            }

            return true;
        }

        public async Task<Message> SaveDraftAsync(SendMessageDto dto, int senderId, CancellationToken cancellationToken, IList<CreateAttachmentDto>? attachments = null)
        {
            var draft = new Message
            {
                SenderId = senderId,
                IsDraft = true,
                Subject = dto.Subject,
                Body = dto.Body,
                SentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            var recipients = dto.Recipients.Select(r => new MessageRecipient
            {
                RecipientEntityId = r.Id,
                RecipientType = r.Type == "group" ? RecipientType.Group : RecipientType.User
            }).ToList();

            draft.Recipients = recipients;
            draft.Attachments = (attachments ?? []).Select(a => new MessageAttachment
            {
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                Data = a.Data
            }).ToList();

            await _messageRepository.AddAsync(draft);
            await _messageRepository.SaveChangesAsync();
            return draft;
        }

        public async Task<List<Message>> GetDraftsForUserAsync(int userId)
        {
            return await _messageRepository.GetDraftsForUserWithRecipientsAsync(userId);
        }

        public async Task<bool> DeleteDraftAsync(int draftId, int userId)
        {
            var draft = await _messageRepository.GetByIdAsync(draftId);
            if (draft == null || !draft.IsDraft || draft.SenderId != userId)
                return false;
            _messageRepository.Remove(draft);
            await _messageRepository.SaveChangesAsync();
            return true;
        }

        public async Task<Message?> UpdateDraftAsync(int draftId, SendMessageDto dto, int senderId, CancellationToken cancellationToken, IList<CreateAttachmentDto>? attachments = null)
        {
            var draft = await _messageRepository.GetDraftWithRecipientsAsync(draftId);
            if (draft == null || !draft.IsDraft || draft.SenderId != senderId)
                return null;

            draft.Subject = dto.Subject;
            draft.Body = dto.Body;
            draft.SentDate = DateTime.UtcNow;
            if (draft.CreatedAt == default)
                draft.CreatedAt = DateTime.UtcNow;

            var recipients = dto.Recipients.Select(r => new MessageRecipient
            {
                RecipientEntityId = r.Id,
                RecipientType = r.Type == "group" ? RecipientType.Group : RecipientType.User
            }).ToList();

            draft.Recipients = recipients;

            draft.Attachments = (attachments ?? []).Select(a => new MessageAttachment
            {
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                Data = a.Data
            }).ToList();

            await _messageRepository.SaveChangesAsync();
            return draft;
        }

        public async Task<bool> MarkAsReadAsync(int messageId, int userId)
        {
            return await _messageRepository.MarkAsReadAsync(messageId, userId);
        }

        public async Task<bool> MarkAsUnreadAsync(int messageId, int userId)
        {
            return await _messageRepository.MarkAsUnreadAsync(messageId, userId);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _messageRepository.GetUnreadCountForUserAsync(userId);
        }

        public async Task<List<Message>> GetAllMessagesAsync()
        {
            return await _messageRepository.GetAllMessagesAsync();
        }

        public async Task<MessageAttachment?> GetAttachmentAsync(int messageId, int attachmentId, int userId)
        {
            var hasAccess = await _messageRepository.UserHasAccessToMessageAsync(messageId, userId);
            if (!hasAccess)
                return null;

            return await _messageRepository.GetAttachmentAsync(messageId, attachmentId);
        }
    }
}
