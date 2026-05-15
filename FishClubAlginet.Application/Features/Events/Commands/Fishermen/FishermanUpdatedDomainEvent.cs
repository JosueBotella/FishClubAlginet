namespace FishClubAlginet.Application.Features.Events.Commands.Fishermen;

public class FishermanUpdatedDomainEvent : IDomainEvent
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
