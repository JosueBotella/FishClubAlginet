using FishClubAlginet.Contracts.Dtos.Common;

namespace FishClubAlginet.Tests.Handlers;

public class GetAllUsersQueryHandlerTests
{
    private readonly Mock<IUserManagementService> _userManagementServiceMock;
    private readonly Mock<ILogger<GetAllUsersQueryHandler>> _loggerMock;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTests()
    {
        _userManagementServiceMock = new Mock<IUserManagementService>();
        _loggerMock = new Mock<ILogger<GetAllUsersQueryHandler>>();
        _handler = new GetAllUsersQueryHandler(_userManagementServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUsersExist_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new UserDto("user-1", "admin@test.com", false, new List<string> { "Admin" }),
            new UserDto("user-2", "fisherman@test.com", false, new List<string> { "Fisherman" })
        };

        _userManagementServiceMock.Setup(x => x.GetUsersPagedAsync(0, 10, null))
            .ReturnsAsync(new PaginatedResult<UserDto>(users, users.Count));

        // Act
        var result = await _handler.Handle(new GetAllUsersQuery(0, 10, null), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal("admin@test.com", result.Value.Items[0].Email);
        Assert.Equal("fisherman@test.com", result.Value.Items[1].Email);
    }

    [Fact]
    public async Task Handle_WhenNoUsersExist_ShouldReturnEmptyList()
    {
        // Arrange
        _userManagementServiceMock.Setup(x => x.GetUsersPagedAsync(0, 10, null))
            .ReturnsAsync(new PaginatedResult<UserDto>(new List<UserDto>(), 0));

        // Act
        var result = await _handler.Handle(new GetAllUsersQuery(0, 10, null), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public async Task Handle_WhenServiceThrows_ShouldReturnFailureError()
    {
        // Arrange
        _userManagementServiceMock.Setup(x => x.GetUsersPagedAsync(0, 10, null))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(new GetAllUsersQuery(0, 10, null), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "Users.GetAllFailed");
    }

    [Fact]
    public async Task Handle_WhenUserIsLockedOut_ShouldReturnUserWithLockedStatus()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new UserDto("user-1", "blocked@test.com", true, new List<string>())
        };

        _userManagementServiceMock.Setup(x => x.GetUsersPagedAsync(0, 10, null))
            .ReturnsAsync(new PaginatedResult<UserDto>(users, users.Count));

        // Act
        var result = await _handler.Handle(new GetAllUsersQuery(0, 10, null), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Single(result.Value.Items);
        Assert.True(result.Value.Items[0].IsLockedOut);
    }
}
