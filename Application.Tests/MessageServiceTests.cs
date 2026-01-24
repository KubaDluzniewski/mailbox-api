using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Core.Entity;
using Moq;
using Xunit;

namespace Application.Tests;

public class MessageServiceTests
{
    private readonly Mock<IMessageRepository> _mockMessageRepo;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ISesEmailService> _mockEmailService;
    private readonly Mock<IGroupRepository> _mockGroupRepo;
    private readonly MessageService _messageService;

    public MessageServiceTests()
    {
        _mockMessageRepo = new Mock<IMessageRepository>();
        _mockUserService = new Mock<IUserService>();
        _mockEmailService = new Mock<ISesEmailService>();
        _mockGroupRepo = new Mock<IGroupRepository>();

        _messageService = new MessageService(
            _mockMessageRepo.Object,
            _mockUserService.Object,
            _mockEmailService.Object,
            _mockGroupRepo.Object
        );
    }

    #region SendMessageAsync Tests

    [Fact]
    public async Task SendMessageAsync_SetsDefaultSentDate_WhenNotProvided()
    {
        // Arrange
        var message = new Message
        {
            Subject = "Test Subject",
            Body = "Test Body",
            SenderId = 1,
            SentDate = default
        };

        _mockMessageRepo.Setup(r => r.AddAsync(It.IsAny<Message>()))
            .Returns(Task.CompletedTask);
        _mockMessageRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _messageService.SendMessageAsync(message);

        // Assert
        Assert.NotEqual(default, message.SentDate);
        _mockMessageRepo.Verify(r => r.AddAsync(message), Times.Once);
        _mockMessageRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_PreservesSentDate_WhenProvided()
    {
        // Arrange
        var expectedDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var message = new Message
        {
            Subject = "Test Subject",
            Body = "Test Body",
            SenderId = 1,
            SentDate = expectedDate
        };

        _mockMessageRepo.Setup(r => r.AddAsync(It.IsAny<Message>()))
            .Returns(Task.CompletedTask);
        _mockMessageRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _messageService.SendMessageAsync(message);

        // Assert
        Assert.Equal(expectedDate, message.SentDate);
    }

    #endregion

    #region GetMessagesForUserAsync Tests

    [Fact]
    public async Task GetMessagesForUserAsync_ReturnsMessages_FromRepository()
    {
        // Arrange
        var userId = 1;
        var expectedMessages = new List<Message>
        {
            CreateTestMessage(1, "Subject 1"),
            CreateTestMessage(2, "Subject 2"),
            CreateTestMessage(3, "Subject 3")
        };

        _mockMessageRepo.Setup(r => r.GetMessagesForUserAsync(userId))
            .ReturnsAsync(expectedMessages);

        // Act
        var result = await _messageService.GetMessagesForUserAsync(userId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(expectedMessages, result);
    }

    [Fact]
    public async Task GetMessagesForUserAsync_ReturnsEmptyList_WhenNoMessages()
    {
        // Arrange
        var userId = 1;
        _mockMessageRepo.Setup(r => r.GetMessagesForUserAsync(userId))
            .ReturnsAsync(new List<Message>());

        // Act
        var result = await _messageService.GetMessagesForUserAsync(userId);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetMessagesSentByUserAsync Tests

    [Fact]
    public async Task GetMessagesSentByUserAsync_ReturnsSentMessages()
    {
        // Arrange
        var userId = 1;
        var expectedMessages = new List<Message>
        {
            CreateTestMessage(1, "Sent 1", senderId: userId),
            CreateTestMessage(2, "Sent 2", senderId: userId)
        };

        _mockMessageRepo.Setup(r => r.GetMessagesSentByUserAsync(userId))
            .ReturnsAsync(expectedMessages);

        // Act
        var result = await _messageService.GetMessagesSentByUserAsync(userId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.Equal(userId, m.SenderId));
    }

    #endregion

    #region SendMessages (with DTO) Tests

    [Fact]
    public async Task SendMessages_ReturnsFalse_WhenSenderNotFound()
    {
        // Arrange
        var dto = CreateSendMessageDto();
        _mockUserService.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _messageService.SendMessages(dto, 1, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendMessages_ReturnsFalse_WhenSenderIsInactive()
    {
        // Arrange
        var dto = CreateSendMessageDto();
        var inactiveUser = CreateTestUser(isActive: false);
        _mockUserService.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(inactiveUser);

        // Act
        var result = await _messageService.SendMessages(dto, 1, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendMessages_ReturnsTrue_AndSavesMessage_WhenValid()
    {
        // Arrange
        var senderId = 1;
        var recipientId = 2;
        var dto = new SendMessageDto
        {
            Subject = "Test Subject",
            Body = "Test Body",
            Recipients = new List<RecipientDto>
            {
                new RecipientDto { Id = recipientId, Type = "user" }
            }
        };

        var sender = CreateTestUser(id: senderId, isActive: true);
        var recipient = CreateTestUser(id: recipientId, email: "recipient@test.com");

        _mockUserService.Setup(s => s.GetByIdAsync(senderId))
            .ReturnsAsync(sender);
        _mockUserService.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<User> { recipient });
        _mockEmailService.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockMessageRepo.Setup(r => r.AddAsync(It.IsAny<Message>()))
            .Returns(Task.CompletedTask);
        _mockMessageRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _messageService.SendMessages(dto, senderId, CancellationToken.None);

        // Assert
        Assert.True(result);
        _mockMessageRepo.Verify(r => r.AddAsync(It.Is<Message>(m =>
            m.Subject == dto.Subject &&
            m.Body == dto.Body &&
            m.SenderId == senderId
        )), Times.Once);
    }

    #endregion

    #region SaveDraftAsync Tests

    [Fact]
    public async Task SaveDraftAsync_CreatesDraft_WithCorrectProperties()
    {
        // Arrange
        var senderId = 1;
        var dto = CreateSendMessageDto();

        Message? savedDraft = null;
        _mockMessageRepo.Setup(r => r.AddAsync(It.IsAny<Message>()))
            .Callback<Message>(m => savedDraft = m)
            .Returns(Task.CompletedTask);
        _mockMessageRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _messageService.SaveDraftAsync(dto, senderId, CancellationToken.None);

        // Assert
        Assert.True(result.IsDraft);
        Assert.Equal(dto.Subject, result.Subject);
        Assert.Equal(dto.Body, result.Body);
        Assert.Equal(senderId, result.SenderId);
        Assert.NotEqual(default, result.CreatedAt);
    }

    #endregion

    #region DeleteDraftAsync Tests

    [Fact]
    public async Task DeleteDraftAsync_ReturnsFalse_WhenDraftNotFound()
    {
        // Arrange
        _mockMessageRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Message?)null);

        // Act
        var result = await _messageService.DeleteDraftAsync(1, 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteDraftAsync_ReturnsFalse_WhenNotADraft()
    {
        // Arrange
        var message = CreateTestMessage(1, "Subject", isDraft: false);
        _mockMessageRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(message);

        // Act
        var result = await _messageService.DeleteDraftAsync(1, message.SenderId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteDraftAsync_ReturnsFalse_WhenNotOwner()
    {
        // Arrange
        var draft = CreateTestMessage(1, "Subject", isDraft: true, senderId: 1);
        _mockMessageRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(draft);

        // Act
        var result = await _messageService.DeleteDraftAsync(1, 999); // Different user

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteDraftAsync_ReturnsTrue_WhenValidDraft()
    {
        // Arrange
        var userId = 1;
        var draft = CreateTestMessage(1, "Subject", isDraft: true, senderId: userId);
        _mockMessageRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(draft);
        _mockMessageRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _messageService.DeleteDraftAsync(1, userId);

        // Assert
        Assert.True(result);
        _mockMessageRepo.Verify(r => r.Remove(draft), Times.Once);
    }

    #endregion

    #region MarkAsReadAsync / MarkAsUnreadAsync Tests

    [Fact]
    public async Task MarkAsReadAsync_CallsRepository()
    {
        // Arrange
        _mockMessageRepo.Setup(r => r.MarkAsReadAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _messageService.MarkAsReadAsync(1, 1);

        // Assert
        Assert.True(result);
        _mockMessageRepo.Verify(r => r.MarkAsReadAsync(1, 1), Times.Once);
    }

    [Fact]
    public async Task MarkAsUnreadAsync_CallsRepository()
    {
        // Arrange
        _mockMessageRepo.Setup(r => r.MarkAsUnreadAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _messageService.MarkAsUnreadAsync(1, 1);

        // Assert
        Assert.True(result);
        _mockMessageRepo.Verify(r => r.MarkAsUnreadAsync(1, 1), Times.Once);
    }

    #endregion

    #region GetUnreadCountAsync Tests

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCount_FromRepository()
    {
        // Arrange
        var userId = 1;
        var expectedCount = 5;
        _mockMessageRepo.Setup(r => r.GetUnreadCountForUserAsync(userId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _messageService.GetUnreadCountAsync(userId);

        // Assert
        Assert.Equal(expectedCount, result);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsZero_WhenNoUnread()
    {
        // Arrange
        _mockMessageRepo.Setup(r => r.GetUnreadCountForUserAsync(It.IsAny<int>()))
            .ReturnsAsync(0);

        // Act
        var result = await _messageService.GetUnreadCountAsync(1);

        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region GetAllMessagesAsync Tests

    [Fact]
    public async Task GetAllMessagesAsync_ReturnsAllMessages()
    {
        // Arrange
        var allMessages = new List<Message>
        {
            CreateTestMessage(1, "Subject 1"),
            CreateTestMessage(2, "Subject 2"),
            CreateTestMessage(3, "Subject 3"),
            CreateTestMessage(4, "Subject 4")
        };

        _mockMessageRepo.Setup(r => r.GetAllMessagesAsync())
            .ReturnsAsync(allMessages);

        // Act
        var result = await _messageService.GetAllMessagesAsync();

        // Assert
        Assert.Equal(4, result.Count);
    }

    #endregion

    #region Helper Methods

    private Message CreateTestMessage(int id, string subject, int senderId = 1, bool isDraft = false)
    {
        return new Message
        {
            Id = id,
            Subject = subject,
            Body = $"Body for {subject}",
            SenderId = senderId,
            IsDraft = isDraft,
            SentDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    private User CreateTestUser(int id = 1, bool isActive = true, string email = "test@example.com")
    {
        return new User
        {
            Id = id,
            Email = email,
            Name = "Jan",
            Surname = "Kowalski",
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            Role = UserRole.STUDENT
        };
    }

    private SendMessageDto CreateSendMessageDto()
    {
        return new SendMessageDto
        {
            Subject = "Test Subject",
            Body = "Test Body",
            Recipients = new List<RecipientDto>
            {
                new RecipientDto { Id = 2, Type = "user" }
            }
        };
    }

    #endregion
}
