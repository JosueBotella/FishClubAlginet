namespace FishClubAlginet.Application.Features.Events.Handlers;

public class FishermanAddedDomainEventHandler : INotificationHandler<FishermanAddedDomainEvent>
{
    private readonly ILogger<FishermanAddedDomainEventHandler> _logger;

    public FishermanAddedDomainEventHandler(ILogger<FishermanAddedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(FishermanAddedDomainEvent notification, CancellationToken cancellationToken)
    {
                
        _logger.LogInformation("Fisherman creado: {FirstName} {LastName}", notification.FirstName, notification.LastName);
        
        return Task.CompletedTask;
    }
}
