namespace FishClubAlginet.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FisherMenController : ApiController
{
    private readonly IRequestHandler<FisherManCommand, int> _addHandler;
    private readonly IRequestHandler<FisherManGetAllQuery, List<FisherManGetAllQueryResponse>> _getAllHandler;

    public FisherMenController(
        IRequestHandler<FisherManCommand, int> addHandler,  
        IRequestHandler<FisherManGetAllQuery, List<FisherManGetAllQueryResponse>> getAllHandler)
    {
        _addHandler = addHandler;
        _getAllHandler = getAllHandler;
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

        var result = await _addHandler.Handle(command, default);

        return result.Match(
            token => Ok(new { Token = token }),
            errors => Problem(errors)
        );
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        var query = new FisherManGetAllQuery();
        var result = await _getAllHandler.Handle(query, default);

        return result.Match(
            fishermen => Ok(fishermen),
            errors => Problem(errors)
        );
    }

}
