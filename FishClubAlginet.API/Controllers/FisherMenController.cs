namespace FishClubAlginet.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FisherMenController : ApiController
{
    private readonly IRequestHandler<FisherManCommand, int> _handler;

    public FisherMenController(
        IRequestHandler<FisherManCommand, int> handler)
    {
        _handler = handler;
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

        var result = await _handler.Handle(command, default);

        return result.Match(
            token => Ok(new { Token = token }),
            errors => Problem(errors)
        );
    }

    //[HttpGet("GetAll")]
    //public async Task<IActionResult> GetAll()
    //{     
    //    var result = await _handler.Handle(command, default);

    //    return result.Match(
    //        token => Ok(new { Token = token }),
    //        errors => Problem(errors)
    //    );
    //}

}
