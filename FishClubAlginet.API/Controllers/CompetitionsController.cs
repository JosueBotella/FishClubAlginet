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

    /// <summary>Returns a single competition by id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCompetitionByIdQuery(id), default);
        return result.Match(
            dto => Ok(dto),
            errors => Problem(errors));
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
            request.MaxSpots,
            request.BiggestCatchMinWeightInGrams);

        var result = await _mediator.Send(command, default);
        return result.Match(
            id => CreatedAtAction(nameof(GetResults), new { id }, new { Id = id }),
            errors => Problem(errors));
    }

    /// <summary>Updates the "pieza mayor" minimum weight for a competition. Admin only.</summary>
    [HttpPatch("{id:guid}/biggest-catch-config")]
    [Authorize(Roles = ApplicationConstants.Roles.Admin)]
    public async Task<IActionResult> UpdateBiggestCatchConfig(Guid id, [FromBody] UpdateBiggestCatchConfigRequest request)
    {
        var command = new UpdateBiggestCatchConfigCommand(id, request.MinWeightInGrams);
        var result = await _mediator.Send(command, default);
        return result.Match(
            _ => NoContent(),
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

    /// <summary>Reopens registration (Closed -> RegistrationOpen, ≤30 days window). Admin only.</summary>
    [HttpPut("{id:guid}/reopen-registration")]
    [Authorize(Roles = ApplicationConstants.Roles.Admin)]
    public async Task<IActionResult> ReopenRegistration(Guid id)
    {
        var result = await _mediator.Send(new ReopenRegistrationCommand(id), default);
        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    /// <summary>Assigns fishing spots by sequential draw (RegistrationOpen). Admin only.</summary>
    [HttpPost("{id:guid}/assign-spots")]
    [Authorize(Roles = ApplicationConstants.Roles.Admin)]
    public async Task<IActionResult> AssignSpots(Guid id)
    {
        var result = await _mediator.Send(new AssignSpotsCommand(id), default);
        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    /// <summary>Moves competition to ResultsDraft (Closed -> ResultsDraft). Admin only.</summary>
    [HttpPost("{id:guid}/results-draft")]
    [Authorize(Roles = ApplicationConstants.Roles.Admin)]
    public async Task<IActionResult> MoveToResultsDraft(Guid id)
    {
        var result = await _mediator.Send(new MoveToResultsDraftCommand(id), default);
        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }

    /// <summary>Validates results (ResultsDraft -> ResultsValidated). Admin only.</summary>
    [HttpPost("{id:guid}/validate-results")]
    [Authorize(Roles = ApplicationConstants.Roles.Admin)]
    public async Task<IActionResult> ValidateResults(Guid id)
    {
        var result = await _mediator.Send(new ValidateResultsCommand(id), default);
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

    /// <summary>Returns the biggest catch for a specific competition.</summary>
    [HttpGet("{id:guid}/biggest-catch")]
    public async Task<IActionResult> GetCompetitionBiggestCatch(Guid id)
    {
        var query = new GetCompetitionBiggestCatchQuery(id);
        var result = await _mediator.Send(query, default);
        return result.Match(
            dto => Ok(dto),
            errors => Problem(errors));
    }
}
