namespace FishClubAlginet.API.Controllers;

[Route("api/[controller]")]
[ApiController]
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
    public async Task<IActionResult> GetAll()
    {
        var query = new FisherManGetAllQuery();
        var result = await _mediator.Send(query, default);

        return result.Match(
            fishermen => Ok(fishermen),
            errors => Problem(errors)
        );
    }

}
