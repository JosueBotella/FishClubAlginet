

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
        // Aquí haces lo que necesites con el evento
        // Ej: Enviar email, crear notificación, sincronizar con otro sistema, etc.
        
        _logger.LogInformation($"Fisherman creado: {notification.FirstName} {notification.LastName}");
        
        // Por ahora solo log, pero puedes agregar más lógica
        return Task.CompletedTask;
    }
}
