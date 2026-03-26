namespace FishClubAlginet.Application.Features.Users.Commands;

public record UnblockUserCommand(string UserId) : IRequest<ErrorOr<bool>>;

public class UnblockUserCommandHandler : IRequestHandler<UnblockUserCommand, ErrorOr<bool>>
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<UnblockUserCommandHandler> _logger;

    public UnblockUserCommandHandler(IUserManagementService userManagementService, ILogger<UnblockUserCommandHandler> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    public async Task<ErrorOr<bool>> Handle(UnblockUserCommand request, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.UnblockUserAsync(request.UserId);

        if (!result.Succeeded)
        {
            _logger.LogError("Error unblocking user {UserId}: {Errors}", request.UserId,
                string.Join(", ", result.Errors.Select(e => e.Description)));

            return result.Errors
                .Select(e => Error.Failure(e.Code, e.Description))
                .ToList();
        }

        _logger.LogInformation("User {UserId} unblocked successfully", request.UserId);
        return true;
    }
}
