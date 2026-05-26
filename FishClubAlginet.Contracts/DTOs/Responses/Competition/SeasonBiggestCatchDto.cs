using System;

namespace FishClubAlginet.Contracts.Dtos.Responses.Competition;

public record SeasonBiggestCatchDto(
    Guid LeagueId,
    string LeagueName,
    int FishermanId,
    string FishermanName,
    int WeightInGrams,
    Guid CompetitionId,
    string CompetitionName,
    int CompetitionNumber,
    DateTime CompetitionDate);
