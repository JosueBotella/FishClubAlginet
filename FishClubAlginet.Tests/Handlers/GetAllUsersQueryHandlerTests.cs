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

        _userManagementServiceMock.Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("admin@test.com", result.Value[0].Email);
        Assert.Equal("fisherman@test.com", result.Value[1].Email);
    }

    [Fact]
    public async Task Handle_WhenNoUsersExist_ShouldReturnEmptyList()
    {
        // Arrange
        _userManagementServiceMock.Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(new List<UserDto>());

        // Act
        var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_WhenServiceThrows_ShouldReturnFailureError()
    {
        // Arrange
        _userManagementServiceMock.Setup(x => x.GetAllUsersAsync())
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

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

        _userManagementServiceMock.Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Single(result.Value);
        Assert.True(result.Value[0].IsLockedOut);
    }
}
