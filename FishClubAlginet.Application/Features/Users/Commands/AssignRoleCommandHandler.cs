namespace FishClubAlginet.Application.Features.Users.Commands;

public record AssignRoleCommand(string UserId, string Role) : IRequest<ErrorOr<bool>>;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, ErrorOr<bool>>
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<AssignRoleCommandHandler> _logger;

    public AssignRoleCommandHandler(IUserManagementService userManagementService, ILogger<AssignRoleCommandHandler> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    public async Task<ErrorOr<bool>> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.Role != ApplicationConstants.Roles.Admin && request.Role != ApplicationConstants.Roles.Fisherman)
        {
            return Error.Validation("Roles.InvalidRole", ErrorMessages.User_InvalidRole);
        }

        var result = await _userManagementService.AssignRoleAsync(request.UserId, request.Role);

        if (!result.Succeeded)
        {
            _logger.LogError("Error assigning role {Role} to user {UserId}: {Errors}",
                request.Role, request.UserId,
                string.Join(", ", result.Errors.Select(e => e.Description)));

            return result.Errors
                .Select(e => Error.Failure(e.Code, e.Description))
                .ToList();
        }

        _logger.LogInformation("Role {Role} assigned to user {UserId} successfully", request.Role, request.UserId);
        return true;
    }
}
