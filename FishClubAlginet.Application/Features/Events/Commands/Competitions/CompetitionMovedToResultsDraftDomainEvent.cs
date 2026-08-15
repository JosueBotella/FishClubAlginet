namespace FishClubAlginet.Application.Features.Events.Commands.Competitions;

public class CompetitionMovedToResultsDraftDomainEvent : IDomainEvent
{
    public Guid CompetitionId { get; set; }
    public Guid LeagueId { get; set; }
}
