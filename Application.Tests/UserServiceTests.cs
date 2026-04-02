using System.Linq.Expressions;
using Application.Interfaces;
using Application.Services;
using Core.Entity;
using Moq;
using Xunit;

namespace Application.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRepository<UserCredential>> _credentialRepo = new();
    private readonly Mock<IRepository<UserRoleAssignment>> _roleRepo = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_userRepo.Object, _credentialRepo.Object, _roleRepo.Object);
    }

    [Fact]
    public async Task GetByIdAsync_UsesRepositoryWithRoles()
    {
        _userRepo.Setup(r => r.GetByIdWithRolesAsync(1)).ReturnsAsync(CreateUser(1));

        var result = await _sut.GetByIdAsync(1);

        Assert.NotNull(result);
        _userRepo.Verify(r => r.GetByIdWithRolesAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_UsesRepositoryWithRoles()
    {
        _userRepo.Setup(r => r.GetAllWithRolesAsync()).ReturnsAsync(new List<User> { CreateUser(1), CreateUser(2) });

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ChangePasswordAsync_ReturnsFalse_WhenOldPasswordInvalid()
    {
        var credential = new UserCredential
        {
            UserId = 1,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct")
        };
        _credentialRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<UserCredential, bool>>>()))
            .ReturnsAsync(credential);

        var result = await _sut.ChangePasswordAsync(1, "wrong", "new");

        Assert.False(result);
    }

    [Fact]
    public async Task ChangePasswordAsync_UpdatesPassword_WhenOldPasswordCorrect()
    {
        var credential = new UserCredential
        {
            UserId = 1,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct")
        };

        _credentialRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<UserCredential, bool>>>()))
            .ReturnsAsync(credential);

        var result = await _sut.ChangePasswordAsync(1, "correct", "new");

        Assert.True(result);
        Assert.True(BCrypt.Net.BCrypt.Verify("new", credential.PasswordHash));
        _credentialRepo.Verify(r => r.Update(credential), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsNull_WhenEmailAlreadyExists()
    {
        _userRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(CreateUser(10, email: "taken@mail.com"));

        var result = await _sut.CreateUserAsync(
            "Jan",
            "Nowak",
            "taken@mail.com",
            "pass",
            new List<UserRole> { UserRole.STUDENT },
            true);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateUserAsync_CreatesCredentialsAndRoles_WhenInputIsValid()
    {
        User? createdUser = null;

        _userRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u =>
            {
                u.Id = 123;
                createdUser = u;
            })
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateUserAsync(
            "Jan",
            "Nowak",
            "new@mail.com",
            "pass",
            new List<UserRole> { UserRole.STUDENT, UserRole.ADMIN },
            false);

        Assert.NotNull(result);
        Assert.Equal(123, result!.Id);
        Assert.False(result.IsActive);
        Assert.Equal(2, result.Roles.Count);
        _credentialRepo.Verify(r => r.AddAsync(It.Is<UserCredential>(c => c.UserId == 123)), Times.Once);
        _roleRepo.Verify(r => r.AddAsync(It.IsAny<UserRoleAssignment>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UpdateUserAsync_UpdatesFieldsAndRoles_WhenDataValid()
    {
        var user = CreateUser(5, email: "old@mail.com");
        _userRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);
        _userRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync((User?)null);
        _roleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRoleAssignment, bool>>>()))
            .ReturnsAsync(new List<UserRoleAssignment> { new() { UserId = 5, Role = UserRole.STUDENT } });

        var result = await _sut.UpdateUserAsync(
            5,
            "Adam",
            "Kowal",
            "new@mail.com",
            new List<UserRole> { UserRole.ADMIN },
            true);

        Assert.NotNull(result);
        Assert.Equal("Adam", result!.Name);
        Assert.Equal("Kowal", result.Surname);
        Assert.Equal("new@mail.com", result.Email);
        Assert.True(result.IsActive);
        Assert.Single(result.Roles);
        _roleRepo.Verify(r => r.Remove(It.IsAny<UserRoleAssignment>()), Times.Once);
        _roleRepo.Verify(r => r.AddAsync(It.Is<UserRoleAssignment>(x => x.Role == UserRole.ADMIN)), Times.Once);
        _userRepo.Verify(r => r.Update(user), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_RemovesCredentialAndUser_WhenUserExists()
    {
        var user = CreateUser(9);
        var credential = new UserCredential { UserId = 9, PasswordHash = "hash" };

        _userRepo.Setup(r => r.GetByIdAsync(9)).ReturnsAsync(user);
        _credentialRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<UserCredential, bool>>>()))
            .ReturnsAsync(credential);

        var result = await _sut.DeleteUserAsync(9);

        Assert.True(result);
        _credentialRepo.Verify(r => r.Remove(credential), Times.Once);
        _userRepo.Verify(r => r.Remove(user), Times.Once);
    }

    [Fact]
    public async Task ToggleUserStatusAsync_TogglesValue()
    {
        var user = CreateUser(2, isActive: false);
        _userRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(user);

        var result = await _sut.ToggleUserStatusAsync(2);

        Assert.True(result);
        Assert.True(user.IsActive);
        _userRepo.Verify(r => r.Update(user), Times.Once);
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
            CreatedAt = DateTime.UtcNow,
            Roles = new List<UserRoleAssignment>
            {
                new() { UserId = id, Role = UserRole.STUDENT }
            }
        };
    }
}
