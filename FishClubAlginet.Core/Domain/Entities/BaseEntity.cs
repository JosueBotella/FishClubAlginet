namespace FishClubAlginet.Core.Domain.Entities;

public abstract class BaseEntity<TId>
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
