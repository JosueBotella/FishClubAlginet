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

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 15, [FromQuery] int? year = null)
    {
        var query = new GetAllLeaguesQuery(skip, take, year);
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
}
