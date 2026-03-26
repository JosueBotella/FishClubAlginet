namespace FishClubAlginet.Tests.Handlers;

public class AssignRoleCommandHandlerTests
{
    private readonly Mock<IUserManagementService> _userManagementServiceMock;
    private readonly Mock<ILogger<AssignRoleCommandHandler>> _loggerMock;
    private readonly AssignRoleCommandHandler _handler;

    public AssignRoleCommandHandlerTests()
    {
        _userManagementServiceMock = new Mock<IUserManagementService>();
        _loggerMock = new Mock<ILogger<AssignRoleCommandHandler>>();
        _handler = new AssignRoleCommandHandler(_userManagementServiceMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData(ApplicationConstants.Roles.Admin)]
    [InlineData(ApplicationConstants.Roles.Fisherman)]
    public async Task Handle_WhenValidRoleAndUserId_ShouldAssignRoleSuccessfully(string role)
    {
        // Arrange
        const string userId = "user-1";
        _userManagementServiceMock.Setup(x => x.AssignRoleAsync(userId, role))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(new AssignRoleCommand(userId, role), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value);
        _userManagementServiceMock.Verify(x => x.AssignRoleAsync(userId, role), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenInvalidRole_ShouldReturnValidationError()
    {
        // Arrange
        const string userId = "user-1";
        const string invalidRole = "SuperAdmin";

        // Act
        var result = await _handler.Handle(new AssignRoleCommand(userId, invalidRole), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "Roles.InvalidRole");
        _userManagementServiceMock.Verify(x => x.AssignRoleAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
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

        _userManagementServiceMock.Setup(x => x.AssignRoleAsync(userId, ApplicationConstants.Roles.Fisherman))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _handler.Handle(
            new AssignRoleCommand(userId, ApplicationConstants.Roles.Fisherman),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "UserNotFound");
    }

    [Fact]
    public async Task Handle_WhenRoleAssignmentFails_ShouldReturnAllErrors()
    {
        // Arrange
        const string userId = "user-1";
        var identityResult = IdentityResult.Failed(
            new IdentityError { Code = "RoleAssignError", Description = "Cannot assign role." });

        _userManagementServiceMock.Setup(x => x.AssignRoleAsync(userId, ApplicationConstants.Roles.Admin))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _handler.Handle(
            new AssignRoleCommand(userId, ApplicationConstants.Roles.Admin),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "RoleAssignError");
    }
}
