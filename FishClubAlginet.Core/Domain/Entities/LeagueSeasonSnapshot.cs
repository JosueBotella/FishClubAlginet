namespace FishClubAlginet.Core.Domain.Entities;

public class LeagueSeasonSnapshot : BaseEntity<Guid>
{
    public Guid LeagueId { get; set; }
    public int Year { get; set; }
    public DateTime ArchivedAtUtc { get; set; }
    public string SnapshotDataJson { get; set; } = string.Empty;

    public League? League { get; set; }

    public static LeagueSeasonSnapshot Create(Guid leagueId, int year, string snapshotDataJson)
    {
        return new LeagueSeasonSnapshot
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            Year = year,
            ArchivedAtUtc = DateTime.UtcNow,
            SnapshotDataJson = snapshotDataJson,
            LastUpdateUtc = DateTime.UtcNow
        };
    }
}
