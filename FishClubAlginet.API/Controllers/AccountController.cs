using FishClubAlginet.Core.Interfaces;

namespace FishClubAlginet.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAuthService _authService;
    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto model)
    {
        var token = await _authService.LoginAsync(model);

        if (token == null)
        {
            return Unauthorized(ApplicationConstants.Authentication.LoginFailed);
        }

        return Ok(new { Token = token });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        var result = await _authService.RegisterAsync(model);

        if (result.Succeeded)
        {
            return Ok(new { Message = ApplicationConstants.Authentication.RegisterSuccess });
        }

        // Si hay errores (ej: contraseña débil), los devolvemos todos
        return BadRequest(result.Errors);
    }
}
