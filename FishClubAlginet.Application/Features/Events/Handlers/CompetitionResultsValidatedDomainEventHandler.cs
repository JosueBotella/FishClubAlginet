using FishClubAlginet.Application.Features.Events.Commands.Competitions;

namespace FishClubAlginet.Application.Features.Events.Handlers;

public class CompetitionResultsValidatedDomainEventHandler : INotificationHandler<CompetitionResultsValidatedDomainEvent>
{
    private readonly ILogger<CompetitionResultsValidatedDomainEventHandler> _logger;

    public CompetitionResultsValidatedDomainEventHandler(ILogger<CompetitionResultsValidatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CompetitionResultsValidatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Resultados validados para Competition: ID {CompetitionId}, Número {CompetitionNumber} en Liga {LeagueId}",
            notification.CompetitionId,
            notification.CompetitionNumber,
            notification.LeagueId);

        return Task.CompletedTask;
    }
}
