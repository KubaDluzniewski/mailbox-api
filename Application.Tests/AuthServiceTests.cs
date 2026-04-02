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
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IRepository<UserCredential>> _credentialRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IUserActivationTokenRepository> _tokenRepo = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-secret-key-that-is-long-enough-for-hmacsha256",
                ["Jwt:Issuer"] = "issuer",
                ["Jwt:Audience"] = "audience",
                ["Registration:Secret"] = "secret-code",
                ["App:FrontendUrl"] = "http://localhost:5173"
            })
            .Build();

        _sut = new AuthService(
            _userService.Object,
            _mapper.Object,
            config,
            _credentialRepo.Object,
            _userRepo.Object,
            _emailService.Object,
            _tokenRepo.Object);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenUserMissing()
    {
        _userService.Setup(s => s.GetByEmailWithRolesAsync("missing@mail.com"))
            .ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync("missing@mail.com", "pass");

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenUserIsInactive()
    {
        var user = CreateUser(isActive: false);
        var credential = new UserCredential
        {
            UserId = user.Id,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass")
        };

        _userService.Setup(s => s.GetByEmailWithRolesAsync(user.Email)).ReturnsAsync(user);
        _userService.Setup(s => s.GetCredentialByUserIdAsync(user.Id)).ReturnsAsync(credential);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.LoginAsync(user.Email, "pass"));
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsValidAndUserActive()
    {
        var user = CreateUser(isActive: true);
        var credential = new UserCredential
        {
            UserId = user.Id,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass")
        };

        _userService.Setup(s => s.GetByEmailWithRolesAsync(user.Email)).ReturnsAsync(user);
        _userService.Setup(s => s.GetCredentialByUserIdAsync(user.Id)).ReturnsAsync(credential);

        var result = await _sut.LoginAsync(user.Email, "pass");

        Assert.NotNull(result);
        Assert.Contains('.', result);
    }

    [Fact]
    public async Task ActivateAsync_ReturnsFalse_WhenUserNotFound()
    {
        _userService.Setup(s => s.GetByEmailAsync("missing@mail.com")).ReturnsAsync((User?)null);

        var result = await _sut.ActivateAsync("missing@mail.com");

        Assert.False(result);
    }

    [Fact]
    public async Task ActivateAsync_GeneratesToken_AndSendsMail_WhenUserInactive()
    {
        var user = CreateUser(isActive: false);
        _userService.Setup(s => s.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        _tokenRepo.Setup(r => r.GetByUserIdAsync(user.Id, "activation")).ReturnsAsync(new List<UserActivationToken>());

        var result = await _sut.ActivateAsync(user.Email);

        Assert.True(result);
        _tokenRepo.Verify(r => r.AddAsync(It.Is<UserActivationToken>(t => t.UserId == user.Id && t.Type == "activation")), Times.Once);
        _emailService.Verify(e => e.SendEmailAsync(
            user.Name,
            user.Email,
            It.IsAny<string>(),
            It.Is<string>(body => body.Contains("confirm?token=")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmAsync_ActivatesUser_WhenActivationTokenValid()
    {
        var user = CreateUser(isActive: false);
        var token = new UserActivationToken
        {
            UserId = user.Id,
            Token = "token",
            Type = "activation",
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        _tokenRepo.Setup(r => r.GetByTokenAsync("token")).ReturnsAsync(token);
        _userService.Setup(s => s.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await _sut.ConfirmAsync(user.Email, "token");

        Assert.Equal("activation", result);
        Assert.True(user.IsActive);
        _userRepo.Verify(r => r.Update(user), Times.Once);
        _tokenRepo.Verify(r => r.Remove(token), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ReturnsFalse_WhenTokenExpired()
    {
        var token = new UserActivationToken
        {
            UserId = 7,
            Token = "expired",
            Type = "password_reset",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };

        _tokenRepo.Setup(r => r.GetByTokenAsync("expired")).ReturnsAsync(token);

        var result = await _sut.ResetPasswordAsync("user@mail.com", "expired", "newpass");

        Assert.False(result);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsTrue_WhenRegistrationCodeValidAndUserCreated()
    {
        _userService.Setup(s => s.CreateUserAsync(
                "Jan",
                "Nowak",
                "jan@mail.com",
                "pass",
                It.IsAny<List<UserRole>>(),
                false))
            .ReturnsAsync(CreateUser(email: "jan@mail.com", isActive: false));

        _userService.Setup(s => s.GetByEmailAsync("jan@mail.com"))
            .ReturnsAsync(CreateUser(email: "jan@mail.com", isActive: false));

        _tokenRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>(), "activation")).ReturnsAsync(new List<UserActivationToken>());

        var result = await _sut.RegisterAsync("Jan", "Nowak", "jan@mail.com", "pass", "secret-code");

        Assert.True(result);
        _userService.Verify(s => s.CreateUserAsync(
            "Jan",
            "Nowak",
            "jan@mail.com",
            "pass",
            It.Is<List<UserRole>>(roles => roles.Contains(UserRole.STUDENT)),
            false), Times.Once);
    }

    private static User CreateUser(int id = 1, string email = "user@mail.com", bool isActive = true)
    {
        return new User
        {
            Id = id,
            Name = "Jan",
            Surname = "Nowak",
            Email = email,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            Roles = new List<UserRoleAssignment>
            {
                new() { UserId = id, Role = UserRole.STUDENT }
            }
        };
    }
}
