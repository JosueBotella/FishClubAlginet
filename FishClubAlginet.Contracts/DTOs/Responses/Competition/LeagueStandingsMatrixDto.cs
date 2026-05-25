namespace FishClubAlginet.Contracts.Dtos.Responses.Competition;

public record LeagueStandingsMatrixDto(
    Guid LeagueId,
    string LeagueName,
    int Year,
    int WorstResultsToDiscard,
    IReadOnlyList<CompetitionHeaderDto> Competitions,
    IReadOnlyList<FishermanMatrixRowDto> ByPoints,
    IReadOnlyList<FishermanMatrixRowDto> ByWeight);
