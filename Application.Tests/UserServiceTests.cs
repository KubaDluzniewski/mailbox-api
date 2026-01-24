using Application.Interfaces;
using Application.Services;
using Core.Entity;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace Application.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IRepository<UserCredential>> _mockCredentialRepo;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockCredentialRepo = new Mock<IRepository<UserCredential>>();
        _userService = new UserService(_mockUserRepo.Object, _mockCredentialRepo.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
    {
        // Arrange
        var userId = 1;
        var user = CreateTestUser(userId);
        _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            CreateTestUser(1, "Jan", "Kowalski"),
            CreateTestUser(2, "Anna", "Nowak"),
            CreateTestUser(3, "Piotr", "Wiśniewski")
        };
        _mockUserRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        // Act
        var result = await _userService.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoUsers()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());

        // Act
        var result = await _userService.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetByEmailAsync Tests

    [Fact]
    public async Task GetByEmailAsync_ReturnsUser_WhenEmailExists()
    {
        // Arrange
        var email = "jan@test.com";
        var user = CreateTestUser(1, email: email);
        _mockUserRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetByEmailAsync(email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result!.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsNull_WhenEmailNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetByEmailAsync("nonexistent@test.com");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetByIdsAsync Tests

    [Fact]
    public async Task GetByIdsAsync_ReturnsMatchingUsers()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };
        var users = new List<User>
        {
            CreateTestUser(1),
            CreateTestUser(2),
            CreateTestUser(3)
        };
        _mockUserRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetByIdsAsync(ids);

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetByIdsAsync_RemovesDuplicateIds()
    {
        // Arrange
        var ids = new List<int> { 1, 1, 2, 2, 3 }; // Duplicates
        var users = new List<User> { CreateTestUser(1), CreateTestUser(2), CreateTestUser(3) };
        _mockUserRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetByIdsAsync(ids);

        // Assert - Should still work correctly
        Assert.Equal(3, result.Count);
    }

    #endregion

    #region ChangePasswordAsync Tests

    [Fact]
    public async Task ChangePasswordAsync_ReturnsFalse_WhenCredentialNotFound()
    {
        // Arrange
        _mockCredentialRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<UserCredential, bool>>>()))
            .ReturnsAsync((UserCredential?)null);

        // Act
        var result = await _userService.ChangePasswordAsync(1, "old", "new");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ChangePasswordAsync_ReturnsFalse_WhenOldPasswordIsWrong()
    {
        // Arrange
        var credential = new UserCredential
        {
            UserId = 1,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct_password")
        };
        _mockCredentialRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<UserCredential, bool>>>()))
            .ReturnsAsync(credential);

        // Act
        var result = await _userService.ChangePasswordAsync(1, "wrong_password", "new_password");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ChangePasswordAsync_ReturnsTrue_WhenPasswordIsCorrect()
    {
        // Arrange
        var oldPassword = "old_password";
        var credential = new UserCredential
        {
            UserId = 1,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(oldPassword)
        };
        UserCredential? updatedCredential = null;

        _mockCredentialRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<UserCredential, bool>>>()))
            .ReturnsAsync(credential);
        _mockCredentialRepo.Setup(r => r.Update(It.IsAny<UserCredential>()))
            .Callback<UserCredential>(c => updatedCredential = c);
        _mockCredentialRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.ChangePasswordAsync(1, oldPassword, "new_password");

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedCredential);
        Assert.True(BCrypt.Net.BCrypt.Verify("new_password", updatedCredential!.PasswordHash));
    }

    #endregion

    #region SetPasswordAsync Tests

    [Fact]
    public async Task SetPasswordAsync_ReturnsFalse_WhenCredentialNotFound()
    {
        // Arrange
        _mockCredentialRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<UserCredential, bool>>>()))
            .ReturnsAsync((UserCredential?)null);

        // Act
        var result = await _userService.SetPasswordAsync(1, "new_password");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SetPasswordAsync_ReturnsTrue_AndUpdatesPassword()
    {
        // Arrange
        var credential = new UserCredential { UserId = 1, PasswordHash = "old_hash" };
        _mockCredentialRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<UserCredential, bool>>>()))
            .ReturnsAsync(credential);
        _mockCredentialRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.SetPasswordAsync(1, "new_password");

        // Assert
        Assert.True(result);
        Assert.True(BCrypt.Net.BCrypt.Verify("new_password", credential.PasswordHash));
    }

    #endregion

    #region ChangeEmailAsync Tests

    [Fact]
    public async Task ChangeEmailAsync_ReturnsFalse_WhenUserNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.ChangeEmailAsync(1, "new@email.com", "password");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ChangeEmailAsync_ReturnsFalse_WhenPasswordIsWrong()
    {
        // Arrange
        var user = CreateTestUser(1);
        var credential = new UserCredential
        {
            UserId = 1,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct")
        };
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockCredentialRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<UserCredential, bool>>>()))
            .ReturnsAsync(credential);

        // Act
        var result = await _userService.ChangeEmailAsync(1, "new@email.com", "wrong");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ChangeEmailAsync_ReturnsFalse_WhenEmailAlreadyTaken()
    {
        // Arrange
        var user = CreateTestUser(1);
        var existingUser = CreateTestUser(2, email: "taken@email.com");
        var credential = new UserCredential
        {
            UserId = 1,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password")
        };

        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockCredentialRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<UserCredential, bool>>>()))
            .ReturnsAsync(credential);
        _mockUserRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(existingUser); // Email already taken

        // Act
        var result = await _userService.ChangeEmailAsync(1, "taken@email.com", "password");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region CreateUserAsync Tests

    [Fact]
    public async Task CreateUserAsync_ReturnsNull_WhenEmailExists()
    {
        // Arrange
        var existingUser = CreateTestUser(1, email: "existing@test.com");
        _mockUserRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _userService.CreateUserAsync("Jan", "Kowalski", "existing@test.com", "password", UserRole.STUDENT);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateUserAsync_CreatesUserAndCredentials()
    {
        // Arrange
        UserCredential? addedCredential = null;

        _mockUserRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null);
        _mockUserRepo.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _mockUserRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _mockCredentialRepo.Setup(r => r.AddAsync(It.IsAny<UserCredential>()))
            .Callback<UserCredential>(c => addedCredential = c)
            .Returns(Task.CompletedTask);
        _mockCredentialRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.CreateUserAsync("Jan", "Kowalski", "new@test.com", "password", UserRole.STUDENT, true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jan", result!.Name);
        Assert.Equal("Kowalski", result.Surname);
        Assert.Equal("new@test.com", result.Email);
        Assert.Equal(UserRole.STUDENT, result.Role);
        Assert.True(result.IsActive);

        Assert.NotNull(addedCredential);
        Assert.True(BCrypt.Net.BCrypt.Verify("password", addedCredential!.PasswordHash));
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    public async Task UpdateUserAsync_ReturnsNull_WhenUserNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.UpdateUserAsync(999, "Updated", null, null, null, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserAsync_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var user = CreateTestUser(1, "Original", "Name");
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null); // No email conflict
        _mockUserRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act - Only update name
        var result = await _userService.UpdateUserAsync(1, "Updated", null, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated", result!.Name);
        Assert.Equal("Name", result.Surname); // Unchanged
    }

    #endregion

    #region DeleteUserAsync Tests

    [Fact]
    public async Task DeleteUserAsync_ReturnsFalse_WhenUserNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.DeleteUserAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteUserAsync_DeletesUserAndCredentials()
    {
        // Arrange
        var user = CreateTestUser(1);
        var credential = new UserCredential { UserId = 1 };

        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockCredentialRepo.Setup(r => r.FindSingleAsync(It.IsAny<Expression<Func<UserCredential, bool>>>()))
            .ReturnsAsync(credential);
        _mockCredentialRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _mockUserRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _userService.DeleteUserAsync(1);

        // Assert
        Assert.True(result);
        _mockCredentialRepo.Verify(r => r.Remove(credential), Times.Once);
        _mockUserRepo.Verify(r => r.Remove(user), Times.Once);
    }

    #endregion

    #region ToggleUserStatusAsync Tests

    [Fact]
    public async Task ToggleUserStatusAsync_ReturnsFalse_WhenUserNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.ToggleUserStatusAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ToggleUserStatusAsync_TogglesActiveToInactive()
    {
        // Arrange
        var user = CreateTestUser(1, isActive: true);
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _userService.ToggleUserStatusAsync(1);

        // Assert
        Assert.True(result);
        Assert.False(user.IsActive);
    }

    [Fact]
    public async Task ToggleUserStatusAsync_TogglesInactiveToActive()
    {
        // Arrange
        var user = CreateTestUser(1, isActive: false);
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _userService.ToggleUserStatusAsync(1);

        // Assert
        Assert.True(result);
        Assert.True(user.IsActive);
    }

    #endregion

    #region Helper Methods

    private User CreateTestUser(int id = 1, string name = "Jan", string surname = "Kowalski", string email = "test@example.com", bool isActive = true)
    {
        return new User
        {
            Id = id,
            Name = name,
            Surname = surname,
            Email = email,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            Role = UserRole.STUDENT
        };
    }

    #endregion
}
