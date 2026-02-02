namespace FishClubAlginet.Core.Domain.Entities;

public abstract class BaseEntity<TId>
{
    public required TId Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedTimeUtc { get; set; }

    public DateTime LastUpdateUtc { get; set; }
}
