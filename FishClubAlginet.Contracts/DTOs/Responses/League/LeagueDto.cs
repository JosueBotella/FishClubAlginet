namespace FishClubAlginet.Contracts.Dtos.Responses.League;

public record LeagueDto(
    Guid Id,
    string Name,
    int Year,
    bool IsActive,
    bool IsArchived,
    int MinPoints,
    int WorstResultsToDiscard,
    int CompetitionsCount,
    DateTime LastUpdateUtc);
