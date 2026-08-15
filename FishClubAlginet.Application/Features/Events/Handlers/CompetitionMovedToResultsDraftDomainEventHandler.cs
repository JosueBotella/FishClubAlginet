using FishClubAlginet.Application.Features.Events.Commands.Competitions;

namespace FishClubAlginet.Application.Features.Events.Handlers;

public class CompetitionMovedToResultsDraftDomainEventHandler : INotificationHandler<CompetitionMovedToResultsDraftDomainEvent>
{
    private readonly ILogger<CompetitionMovedToResultsDraftDomainEventHandler> _logger;

    public CompetitionMovedToResultsDraftDomainEventHandler(ILogger<CompetitionMovedToResultsDraftDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CompetitionMovedToResultsDraftDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Competition movida a borrador de resultados: ID {CompetitionId} en Liga {LeagueId}",
            notification.CompetitionId,
            notification.LeagueId);

        return Task.CompletedTask;
    }
}
