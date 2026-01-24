using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Core.Entity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IRepository<UserCredential>> _mockCredentialRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<ISesEmailService> _mockEmailService;
    private readonly Mock<IUserActivationTokenRepository> _mockTokenRepo;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockMapper = new Mock<IMapper>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockCredentialRepo = new Mock<IRepository<UserCredential>>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockEmailService = new Mock<ISesEmailService>();
        _mockTokenRepo = new Mock<IUserActivationTokenRepository>();

        // Setup JWT configuration
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("test-secret-key-that-is-long-enough-for-hmacsha256");
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

        _authService = new AuthService(
            _mockUserService.Object,
            _mockMapper.Object,
            _mockConfiguration.Object,
            _mockCredentialRepo.Object,
            _mockUserRepo.Object,
            _mockEmailService.Object,
            _mockTokenRepo.Object
        );
    }

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenUserNotFound()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync("nonexistent@test.com", "password");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenCredentialsNotFound()
    {
        // Arrange
        var user = CreateTestUser();
        _mockUserService.Setup(s => s.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);
        _mockUserService.Setup(s => s.GetCredentialByUserIdAsync(user.Id))
            .ReturnsAsync((UserCredential?)null);

        // Act
        var result = await _authService.LoginAsync(user.Email, "password");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenPasswordIsInvalid()
    {
        // Arrange
        var user = CreateTestUser();
        var credential = new UserCredential
        {
            UserId = user.Id,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct_password")
        };

        _mockUserService.Setup(s => s.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);
        _mockUserService.Setup(s => s.GetCredentialByUserIdAsync(user.Id))
            .ReturnsAsync(credential);

        // Act
        var result = await _authService.LoginAsync(user.Email, "wrong_password");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsAreValid()
    {
        // Arrange
        var user = CreateTestUser();
        var password = "correct_password";
        var credential = new UserCredential
        {
            UserId = user.Id,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _mockUserService.Setup(s => s.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);
        _mockUserService.Setup(s => s.GetCredentialByUserIdAsync(user.Id))
            .ReturnsAsync(credential);

        // Act
        var result = await _authService.LoginAsync(user.Email, password);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(".", result); // JWT tokens contain dots
    }

    #endregion

    #region IsActiveAsync Tests

    [Fact]
    public async Task IsActiveAsync_ReturnsFalse_WhenUserNotFound()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.IsActiveAsync("nonexistent@test.com");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsActiveAsync_ReturnsFalse_WhenUserIsInactive()
    {
        // Arrange
        var user = CreateTestUser(isActive: false);
        _mockUserService.Setup(s => s.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.IsActiveAsync(user.Email);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsActiveAsync_ReturnsTrue_WhenUserIsActive()
    {
        // Arrange
        var user = CreateTestUser(isActive: true);
        _mockUserService.Setup(s => s.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.IsActiveAsync(user.Email);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region ActivateAsync Tests

    [Fact]
    public async Task ActivateAsync_ReturnsFalse_WhenUserNotFound()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.ActivateAsync("nonexistent@test.com");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ActivateAsync_ReturnsFalse_WhenUserAlreadyActive()
    {
        // Arrange
        var user = CreateTestUser(isActive: true);
        _mockUserService.Setup(s => s.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.ActivateAsync(user.Email);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ActivateAsync_ReturnsTrue_AndSendsEmail_WhenUserIsInactive()
    {
        // Arrange
        var user = CreateTestUser(isActive: false);
        _mockUserService.Setup(s => s.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);
        _mockTokenRepo.Setup(r => r.GetByUserIdAsync(user.Id, "activation"))
            .ReturnsAsync(new List<UserActivationToken>());
        _mockTokenRepo.Setup(r => r.AddAsync(It.IsAny<UserActivationToken>()))
            .Returns(Task.CompletedTask);
        _mockTokenRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _mockEmailService.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.ActivateAsync(user.Email);

        // Assert
        Assert.True(result);
        _mockEmailService.Verify(e => e.SendEmailAsync(
            It.IsAny<string>(),
            user.Email,
            "Potwierdzenie konta",
            It.Is<string>(body => body.Contains("Aktywuj konto")),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    #endregion

    #region ForgotPasswordAsync Tests

    [Fact]
    public async Task ForgotPasswordAsync_ReturnsNull_WhenUserNotFound()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.ForgotPasswordAsync("nonexistent@test.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ReturnsResetLink_WhenUserExists()
    {
        // Arrange
        var user = CreateTestUser();
        _mockUserService.Setup(s => s.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);
        _mockTokenRepo.Setup(r => r.GetByUserIdAsync(user.Id, "password_reset"))
            .ReturnsAsync(new List<UserActivationToken>());
        _mockTokenRepo.Setup(r => r.AddAsync(It.IsAny<UserActivationToken>()))
            .Returns(Task.CompletedTask);
        _mockTokenRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _mockEmailService.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.ForgotPasswordAsync(user.Email);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("reset-password", result);
        Assert.Contains("token=", result);
    }

    #endregion

    #region ResetPasswordAsync Tests

    [Fact]
    public async Task ResetPasswordAsync_ReturnsFalse_WhenTokenNotFound()
    {
        // Arrange
        _mockTokenRepo.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((UserActivationToken?)null);

        // Act
        var result = await _authService.ResetPasswordAsync("test@test.com", "invalid-token", "newpassword");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ResetPasswordAsync_ReturnsFalse_WhenTokenExpired()
    {
        // Arrange
        var token = new UserActivationToken
        {
            Token = "valid-token",
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // Expired
            Type = "password_reset",
            UserId = 1
        };
        _mockTokenRepo.Setup(r => r.GetByTokenAsync("valid-token"))
            .ReturnsAsync(token);

        // Act
        var result = await _authService.ResetPasswordAsync("test@test.com", "valid-token", "newpassword");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ResetPasswordAsync_ReturnsFalse_WhenTokenTypeIsWrong()
    {
        // Arrange
        var token = new UserActivationToken
        {
            Token = "valid-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Type = "activation", // Wrong type
            UserId = 1
        };
        _mockTokenRepo.Setup(r => r.GetByTokenAsync("valid-token"))
            .ReturnsAsync(token);

        // Act
        var result = await _authService.ResetPasswordAsync("test@test.com", "valid-token", "newpassword");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ResetPasswordAsync_ReturnsTrue_WhenValid()
    {
        // Arrange
        var user = CreateTestUser();
        var token = new UserActivationToken
        {
            Token = "valid-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Type = "password_reset",
            UserId = user.Id
        };

        _mockTokenRepo.Setup(r => r.GetByTokenAsync("valid-token"))
            .ReturnsAsync(token);
        _mockUserService.Setup(s => s.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        _mockUserService.Setup(s => s.SetPasswordAsync(user.Id, "newpassword"))
            .ReturnsAsync(true);
        _mockTokenRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.ResetPasswordAsync(user.Email, "valid-token", "newpassword");

        // Assert
        Assert.True(result);
        _mockTokenRepo.Verify(r => r.Remove(token), Times.Once);
    }

    #endregion

    #region Helper Methods

    private User CreateTestUser(int id = 1, bool isActive = true)
    {
        return new User
        {
            Id = id,
            Email = "test@example.com",
            Name = "Jan",
            Surname = "Kowalski",
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            Role = UserRole.STUDENT
        };
    }

    #endregion
}
