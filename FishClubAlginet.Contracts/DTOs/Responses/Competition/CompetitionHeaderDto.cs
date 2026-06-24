namespace FishClubAlginet.Contracts.Dtos.Responses.Competition;

public record CompetitionHeaderDto(
    Guid Id,
    int CompetitionNumber,
    string Name,
    DateTime Date);
