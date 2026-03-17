namespace FishClubAlginet.Application.Features.Events.Commands.Fishermen;

public class FishermanAddedDomainEvent : IDomainEvent
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
}
