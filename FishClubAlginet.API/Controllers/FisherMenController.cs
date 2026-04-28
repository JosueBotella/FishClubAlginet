using System.Security.Claims;

namespace FishClubAlginet.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = $"{ApplicationConstants.Roles.Admin},{ApplicationConstants.Roles.Fisherman}")]
public class FisherMenController : ApiController
{
    private readonly IMediator _mediator;

    public FisherMenController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("Add")]
    public async Task<IActionResult> Add([FromBody] CreateFishermanDto request)
    {
        var command = new FisherManCommand(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.DocumentType,
            request.DocumentNumber,
            request.FederationLicense,
            request.AddressStreet,
            request.AddressCity,
            request.AddressZipCode,
            request.AddressProvince
        );

        var result = await _mediator.Send(command, default);

        return result.Match(
            fishermanId => Ok(new { Id = fishermanId }),
            errors => Problem(errors)
        );
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] string? search = null)
    {
        var query = new FisherManGetAllQuery(skip, take, search);
        var result = await _mediator.Send(query, default);

        return result.Match(
            fishermen => Ok(fishermen),
            errors => Problem(errors)
        );
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = ApplicationConstants.Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new SoftDeleteFishermanCommand(id);
        var result = await _mediator.Send(command, default);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors)
        );
    }

    [HttpGet("my-profile")]
    public async Task<IActionResult> MyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var query = new GetFishermanByUserIdQuery(userId);
        var result = await _mediator.Send(query, default);

        return result.Match(
            profile => Ok(profile),
            errors => Problem(errors)
        );
    }
}
