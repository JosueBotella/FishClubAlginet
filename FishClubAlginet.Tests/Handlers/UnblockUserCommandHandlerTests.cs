namespace FishClubAlginet.Tests.Handlers;

public class UnblockUserCommandHandlerTests
{
    private readonly Mock<IUserManagementService> _userManagementServiceMock;
    private readonly Mock<ILogger<UnblockUserCommandHandler>> _loggerMock;
    private readonly UnblockUserCommandHandler _handler;

    public UnblockUserCommandHandlerTests()
    {
        _userManagementServiceMock = new Mock<IUserManagementService>();
        _loggerMock = new Mock<ILogger<UnblockUserCommandHandler>>();
        _handler = new UnblockUserCommandHandler(_userManagementServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidUserId_ShouldUnblockUserSuccessfully()
    {
        // Arrange
        const string userId = "user-1";
        _userManagementServiceMock.Setup(x => x.UnblockUserAsync(userId))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(new UnblockUserCommand(userId), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value);
        _userManagementServiceMock.Verify(x => x.UnblockUserAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailureError()
    {
        // Arrange
        const string userId = "non-existent-user";
        var identityResult = IdentityResult.Failed(new IdentityError
        {
            Code = "UserNotFound",
            Description = $"User with id '{userId}' was not found."
        });

        _userManagementServiceMock.Setup(x => x.UnblockUserAsync(userId))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _handler.Handle(new UnblockUserCommand(userId), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "UserNotFound");
    }
}
