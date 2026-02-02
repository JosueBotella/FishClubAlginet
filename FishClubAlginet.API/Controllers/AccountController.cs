using FishClubAlginet.Application.Features.Auth.Commands;

namespace FishClubAlginet.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ApiController 
{
    private readonly IRequestHandler<RegisterUserCommand, string> _registerHandler;
    private readonly IRequestHandler<LoginUserCommand, string> _loginHandler;

    public AccountController(
        IRequestHandler<RegisterUserCommand, string> registerHandler,
        IRequestHandler<LoginUserCommand, string> loginHandler)
    {
        _registerHandler = registerHandler;
        _loginHandler = loginHandler;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
    {
        var command = new RegisterUserCommand(          
            request.Email,
            request.Password,
            request.ConfirmPassword
        );

        var result = await _registerHandler.Handle(command, default);

        return result.Match(
            token => Ok(new { Token = token }),
            errors => Problem(errors)
        );
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var command = new LoginUserCommand(request.UserName, request.Password);

        var result = await _loginHandler.Handle(command, default);

        return result.Match(
            token => Ok(new { Token = token }),
            errors => Problem(errors)
        );
    }
}
