namespace FishClubAlginet.Application.Features.Events.Commands.Competitions;

public class CompetitionResultsValidatedDomainEvent : IDomainEvent
{
    public Guid CompetitionId { get; set; }
    public Guid LeagueId { get; set; }
    public int CompetitionNumber { get; set; }
}
