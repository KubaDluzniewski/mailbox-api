using Application.Interfaces;
using Application.Services;
using Core.Entity;
using Moq;
using Xunit;

namespace Application.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
    {
        // Arrange
        var userId = 0;
        var user = new User { Id = userId, Email = "test@example.com", Name = "Test", Surname = "User", CreatedAt = DateTime.UtcNow, IsActive = true };
        var mockRepo = new Mock<IRepository<User>>();
        var mockCredentialRepo = new Mock<IRepository<UserCredential>>();
        mockRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var service = new UserService(mockRepo.Object, mockCredentialRepo.Object);

        // Act
        var result = await service.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = 0;
        var mockRepo = new Mock<IRepository<User>>();
        var mockCredentialRepo = new Mock<IRepository<UserCredential>>();
        mockRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var service = new UserService(mockRepo.Object, mockCredentialRepo.Object);

        // Act
        var result = await service.GetByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }
}
