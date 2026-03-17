namespace FishClubAlginet.Infrastructure.Abstractions;

public interface IDomainEventHandlerService<in T> where T : IDomainEvent
{
    Task Handle(T domainEvent, CancellationToken cancellationToken = default);
}
