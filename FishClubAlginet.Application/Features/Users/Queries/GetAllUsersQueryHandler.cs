using FishClubAlginet.Contracts.Dtos.Common;
using FishClubAlginet.Contracts.Dtos.Responses.Users;

namespace FishClubAlginet.Application.Features.Users.Queries;

public record GetAllUsersQuery(int Skip, int Take, string? Search) : IRequest<ErrorOr<PaginatedResult<UserDto>>>;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, ErrorOr<PaginatedResult<UserDto>>>
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(IUserManagementService userManagementService, ILogger<GetAllUsersQueryHandler> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    public async Task<ErrorOr<PaginatedResult<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _userManagementService.GetUsersPagedAsync(request.Skip, request.Take, request.Search);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return Error.Failure("Users.GetAllFailed", ErrorMessages.User_GetAllFailed);
        }
    }
}
