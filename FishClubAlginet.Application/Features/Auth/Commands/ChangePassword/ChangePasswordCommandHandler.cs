namespace FishClubAlginet.Application.Features.Auth.Commands;

public record ChangePasswordCommand(
    string UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<ErrorOr<bool>>;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ErrorOr<bool>>
{
    private readonly IAuthService _authService;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(IAuthService authService, ILogger<ChangePasswordCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<ErrorOr<bool>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.ChangePasswordAsync(command.UserId, command.CurrentPassword, command.NewPassword);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Password change failed for user {UserId}: {Errors}",
                    command.UserId,
                    string.Join(", ", result.Errors.Select(e => e.Description)));

                return result.Errors
                    .Select(e => Error.Validation(e.Code, e.Description))
                    .ToList();
            }

            _logger.LogInformation("Password changed successfully for user {UserId}", command.UserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error changing password for user {UserId}", command.UserId);
            return Error.Failure("Auth.ChangePasswordFailed", ex.Message);
        }
    }
}
