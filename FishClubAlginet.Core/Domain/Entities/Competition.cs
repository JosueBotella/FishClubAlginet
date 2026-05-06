namespace FishClubAlginet.Core.Domain.Entities;

public class Competition : BaseEntity<Guid>
{
    public Guid LeagueId { get; set; }
    public League League { get; set; } = null!;

    public int CompetitionNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public string Subspecialty { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int MaxSpots { get; set; }
    public int ParticipantCount { get; set; }
}
