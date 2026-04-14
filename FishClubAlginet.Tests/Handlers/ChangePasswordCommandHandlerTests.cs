using FluentAssertions;

namespace FishClubAlginet.Tests.Handlers;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<ChangePasswordCommandHandler>> _loggerMock;
    private readonly ChangePasswordCommandHandler _handler;

    private const string UserId = "user-123";
    private const string CurrentPassword = "OldPass123!";
    private const string NewPassword = "NewPass456!";

    public ChangePasswordCommandHandlerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<ChangePasswordCommandHandler>>();
        _handler = new ChangePasswordCommandHandler(_authServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidPasswordChange_ShouldReturnTrue()
    {
        // Arrange
        _authServiceMock.Setup(x => x.ChangePasswordAsync(UserId, CurrentPassword, NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        var command = new ChangePasswordCommand(UserId, CurrentPassword, NewPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
        _authServiceMock.Verify(x => x.ChangePasswordAsync(UserId, CurrentPassword, NewPassword), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidCurrentPassword_ShouldReturnValidationErrors()
    {
        // Arrange
        var identityResult = IdentityResult.Failed(new IdentityError
        {
            Code = "PasswordMismatch",
            Description = "Incorrect password."
        });

        _authServiceMock.Setup(x => x.ChangePasswordAsync(UserId, CurrentPassword, NewPassword))
            .ReturnsAsync(identityResult);

        var command = new ChangePasswordCommand(UserId, CurrentPassword, NewPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Code.Should().Be("PasswordMismatch");
        result.Errors[0].Description.Should().Be("Incorrect password.");
    }

    [Fact]
    public async Task Handle_MultipleIdentityErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var identityResult = IdentityResult.Failed(
            new IdentityError { Code = "PasswordTooShort", Description = "Password too short." },
            new IdentityError { Code = "PasswordRequiresDigit", Description = "Password requires a digit." });

        _authServiceMock.Setup(x => x.ChangePasswordAsync(UserId, CurrentPassword, NewPassword))
            .ReturnsAsync(identityResult);

        var command = new ChangePasswordCommand(UserId, CurrentPassword, NewPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors.Select(e => e.Code).Should().Contain("PasswordTooShort");
        result.Errors.Select(e => e.Code).Should().Contain("PasswordRequiresDigit");
    }

    [Fact]
    public async Task Handle_ServiceThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        _authServiceMock.Setup(x => x.ChangePasswordAsync(UserId, CurrentPassword, NewPassword))
            .ThrowsAsync(new Exception("Database connection failed"));

        var command = new ChangePasswordCommand(UserId, CurrentPassword, NewPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Code.Should().Be("Auth.ChangePasswordFailed");
        result.Errors[0].Description.Should().Be("Database connection failed");
    }
}
