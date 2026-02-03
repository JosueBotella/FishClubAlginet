using FishClubAlginet.Application.Features.Auth.Commands;

namespace FishClubAlginet.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FisherMenController : ApiController
{
    private readonly IRequestHandler<FisherManCommand, string> _handler;

    public FisherMenController(
        IRequestHandler<FisherManCommand, string> handler
    {
        _handler = handler;
    }

    [HttpPost("Add")]
    public async Task<IActionResult> Add([FromBody] RegisterUserDto request)
    {
        var command = new FisherManCommand(
            request.Email,
            request.Password,
            request.ConfirmPassword
        );

        var result = await _handler.Handle(command, default);

        return result.Match(
            token => Ok(new { Token = token }),
            errors => Problem(errors)
        );
    }
   
}
