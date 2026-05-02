namespace FishClubAlginet.Tests.Handlers;

public class RemoveRoleCommandHandlerTests
{
    private readonly Mock<IUserManagementService> _userManagementServiceMock;
    private readonly Mock<ILogger<RemoveRoleCommandHandler>> _loggerMock;
    private readonly RemoveRoleCommandHandler _handler;

    public RemoveRoleCommandHandlerTests()
    {
        _userManagementServiceMock = new Mock<IUserManagementService>();
        _loggerMock = new Mock<ILogger<RemoveRoleCommandHandler>>();
        _handler = new RemoveRoleCommandHandler(_userManagementServiceMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData(ApplicationConstants.Roles.Admin)]
    [InlineData(ApplicationConstants.Roles.Fisherman)]
    public async Task Handle_WhenValidRoleAndUserId_ShouldRemoveRoleSuccessfully(string role)
    {
        // Arrange
        const string userId = "user-1";
        _userManagementServiceMock.Setup(x => x.RemoveRoleAsync(userId, role))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(new RemoveRoleCommand(userId, role), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value);
        _userManagementServiceMock.Verify(x => x.RemoveRoleAsync(userId, role), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenInvalidRole_ShouldReturnValidationErrorAndNotCallService()
    {
        // Arrange
        const string userId = "user-1";
        const string invalidRole = "SuperAdmin";

        // Act
        var result = await _handler.Handle(new RemoveRoleCommand(userId, invalidRole), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "Roles.InvalidRole");
        // No debe haber tocado al servicio si la validación falla.
        _userManagementServiceMock.Verify(
            x => x.RemoveRoleAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
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

        _userManagementServiceMock.Setup(x => x.RemoveRoleAsync(userId, ApplicationConstants.Roles.Fisherman))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _handler.Handle(
            new RemoveRoleCommand(userId, ApplicationConstants.Roles.Fisherman),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "UserNotFound");
    }

    [Fact]
    public async Task Handle_WhenRoleRemovalFails_ShouldReturnAllErrors()
    {
        // Arrange
        const string userId = "user-1";
        var identityResult = IdentityResult.Failed(
            new IdentityError { Code = "RoleRemoveError", Description = "Cannot remove role." });

        _userManagementServiceMock.Setup(x => x.RemoveRoleAsync(userId, ApplicationConstants.Roles.Admin))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _handler.Handle(
            new RemoveRoleCommand(userId, ApplicationConstants.Roles.Admin),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "RoleRemoveError");
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ShouldLogInformation()
    {
        // Arrange
        const string userId = "user-1";
        _userManagementServiceMock
            .Setup(x => x.RemoveRoleAsync(userId, ApplicationConstants.Roles.Admin))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _handler.Handle(
            new RemoveRoleCommand(userId, ApplicationConstants.Roles.Admin),
            CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Role") && v.ToString()!.Contains("removed")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
