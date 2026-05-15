namespace FishClubAlginet.Core.Domain.Entities;

/// <summary>
/// Contrato no genérico para acceder a los domain events de cualquier entidad,
/// independientemente del tipo del identificador (int, Guid, etc.).
/// Permite que el SaveChangesInterceptor escanee todas las entidades del ChangeTracker
/// sin estar atado a un <typeparamref name="TId"/> concreto.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> GetDomainEvents();
    void ClearDomainEvents();
    void RaiseDomainEvent(IDomainEvent domainEvent);
}

public abstract class BaseEntity<TId> : IHasDomainEvents
{
    public required TId Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedTimeUtc { get; set; }

    public DateTime LastUpdateUtc { get; set; }


    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.ToList();

    public void ClearDomainEvents() => _domainEvents.Clear();

    public void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
