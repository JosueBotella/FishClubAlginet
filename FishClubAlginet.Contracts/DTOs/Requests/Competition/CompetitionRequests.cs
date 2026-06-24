namespace FishClubAlginet.Contracts.Dtos.Requests.Competition;

public record CreateCompetitionRequest(
    Guid LeagueId,
    int CompetitionNumber,
    string? Name,
    DateTime Date,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Venue,
    string? Zone,
    Subspecialty Subspecialty,
    Category Category,
    int MaxSpots,
    int? BiggestCatchMinWeightInGrams = null);

public record RegisterFishermanRequest(
    Guid CompetitionId,
    int FishermanId);

public record UpdateCompetitionResultRequest
{
    public bool DidAttend { get; init; }
    public int WeightInGrams { get; init; }
    public int? BiggestCatchWeight { get; init; }
}

public record UpdateBiggestCatchConfigRequest(int? MinWeightInGrams);

