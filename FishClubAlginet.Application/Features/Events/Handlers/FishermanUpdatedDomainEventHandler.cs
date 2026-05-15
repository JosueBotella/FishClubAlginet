namespace FishClubAlginet.Application.Features.Events.Handlers;

public class FishermanUpdatedDomainEventHandler : INotificationHandler<FishermanUpdatedDomainEvent>
{
    private readonly ILogger<FishermanUpdatedDomainEventHandler> _logger;

    public FishermanUpdatedDomainEventHandler(ILogger<FishermanUpdatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(FishermanUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fisherman actualizado: Id={Id} {FirstName} {LastName}",
            notification.Id, notification.FirstName, notification.LastName);

        return Task.CompletedTask;
    }
}
