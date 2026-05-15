namespace FishClubAlginet.Application.Features.Events.Handlers;

public class FishermanDeletedDomainEventHandler : INotificationHandler<FishermanDeletedDomainEvent>
{
    private readonly ILogger<FishermanDeletedDomainEventHandler> _logger;

    public FishermanDeletedDomainEventHandler(ILogger<FishermanDeletedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(FishermanDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fisherman eliminado: Id={Id}", notification.Id);

        return Task.CompletedTask;
    }
}
