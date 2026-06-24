using System;

namespace FishClubAlginet.Contracts.Dtos.Responses.Competition;

public record CompetitionBiggestCatchDto(
    Guid CompetitionId,
    string CompetitionName,
    int FishermanId,
    string FishermanName,
    int WeightInGrams);
