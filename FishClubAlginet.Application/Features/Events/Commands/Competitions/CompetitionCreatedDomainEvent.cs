namespace FishClubAlginet.Application.Features.Events.Commands.Competitions;

public class CompetitionCreatedDomainEvent : IDomainEvent
{
    public Guid CompetitionId { get; set; }
    public Guid LeagueId { get; set; }
    public int CompetitionNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int MaxSpots { get; set; }
}
