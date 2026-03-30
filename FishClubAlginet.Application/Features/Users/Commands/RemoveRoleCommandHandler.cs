namespace FishClubAlginet.Application.Features.Users.Commands;

public record RemoveRoleCommand(string UserId, string Role) : IRequest<ErrorOr<bool>>;

public class RemoveRoleCommandHandler : IRequestHandler<RemoveRoleCommand, ErrorOr<bool>>
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<RemoveRoleCommandHandler> _logger;

    public RemoveRoleCommandHandler(IUserManagementService userManagementService, ILogger<RemoveRoleCommandHandler> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    public async Task<ErrorOr<bool>> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.Role != ApplicationConstants.Roles.Admin && request.Role != ApplicationConstants.Roles.Fisherman)
        {
            return Error.Validation("Roles.InvalidRole", ErrorMessages.User_InvalidRole);
        }

        var result = await _userManagementService.RemoveRoleAsync(request.UserId, request.Role);

        if (!result.Succeeded)
        {
            _logger.LogError("Error removing role {Role} from user {UserId}: {Errors}",
                request.Role, request.UserId,
                string.Join(", ", result.Errors.Select(e => e.Description)));

            return result.Errors
                .Select(e => Error.Failure(e.Code, e.Description))
                .ToList();
        }

        _logger.LogInformation("Role {Role} removed from user {UserId} successfully", request.Role, request.UserId);
        return true;
    }
}
