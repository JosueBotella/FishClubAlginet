using FishClubAlginet.Application.Features.Events.Commands.Competitions;

namespace FishClubAlginet.Application.Features.Events.Handlers;

public class CompetitionRegistrationOpenedDomainEventHandler : INotificationHandler<CompetitionRegistrationOpenedDomainEvent>
{
    private readonly ILogger<CompetitionRegistrationOpenedDomainEventHandler> _logger;

    public CompetitionRegistrationOpenedDomainEventHandler(ILogger<CompetitionRegistrationOpenedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CompetitionRegistrationOpenedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Inscripciones abiertas para Competition: ID {CompetitionId}, Número {CompetitionNumber} en Liga {LeagueId}",
            notification.CompetitionId,
            notification.CompetitionNumber,
            notification.LeagueId);

        return Task.CompletedTask;
    }
}
