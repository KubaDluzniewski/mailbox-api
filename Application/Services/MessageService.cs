using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Email;
using Application.Interfaces;
using Core.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserService _userService;
        private readonly IEmailService _sesEmailService;
        private readonly IGroupRepository _groupRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MessageService> _logger;

        public MessageService(
            IMessageRepository messageRepository,
            IUserService userService,
            IEmailService sesEmailService,
            IGroupRepository groupRepository,
            IConfiguration configuration,
            ILogger<MessageService> logger)
        {
            _messageRepository = messageRepository;
            _userService = userService;
            _sesEmailService = sesEmailService;
            _groupRepository = groupRepository;
            _configuration = configuration;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task SendMessageAsync(Message message)
        {
            if (message.SentDate == default)
                message.SentDate = DateTime.UtcNow;
            await _messageRepository.AddAsync(message);
            await _messageRepository.SaveChangesAsync();
        }


        /// <inheritdoc/>
        public async Task<List<MessageViewDto>> GetInboxViewAsync(int userId)
        {
            var list = await _messageRepository.GetMessagesForUserAsync(userId);

            return list.Select(m =>
            {
                var currentUserRecipient = m.Recipients.FirstOrDefault(r => r.RecipientEntityId == userId && r.RecipientType == RecipientType.User);

                return new MessageViewDto
                {
                    Id = m.Id,
                    Subject = m.Subject,
                    Body = m.Body,
                    Sender = MapSender(m.Sender),
                    SentDate = m.SentDate,
                    CreatedAt = m.SentDate,
                    Recipients = new List<RecipientDto>
                    {
                        new RecipientDto
                        {
                            Id = m.Sender?.Id ?? 0,
                            Type = "user",
                            DisplayName = m.Sender?.FullName() ?? "Unknown",
                            Subtitle = m.Sender?.Email ?? string.Empty
                        }
                    },
                    IsRead = currentUserRecipient?.IsRead ?? false,
                    ReadAt = currentUserRecipient?.ReadAt,
                    Attachments = MapAttachments(m.Attachments)
                };
            }).ToList();
        }

        /// <inheritdoc/>
        public async Task<List<MessageViewDto>> GetSentViewAsync(int userId)
        {
            var list = await _messageRepository.GetMessagesSentByUserAsync(userId);

            var userIds = list.SelectMany(m => m.Recipients.Select(r => r.RecipientEntityId)).Distinct().ToList();
            var users = await _userService.GetByIdsAsync(userIds);

            return list.Select(m => new MessageViewDto
            {
                Id = m.Id,
                Subject = m.Subject,
                Body = m.Body,
                Sender = MapSender(m.Sender),
                SentDate = m.SentDate,
                Recipients = MapUserRecipients(m.Recipients, users),
                Attachments = MapAttachments(m.Attachments)
            }).ToList();
        }

        /// <inheritdoc/>
        public async Task<List<MessageViewDto>> GetDraftsViewAsync(int userId)
        {
            var list = await _messageRepository.GetDraftsForUserWithRecipientsAsync(userId);

            var userIds = list.SelectMany(m => m.Recipients.Where(r => r.RecipientType == RecipientType.User).Select(r => r.RecipientEntityId)).Distinct().ToList();
            var groupIds = list.SelectMany(m => m.Recipients.Where(r => r.RecipientType == RecipientType.Group).Select(r => r.RecipientEntityId)).Distinct().ToList();

            var users = await _userService.GetByIdsAsync(userIds);
            var groups = await _groupRepository.GetByIdsAsync(groupIds);

            return list.Select(m => new MessageViewDto
            {
                Id = m.Id,
                Subject = m.Subject,
                Body = m.Body,
                CreatedAt = m.CreatedAt,
                Recipients = MapDraftRecipients(m.Recipients, users, groups),
                Attachments = MapAttachments(m.Attachments)
            }).ToList();
        }

        /// <inheritdoc/>
        public async Task<MessageViewDto?> GetDraftViewAsync(int draftId, int userId)
        {
            var draft = await _messageRepository.GetDraftWithRecipientsAsync(draftId);
            if (draft == null || !draft.IsDraft || draft.SenderId != userId)
                return null;

            var userIds = draft.Recipients
                .Where(r => r.RecipientType == RecipientType.User)
                .Select(r => r.RecipientEntityId)
                .Distinct()
                .ToList();
            var groupIds = draft.Recipients
                .Where(r => r.RecipientType == RecipientType.Group)
                .Select(r => r.RecipientEntityId)
                .Distinct()
                .ToList();

            var users = await _userService.GetByIdsAsync(userIds);
            var groups = await _groupRepository.GetByIdsAsync(groupIds);

            return new MessageViewDto
            {
                Id = draft.Id,
                Subject = draft.Subject,
                Body = draft.Body,
                CreatedAt = draft.CreatedAt,
                Recipients = MapDraftRecipients(draft.Recipients, users, groups),
                Attachments = MapAttachments(draft.Attachments)
            };
        }

        /// <inheritdoc/>
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

            IList<CreateAttachmentDto> finalAttachments = attachments ?? [];
            if (dto.Id.HasValue && finalAttachments.Count == 0)
            {
                var existingDraft = await _messageRepository.GetDraftWithRecipientsAsync(dto.Id.Value);
                if (existingDraft?.Attachments != null && existingDraft.Attachments.Count > 0)
                {
                    finalAttachments = existingDraft.Attachments.Select(a => new CreateAttachmentDto
                    {
                        FileName = a.FileName,
                        ContentType = a.ContentType,
                        FileSize = a.FileSize,
                        Data = a.Data
                    }).ToList();
                }
            }

            var message = new Message
            {
                Subject = dto.Subject,
                Body = dto.Body,
                SenderId = senderId,
                SentDate = DateTime.UtcNow,
                Recipients = allRecipientUserIds.Select(id => new MessageRecipient { RecipientEntityId = id, RecipientType = RecipientType.User }).ToList(),
                Attachments = finalAttachments.Select(a => new MessageAttachment
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

            var frontendUrl = _configuration["App:FrontendUrl"]?.TrimEnd('/');
            var loginLink = string.IsNullOrWhiteSpace(frontendUrl)
                ? "http://localhost:5173/login"
                : $"{frontendUrl}/login";

            foreach (var user in users)
            {
                if (string.IsNullOrWhiteSpace(user.Email)) continue;

                var subject = $"Nowa wiadomość od {userDb.FullName()}: {dto.Subject}";
                var htmlBody = EmailTemplates.NewMessageNotification(
                    user.Name,
                    userDb.FullName(),
                    dto.Subject,
                    loginLink);

                try
                {
                    await _sesEmailService.SendEmailAsync(
                        user.Name,
                        user.Email!,
                        subject,
                        htmlBody,
                        cancellationToken
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Wiadomosc {MessageSubject} zapisana, ale nie udalo sie wyslac powiadomienia email do {RecipientEmail}", dto.Subject, user.Email);
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> SendFromFormAsync(string? subject, string? body, string? recipients, int? id, IFormFileCollection? attachments, int senderId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Brak tematu.");

            var recipientList = ParseRecipients(recipients, requireRecipients: true);

            var dto = new SendMessageDto
            {
                Id = id,
                Subject = subject,
                Body = body ?? string.Empty,
                Recipients = recipientList
            };

            var attachmentDtos = await ReadAttachmentsAsync(attachments, cancellationToken);
            return await SendMessages(dto, senderId, cancellationToken, attachmentDtos);
        }

        /// <inheritdoc/>
        public async Task<MessageViewDto> SaveDraftFromFormAsync(string? subject, string? body, string? recipients, IFormFileCollection? attachments, int senderId, CancellationToken cancellationToken)
        {
            var recipientList = ParseRecipients(recipients, requireRecipients: false);

            var dto = new SendMessageDto
            {
                Subject = subject ?? string.Empty,
                Body = body ?? string.Empty,
                Recipients = recipientList
            };

            var attachmentDtos = await ReadAttachmentsAsync(attachments, cancellationToken);
            var draft = await SaveDraftAsync(dto, senderId, cancellationToken, attachmentDtos);

            return new MessageViewDto
            {
                Id = draft.Id,
                Subject = draft.Subject,
                Body = draft.Body,
                CreatedAt = draft.CreatedAt,
                Recipients = recipientList,
                Attachments = MapAttachments(draft.Attachments)
            };
        }

        /// <inheritdoc/>
        public async Task<MessageViewDto?> UpdateDraftFromFormAsync(int draftId, string? subject, string? body, string? recipients, IFormFileCollection? attachments, int senderId, CancellationToken cancellationToken)
        {
            var recipientList = ParseRecipients(recipients, requireRecipients: false);

            var dto = new SendMessageDto
            {
                Subject = subject ?? string.Empty,
                Body = body ?? string.Empty,
                Recipients = recipientList
            };

            var attachmentDtos = await ReadAttachmentsAsync(attachments, cancellationToken);
            var draft = await UpdateDraftAsync(draftId, dto, senderId, cancellationToken, attachmentDtos.Count > 0 ? attachmentDtos : null);
            if (draft == null)
                return null;

            return new MessageViewDto
            {
                Id = draft.Id,
                Subject = draft.Subject,
                Body = draft.Body,
                Recipients = recipientList,
                Attachments = MapAttachments(draft.Attachments)
            };
        }

        /// <inheritdoc/>
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


        /// <inheritdoc/>
        public async Task<bool> DeleteDraftAsync(int draftId, int userId)
        {
            var draft = await _messageRepository.GetByIdAsync(draftId);
            if (draft == null || !draft.IsDraft || draft.SenderId != userId)
                return false;
            _messageRepository.Remove(draft);
            await _messageRepository.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteSentMessageAsync(int messageId, int userId)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message == null || message.IsDraft || message.SenderId != userId)
                return false;
            _messageRepository.Remove(message);
            await _messageRepository.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
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

            if (attachments != null)
            {
                draft.Attachments = attachments.Select(a => new MessageAttachment
                {
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    FileSize = a.FileSize,
                    Data = a.Data
                }).ToList();
            }

            await _messageRepository.SaveChangesAsync();
            return draft;
        }

        /// <inheritdoc/>
        public async Task<bool> MarkAsReadAsync(int messageId, int userId)
        {
            return await _messageRepository.MarkAsReadAsync(messageId, userId);
        }

        /// <inheritdoc/>
        public async Task<bool> MarkAsUnreadAsync(int messageId, int userId)
        {
            return await _messageRepository.MarkAsUnreadAsync(messageId, userId);
        }

        /// <inheritdoc/>
        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _messageRepository.GetUnreadCountForUserAsync(userId);
        }

        /// <inheritdoc/>
        public async Task<List<Message>> GetAllMessagesAsync()
        {
            return await _messageRepository.GetAllMessagesAsync();
        }

        /// <inheritdoc/>
        public async Task<List<MessageViewDto>> GetAdminMessagesViewAsync()
        {
            var list = await _messageRepository.GetAllMessagesAsync();

            var userIds = list.SelectMany(m => m.Recipients.Select(r => r.RecipientEntityId)).Distinct().ToList();
            var users = await _userService.GetByIdsAsync(userIds);

            return list.Select(m => new MessageViewDto
            {
                Id = m.Id,
                Subject = m.Subject,
                Body = m.Body,
                Sender = MapSender(m.Sender),
                SentDate = m.SentDate,
                RecipientCount = m.Recipients.Count,
                Recipients = MapUserRecipients(m.Recipients, users),
                Attachments = MapAttachments(m.Attachments)
            }).ToList();
        }

        /// <summary>
        ///     Mapuje encję <see cref="User"/> nadawcy na <see cref="UserSummaryDto"/>.
        ///     Zwraca null jeśli nadawca jest null.
        /// </summary>
        private static UserSummaryDto? MapSender(User? sender)
        {
            if (sender == null) return null;

            return new UserSummaryDto
            {
                Id = sender.Id,
                Name = sender.Name,
                Surname = sender.Surname,
                Email = sender.Email ?? string.Empty
            };
        }

        /// <summary>Mapuje kolekcję <see cref="MessageAttachment"/> na listę <see cref="AttachmentDto"/>.</summary>
        private static List<AttachmentDto> MapAttachments(IEnumerable<MessageAttachment> attachments)
        {
            return attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize
            }).ToList();
        }

        /// <summary>Mapuje odbiorców wiadomości wysłanej (tylko użytkownicy) na listę <see cref="RecipientDto"/>.</summary>
        private static List<RecipientDto> MapUserRecipients(IEnumerable<MessageRecipient> recipients, List<User> users)
        {
            return recipients.Select(r =>
            {
                var user = users.FirstOrDefault(u => u.Id == r.RecipientEntityId);
                return new RecipientDto
                {
                    Id = r.RecipientEntityId,
                    Type = "user",
                    DisplayName = user?.FullName() ?? "Unknown",
                    Subtitle = user?.Email ?? string.Empty,
                    IsRead = r.IsRead,
                    ReadAt = r.ReadAt
                };
            }).ToList();
        }

        /// <summary>Mapuje odbiorców wersji roboczej (użytkownicy i grupy) na listę <see cref="RecipientDto"/>.</summary>
        private static List<RecipientDto> MapDraftRecipients(IEnumerable<MessageRecipient> recipients, List<User> users, List<Group> groups)
        {
            return recipients.Select(r =>
            {
                if (r.RecipientType == RecipientType.User)
                {
                    var user = users.FirstOrDefault(u => u.Id == r.RecipientEntityId);
                    return new RecipientDto
                    {
                        Id = r.RecipientEntityId,
                        Type = "user",
                        DisplayName = user?.FullName() ?? "Unknown",
                        Subtitle = user?.Email ?? string.Empty
                    };
                }

                var group = groups.FirstOrDefault(g => g.Id == r.RecipientEntityId);
                return new RecipientDto
                {
                    Id = r.RecipientEntityId,
                    Type = "group",
                    DisplayName = group?.Name ?? "Unknown",
                    Subtitle = "Group"
                };
            }).ToList();
        }

        /// <summary>
        ///     Deserializuje JSON odbiorców z pola formularza. Rzuca <see cref="ArgumentException"/>
        ///     przy błędnym formacie lub gdy <paramref name="requireRecipients"/> = true i lista jest pusta.
        /// </summary>
        private static List<RecipientDto> ParseRecipients(string? recipients, bool requireRecipients)
        {
            List<RecipientDto> recipientList;
            try
            {
                recipientList = string.IsNullOrWhiteSpace(recipients)
                    ? []
                    : JsonSerializer.Deserialize<List<RecipientDto>>(
                        recipients,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
            }
            catch
            {
                throw new ArgumentException("Nieprawidlowy format odbiorcow.");
            }

            if (requireRecipients && recipientList.Count == 0)
                throw new ArgumentException("Brak odbiorcow.");

            return recipientList;
        }

        /// <summary>
        ///     Odczytuje pliki z <see cref="IFormFileCollection"/> do listy <see cref="CreateAttachmentDto"/>.
        ///     Pomija pliki puste i przekraczające 10 MB.
        /// </summary>
        private static async Task<List<CreateAttachmentDto>> ReadAttachmentsAsync(IFormFileCollection? files, CancellationToken cancellationToken)
        {
            if (files == null || files.Count == 0)
                return [];

            const long maxFileSizeBytes = 10 * 1024 * 1024; // 10 MB per file

            var result = new List<CreateAttachmentDto>();
            foreach (var file in files)
            {
                if (file.Length == 0) continue;
                if (file.Length > maxFileSizeBytes) continue;

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms, cancellationToken);

                result.Add(new CreateAttachmentDto
                {
                    FileName = SanitizeFileName(file.FileName),
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    Data = ms.ToArray()
                });
            }

            return result;
        }

        /// <summary>Sanityzuje nazwę pliku usuwając niedozwolone znaki i pobierając tylko basename.</summary>
        private static string SanitizeFileName(string fileName)
        {
            var name = Path.GetFileName(fileName);
            foreach (var invalid in Path.GetInvalidFileNameChars())
                name = name.Replace(invalid, '_');
            return name;
        }

        /// <inheritdoc/>
        public async Task<MessageAttachment?> GetAttachmentAsync(int messageId, int attachmentId, int userId)
        {
            var hasAccess = await _messageRepository.UserHasAccessToMessageAsync(messageId, userId);
            if (!hasAccess)
                return null;

            return await _messageRepository.GetAttachmentAsync(messageId, attachmentId);
        }
    }
}
