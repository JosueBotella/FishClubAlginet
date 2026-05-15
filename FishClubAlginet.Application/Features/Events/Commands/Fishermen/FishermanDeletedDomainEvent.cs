namespace FishClubAlginet.Application.Features.Events.Commands.Fishermen;

public class FishermanDeletedDomainEvent : IDomainEvent
{
    public int Id { get; set; }
}
