namespace FishClubAlginet.Contracts.Dtos.Responses.Competition;

public record CompetitionCellDto(
    int WeightInGrams,
    decimal Points,
    int Ranking,
    bool DidAttend,
    bool IsDiscarded);
