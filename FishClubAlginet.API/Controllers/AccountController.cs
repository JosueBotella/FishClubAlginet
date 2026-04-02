using FishClubAlginet.Application.Features.Auth.Commands;
using System.Security.Claims;

namespace FishClubAlginet.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ApiController 
{
 
    private readonly IMediator _mediator;
    public AccountController(IMediator mediator)
        
    {
       _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
    {
        var command = new RegisterUserCommand(          
            request.Email,
            request.Password,
            request.ConfirmPassword
        );

        var result = await _mediator.Send(command, default);

        return result.Match(
            token => Ok(new { Token = token }),
            errors => Problem(errors)
        );
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var command = new LoginUserCommand(request.UserName, request.Password);

        var result = await _mediator.Send(command, default);

        return result.Match(
            token => Ok(new { Token = token }),
            errors => Problem(errors)
        );
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
        var result = await _mediator.Send(command, default);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors)
        );
    }
}
