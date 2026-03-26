using FishClubAlginet.Contracts.Dtos.Responses.Users;

namespace FishClubAlginet.Application.Features.Users.Queries;

public record GetAllUsersQuery() : IRequest<ErrorOr<List<UserDto>>>;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, ErrorOr<List<UserDto>>>
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(IUserManagementService userManagementService, ILogger<GetAllUsersQueryHandler> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    public async Task<ErrorOr<List<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userManagementService.GetAllUsersAsync();
            return users.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return Error.Failure("Users.GetAllFailed", ErrorMessages.User_GetAllFailed);
        }
    }
}
