namespace FishClubAlginet.Application.Features.Users.Commands;

public record BlockUserCommand(string UserId) : IRequest<ErrorOr<bool>>;

public class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, ErrorOr<bool>>
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<BlockUserCommandHandler> _logger;

    public BlockUserCommandHandler(IUserManagementService userManagementService, ILogger<BlockUserCommandHandler> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    public async Task<ErrorOr<bool>> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.BlockUserAsync(request.UserId);

        if (!result.Succeeded)
        {
            _logger.LogError("Error blocking user {UserId}: {Errors}", request.UserId,
                string.Join(", ", result.Errors.Select(e => e.Description)));

            return result.Errors
                .Select(e => Error.Failure(e.Code, e.Description))
                .ToList();
        }

        _logger.LogInformation("User {UserId} blocked successfully", request.UserId);
        return true;
    }
}
