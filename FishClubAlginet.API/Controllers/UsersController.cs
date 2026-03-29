namespace FishClubAlginet.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = ApplicationConstants.Roles.Admin)]
public class UsersController : ApiController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllUsersQuery());
        return result.Match(users => Ok(users), errors => Problem(errors));
    }

    [HttpPost("{userId}/block")]
    public async Task<IActionResult> Block(string userId)
    {
        var result = await _mediator.Send(new BlockUserCommand(userId));
        return result.Match(_ => NoContent(), errors => Problem(errors));
    }

    [HttpPost("{userId}/unblock")]
    public async Task<IActionResult> Unblock(string userId)
    {
        var result = await _mediator.Send(new UnblockUserCommand(userId));
        return result.Match(_ => NoContent(), errors => Problem(errors));
    }

    [HttpPost("{userId}/assign-role")]
    public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleRequest request)
    {
        var result = await _mediator.Send(new AssignRoleCommand(userId, request.Role));
        return result.Match(_ => NoContent(), errors => Problem(errors));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var result = await _mediator.Send(new CreateUserCommand(request.Email, request.Password, request.Role));
        return result.Match(userId => Ok(new { Id = userId }), errors => Problem(errors));
    }
}
