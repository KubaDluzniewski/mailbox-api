using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Core.Entity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Application.Tests;

public class MessageServiceTests
{
    private readonly Mock<IMessageRepository> _messageRepo = new();
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IGroupRepository> _groupRepo = new();
    private readonly Mock<IConfiguration> _configuration = new();
    private readonly MessageService _sut;

    public MessageServiceTests()
    {
        _configuration.Setup(c => c["App:FrontendUrl"]).Returns("http://localhost:5173");
        _sut = new MessageService(_messageRepo.Object, _userService.Object, _emailService.Object, _groupRepo.Object, _configuration.Object);
    }

    [Fact]
    public async Task SendMessageAsync_SetsSentDate_WhenMissing()
    {
        var message = CreateMessage();
        message.SentDate = default;

        await _sut.SendMessageAsync(message);

        Assert.NotNull(message.SentDate);
        _messageRepo.Verify(r => r.AddAsync(message), Times.Once);
        _messageRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SendMessages_ReturnsFalse_WhenSenderIsMissingOrInactive()
    {
        var dto = CreateSendDto();
        _userService.Setup(s => s.GetByIdAsync(10)).ReturnsAsync((User?)null);

        var missingSender = await _sut.SendMessages(dto, 10, CancellationToken.None);

        Assert.False(missingSender);

        _userService.Setup(s => s.GetByIdAsync(10)).ReturnsAsync(CreateUser(10, isActive: false));

        var inactiveSender = await _sut.SendMessages(dto, 10, CancellationToken.None);

        Assert.False(inactiveSender);
    }

    [Fact]
    public async Task SendMessages_ReturnsTrue_AndCreatesMessage_WithDistinctRecipients()
    {
        var sender = CreateUser(1, email: "sender@mail.com");
        var directUser = CreateUser(2, email: "u2@mail.com");
        var fromGroup = CreateUser(3, email: "u3@mail.com");

        var dto = new SendMessageDto
        {
            Subject = "Temat",
            Body = "Tresc",
            Recipients = new List<RecipientDto>
            {
                new() { Id = 2, Type = "user", DisplayName = "User 2" },
                new() { Id = 99, Type = "group", DisplayName = "Group" }
            }
        };

        _userService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(sender);
        _groupRepo.Setup(r => r.GetByIdsAsync(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 99 }))))
            .ReturnsAsync(new List<Group>
            {
                new() { Id = 99, Name = "Group", Users = new List<User> { directUser, fromGroup } }
            });

        _userService.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<User> { directUser, fromGroup });

        Message? savedMessage = null;
        _messageRepo.Setup(r => r.AddAsync(It.IsAny<Message>()))
            .Callback<Message>(m => savedMessage = m)
            .Returns(Task.CompletedTask);

        var result = await _sut.SendMessages(dto, 1, CancellationToken.None);

        Assert.True(result);
        Assert.NotNull(savedMessage);
        Assert.Equal(2, savedMessage!.Recipients.Count);
        _emailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _messageRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SendMessages_UsesLoginLinkInNotificationEmail()
    {
        var sender = CreateUser(1, email: "sender@mail.com");
        var directUser = CreateUser(2, email: "u2@mail.com");

        var dto = new SendMessageDto
        {
            Subject = "Temat",
            Body = "<b>Tresc</b>",
            Recipients = new List<RecipientDto> { new() { Id = 2, Type = "user", DisplayName = "User 2" } }
        };

        _userService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(sender);
        _userService.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<User> { directUser });
        _messageRepo.Setup(r => r.AddAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);

        await _sut.SendMessages(dto, 1, CancellationToken.None);

        _emailService.Verify(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.Is<string>(html => html.Contains("http://localhost:5173/login")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMessages_ReusesDraftAttachments_WhenNoNewFilesProvided()
    {
        var sender = CreateUser(1, email: "sender@mail.com");
        var directUser = CreateUser(2, email: "u2@mail.com");

        var dto = new SendMessageDto
        {
            Id = 77,
            Subject = "Temat",
            Body = "Tresc",
            Recipients = new List<RecipientDto> { new() { Id = 2, Type = "user", DisplayName = "User 2" } }
        };

        _userService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(sender);
        _userService.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<User> { directUser });
        _messageRepo.Setup(r => r.GetDraftWithRecipientsAsync(77)).ReturnsAsync(new Message
        {
            Id = 77,
            IsDraft = true,
            SenderId = 1,
            Subject = "Draft",
            Body = "Draft body",
            Attachments = new List<MessageAttachment>
            {
                new() { FileName = "x.txt", ContentType = "text/plain", FileSize = 1, Data = new byte[] { 1 } }
            }
        });

        Message? savedMessage = null;
        _messageRepo.Setup(r => r.AddAsync(It.IsAny<Message>()))
            .Callback<Message>(m => savedMessage = m)
            .Returns(Task.CompletedTask);

        var result = await _sut.SendMessages(dto, 1, CancellationToken.None, null);

        Assert.True(result);
        Assert.NotNull(savedMessage);
        Assert.Single(savedMessage!.Attachments);
        Assert.Equal("x.txt", savedMessage.Attachments.First().FileName);
    }

    [Fact]
    public async Task SaveDraftAsync_PersistsDraft_WithRecipients()
    {
        var dto = CreateSendDto();

        var result = await _sut.SaveDraftAsync(dto, 7, CancellationToken.None);

        Assert.True(result.IsDraft);
        Assert.Equal(7, result.SenderId);
        Assert.Single(result.Recipients);
        _messageRepo.Verify(r => r.AddAsync(It.IsAny<Message>()), Times.Once);
        _messageRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateDraftAsync_ReturnsNull_WhenDraftNotOwnedOrMissing()
    {
        _messageRepo.Setup(r => r.GetDraftWithRecipientsAsync(1)).ReturnsAsync((Message?)null);

        var missing = await _sut.UpdateDraftAsync(1, CreateSendDto(), 5, CancellationToken.None);

        Assert.Null(missing);

        _messageRepo.Setup(r => r.GetDraftWithRecipientsAsync(1)).ReturnsAsync(CreateMessage(id: 1, isDraft: true, senderId: 6));

        var notOwner = await _sut.UpdateDraftAsync(1, CreateSendDto(), 5, CancellationToken.None);

        Assert.Null(notOwner);
    }

    [Fact]
    public async Task DeleteDraftAsync_ReturnsTrue_OnlyForOwnerDraft()
    {
        var draft = CreateMessage(id: 5, isDraft: true, senderId: 2);
        _messageRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(draft);

        var result = await _sut.DeleteDraftAsync(5, 2);

        Assert.True(result);
        _messageRepo.Verify(r => r.Remove(draft), Times.Once);
        _messageRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ReadAndCountMethods_AreDelegatedToRepository()
    {
        _messageRepo.Setup(r => r.MarkAsReadAsync(1, 2)).ReturnsAsync(true);
        _messageRepo.Setup(r => r.MarkAsUnreadAsync(1, 2)).ReturnsAsync(true);
        _messageRepo.Setup(r => r.GetUnreadCountForUserAsync(2)).ReturnsAsync(4);
        _messageRepo.Setup(r => r.GetAllMessagesAsync()).ReturnsAsync(new List<Message> { CreateMessage(1) });

        var markedRead = await _sut.MarkAsReadAsync(1, 2);
        var markedUnread = await _sut.MarkAsUnreadAsync(1, 2);
        var count = await _sut.GetUnreadCountAsync(2);
        var all = await _sut.GetAllMessagesAsync();

        Assert.True(markedRead);
        Assert.True(markedUnread);
        Assert.Equal(4, count);
        Assert.Single(all);
    }

    private static SendMessageDto CreateSendDto()
    {
        return new SendMessageDto
        {
            Subject = "Temat",
            Body = "Tresc",
            Recipients = new List<RecipientDto>
            {
                new() { Id = 2, Type = "user", DisplayName = "User 2" }
            }
        };
    }

    private static Message CreateMessage(int id = 1, bool isDraft = false, int senderId = 1)
    {
        return new Message
        {
            Id = id,
            Subject = "Temat",
            Body = "Body",
            SenderId = senderId,
            IsDraft = isDraft,
            SentDate = DateTime.UtcNow,
            Recipients = new List<MessageRecipient>()
        };
    }

    private static User CreateUser(int id, string email = "user@mail.com", bool isActive = true)
    {
        return new User
        {
            Id = id,
            Name = "Jan",
            Surname = "Nowak",
            Email = email,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
    }
}
