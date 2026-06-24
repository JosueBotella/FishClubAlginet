namespace FishClubAlginet.Contracts.Dtos.Responses.Competition;

public record FishermanMatrixRowDto(
    int FishermanId,
    string FullName,
    int TotalWeightGrams,
    decimal TotalPoints,
    decimal PointsAfterDiscard,
    int CompetitionsAttended,
    IReadOnlyDictionary<Guid, CompetitionCellDto> ResultsPerCompetition);
