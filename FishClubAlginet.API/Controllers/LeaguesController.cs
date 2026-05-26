namespace FishClubAlginet.API.Controllers;

[Route("api/leagues")]
[ApiController]
[Authorize(Roles = $"{ApplicationConstants.Roles.Admin},{ApplicationConstants.Roles.Fisherman}")]
public class LeaguesController : ApiController
{
    private readonly IMediator _mediator;

    public LeaguesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = ApplicationConstants.Roles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateLeagueRequest request)
    {
        var command = new CreateLeagueCommand(
            request.Name,
            request.Year,
            request.MinPoints,
            request.WorstResultsToDiscard);

        var result = await _mediator.Send(command, default);
        return result.Match(
            id => CreatedAtAction(nameof(GetById), new { id }, new { Id = id }),
            errors => Problem(errors));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = ApplicationConstants.Roles.Admin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeagueRequest request)
    {
        var command = new UpdateLeagueCommand(
            id,
            request.Name,
            request.MinPoints,
            request.WorstResultsToDiscard);

        var result = await _mediator.Send(command, default);
        return result.Match(
            dto => Ok(dto),
            errors => Problem(errors));
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = ApplicationConstants.Roles.Admin)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var command = new ActivateLeagueCommand(id);
        var result = await _mediator.Send(command, default);
        return result.Match(
            dto => Ok(dto),
            errors => Problem(errors));
    }

    [HttpPost("{id:guid}/archive")]
    [Authorize(Roles = ApplicationConstants.Roles.Admin)]
    public async Task<IActionResult> Archive(Guid id)
    {
        var command = new ArchiveLeagueCommand(id);
        var result = await _mediator.Send(command, default);
        return result.Match(
            dto => Ok(dto),
            errors => Problem(errors));
    }

    /// <summary>Unarchives a league (IsArchived → false, IsActive stays false). Admin only.</summary>
    [HttpPut("{id:guid}/unarchive")]
    [Authorize(Roles = ApplicationConstants.Roles.Admin)]
    public async Task<IActionResult> Unarchive(Guid id)
    {
        var command = new UnarchiveLeagueCommand(id);
        var result = await _mediator.Send(command, default);
        return result.Match(
            dto => Ok(dto),
            errors => Problem(errors));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 15,
        [FromQuery] int? year = null,
        [FromQuery] bool? archived = null)
    {
        var query = new GetAllLeaguesQuery(skip, take, year, archived);
        var result = await _mediator.Send(query, default);
        return result.Match(
            paginated => Ok(paginated),
            errors => Problem(errors));
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var query = new GetActiveLeagueQuery();
        var result = await _mediator.Send(query, default);
        return result.Match(
            dto => Ok(dto),
            errors => Problem(errors));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetLeagueByIdQuery(id);
        var result = await _mediator.Send(query, default);
        return result.Match(
            dto => Ok(dto),
            errors => Problem(errors));
    }

    /// <summary>Returns league standings by weight and by points (with worst-results discard).</summary>
    [HttpGet("{id:guid}/standings")]
    public async Task<IActionResult> GetStandings(Guid id)
    {
        var query = new GetLeagueStandingsQuery(id);
        var result = await _mediator.Send(query, default);
        return result.Match(
            dto => Ok(dto),
            errors => Problem(errors));
    }

    /// <summary>Returns the detailed league standings matrix by weight and by points (with worst-results discard).</summary>
    [HttpGet("{id:guid}/standings-matrix")]
    public async Task<IActionResult> GetStandingsMatrix(Guid id)
    {
        var query = new GetLeagueStandingsMatrixQuery(id);
        var result = await _mediator.Send(query, default);
        return result.Match(
            dto => Ok(dto),
            errors => Problem(errors));
    }

    /// <summary>Returns the biggest catch of the season/league.</summary>
    [HttpGet("{id:guid}/biggest-catch")]
    public async Task<IActionResult> GetSeasonBiggestCatch(Guid id)
    {
        var query = new GetSeasonBiggestCatchQuery(id);
        var result = await _mediator.Send(query, default);
        return result.Match(
            dto => Ok(dto),
            errors => Problem(errors));
    }
}
