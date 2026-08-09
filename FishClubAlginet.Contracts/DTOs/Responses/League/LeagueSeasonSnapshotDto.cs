namespace FishClubAlginet.Contracts.Dtos.Responses.League;

public record LeagueSeasonSnapshotDto(
    Guid Id,
    Guid LeagueId,
    int Year,
    DateTime ArchivedAtUtc,
    string SnapshotDataJson);
