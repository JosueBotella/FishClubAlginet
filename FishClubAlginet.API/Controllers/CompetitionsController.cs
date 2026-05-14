using FishClubAlginet.Application.Features.Competitions;
using FishClubAlginet.Contracts.Dtos.Requests.Competition;

namespace FishClubAlginet.API.Controllers;

[Route("api/competitions")]
[ApiController]
[Authorize(Roles = $"{ApplicationConstants.Roles.Admin},{ApplicationConstants.Roles.Fisherman}")]
public class CompetitionsController : ApiController
{
    private readonly IMediator _mediator;

    public CompetitionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Returns all competitions for a league.</summary>
    [HttpGet]
    public async Task<IActionResult> GetByLeague([FromQuery] Guid leagueId)
    {
        var query = new GetCompetitionsByLeagueQuery(leagueId);
        var result = await _mediator.Send(query, default);
        return result.Match(
            dtos => Ok(dtos),
            errors => Problem(errors));
    }

    /// <summary>Creates a new competition inside a league. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = ApplicationConstants.Roles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateCompetitionRequest request)
    {
        var command = new CreateCompetitionCommand(
            request.LeagueId,
            request.CompetitionNumber,
            request.Name,
            request.Date,
            request.StartTime,
            request.EndTime,
            request.Venue,
            request.Zone,
            request.Subspecialty,
            request.Category,
            request.MaxSpots);

        var result = await _mediator.Send(command, default);
        return result.Match(
            id => CreatedAtAction(nameof(GetResults), new { id }, new { Id = id }),
            errors => Problem(errors));
    }

    /// <summary>Registers a fisherman to a competition. Admin or the fisherman themselves.</summary>
    [HttpPost("{id:guid}/register")]
    public async Task<IActionResult> Register(Guid id, [FromBody] RegisterFishermanRequest request)
    {
        var command = new RegisterFishermanCommand(id, request.FishermanId);
        var result = await _mediator.Send(command, default);
        return result.Match(
            resultId => Ok(new { Id = resultId }),
            errors => Problem(errors));
    }

    /// <summary>Opens registration for a competition (Planned -> RegistrationOpen). Admin only.</summary>
    [HttpPost("{id:guid}/open-registration")]
    [Authorize(Roles = ApplicationConstants.Roles.Admin)]
    public async Task<IActionResult> OpenRegistration(Guid id)
    {
        var result = await _mediator.Send(new OpenRegistrationCommand(id), default);
        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    /// <summary>Closes registration for a competition (RegistrationOpen -> Closed). Admin only.</summary>
    [HttpPost("{id:guid}/close-registration")]
    [Authorize(Roles = ApplicationConstants.Roles.Admin)]
    public async Task<IActionResult> CloseRegistration(Guid id)
    {
        var result = await _mediator.Send(new CloseRegistrationCommand(id), default);
        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    /// <summary>Removes a fisherman registration from a competition. Admin only.</summary>
    [HttpDelete("results/{resultId:guid}")]
    [Authorize(Roles = ApplicationConstants.Roles.Admin)]
    public async Task<IActionResult> RemoveRegistration(Guid resultId)
    {
        var result = await _mediator.Send(new RemoveRegistrationCommand(resultId), default);
        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    /// <summary>Updates attendance and weight data for a competition result. Admin only.</summary>
    [HttpPut("results/{resultId:guid}")]
    [Authorize(Roles = ApplicationConstants.Roles.Admin)]
    public async Task<IActionResult> UpdateResult(Guid resultId, [FromBody] UpdateCompetitionResultRequest request)
    {
        var command = new UpdateCompetitionResultCommand(
            resultId,
            request.DidAttend,
            request.WeightInGrams,
            request.BiggestCatchWeight);
        var result = await _mediator.Send(command, default);
        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    /// <summary>Returns all results for a competition with live rankings.</summary>
    [HttpGet("{id:guid}/results")]
    public async Task<IActionResult> GetResults(Guid id)
    {
        var query = new GetCompetitionResultsQuery(id);
        var result = await _mediator.Send(query, default);
        return result.Match(
            dtos => Ok(dtos),
            errors => Problem(errors));
    }
}
