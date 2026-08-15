namespace FishClubAlginet.Application.Features.Events.Commands.Competitions;

public class CompetitionRegistrationClosedDomainEvent : IDomainEvent
{
    public Guid CompetitionId { get; set; }
    public Guid LeagueId { get; set; }
    public int CompetitionNumber { get; set; }
    public int ParticipantCount { get; set; }
}
