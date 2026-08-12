namespace FishClubAlginet.Application.Features.Events.Handlers;

public class FishermanRestoredDomainEventHandler : INotificationHandler<FishermanRestoredDomainEvent>
{
    private readonly ILogger<FishermanRestoredDomainEventHandler> _logger;

    public FishermanRestoredDomainEventHandler(ILogger<FishermanRestoredDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(FishermanRestoredDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fisherman restaurado: Id={Id}", notification.Id);

        return Task.CompletedTask;
    }
}
