using FishClubAlginet.Application.Features.Events.Commands.Competitions;

namespace FishClubAlginet.Application.Features.Events.Handlers;

public class CompetitionRegistrationClosedDomainEventHandler : INotificationHandler<CompetitionRegistrationClosedDomainEvent>
{
    private readonly ILogger<CompetitionRegistrationClosedDomainEventHandler> _logger;

    public CompetitionRegistrationClosedDomainEventHandler(ILogger<CompetitionRegistrationClosedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CompetitionRegistrationClosedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Inscripciones cerradas para Competition: ID {CompetitionId}, Número {CompetitionNumber} en Liga {LeagueId} con {ParticipantCount} participantes",
            notification.CompetitionId,
            notification.CompetitionNumber,
            notification.LeagueId,
            notification.ParticipantCount);

        return Task.CompletedTask;
    }
}
