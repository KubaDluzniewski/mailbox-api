using Application.Interfaces;
using Application.Services;
using Core.Entity;
using Moq;
using Xunit;

namespace Application.Tests;

public class GroupServiceTests
{
    private readonly Mock<IGroupRepository> _groupRepo = new();
    private readonly GroupService _sut;

    public GroupServiceTests()
    {
        _sut = new GroupService(_groupRepo.Object);
    }

    [Fact]
    public async Task GetUsersFromGroup_ReturnsUsersFromRepository()
    {
        var users = new List<User>
        {
            new() { Id = 1, Name = "Jan", Surname = "Nowak", Email = "jan@mail.com", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Anna", Surname = "Kowal", Email = "anna@mail.com", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        _groupRepo.Setup(r => r.GetAllUsersAsyncByGroup(10)).ReturnsAsync(users);

        var result = await _sut.GetUsersFromGroup(10);

        Assert.Equal(2, result.Count);
        _groupRepo.Verify(r => r.GetAllUsersAsyncByGroup(10), Times.Once);
    }

    [Fact]
    public async Task SearchAndSuggestions_DelegateToSearchAsync()
    {
        var groups = new List<Group> { new() { Id = 1, Name = "IT" } };
        _groupRepo.Setup(r => r.SearchAsync("it", 5)).ReturnsAsync(groups);

        var search = await _sut.SearchAsync("it", 5);
        var suggestions = await _sut.GetSuggestionsAsync("it", 5);

        Assert.Single(search);
        Assert.Single(suggestions);
        _groupRepo.Verify(r => r.SearchAsync("it", 5), Times.Exactly(2));
    }

    [Fact]
    public async Task GetByIdAsync_DelegatesToRepository()
    {
        var group = new Group { Id = 7, Name = "Admins" };
        _groupRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(group);

        var result = await _sut.GetByIdAsync(7);

        Assert.NotNull(result);
        Assert.Equal("Admins", result!.Name);
    }
}
