using FishClubAlginet.Application.Features.Events.Commands.Competitions;

namespace FishClubAlginet.Application.Features.Events.Handlers;

public class CompetitionCreatedDomainEventHandler : INotificationHandler<CompetitionCreatedDomainEvent>
{
    private readonly ILogger<CompetitionCreatedDomainEventHandler> _logger;

    public CompetitionCreatedDomainEventHandler(ILogger<CompetitionCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CompetitionCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Competition creada: ID {CompetitionId}, Número {CompetitionNumber}, Nombre '{Name}' en Liga {LeagueId}",
            notification.CompetitionId,
            notification.CompetitionNumber,
            notification.Name,
            notification.LeagueId);

        return Task.CompletedTask;
    }
}
