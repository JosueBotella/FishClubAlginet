namespace FishClubAlginet.Contracts.Dtos.Responses.Competition;

public record CompetitionDto(
    Guid Id,
    Guid LeagueId,
    int CompetitionNumber,
    string? Name,
    DateTime Date,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Venue,
    string Zone,
    string Subspecialty,
    string Category,
    string Status,
    int MaxSpots,
    int ParticipantCount,
    DateTime LastUpdateUtc);

public record CompetitionResultDto(
    Guid Id,
    Guid CompetitionId,
    int FishermanId,
    int? AssignedSpotNumber,
    bool DidAttend,
    int WeightInGrams,
    int? BiggestCatchWeight,
    decimal Points,
    int Ranking,
    bool IsValidated,
    DateTime RegistrationDate);
