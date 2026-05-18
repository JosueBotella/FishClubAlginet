namespace FishClubAlginet.Core.Domain.Services;

/// <summary>
/// Domain service that calculates competition points and rankings for a set of results.
/// Points are determined by ranking (highest weight = best rank).
/// Ties share the average of the points for the tied positions.
/// Every attendee receives at least <c>minPoints</c> regardless of ranking.
/// Non-attendees always receive 0 points.
/// </summary>
public interface IPointsCalculator
{
    /// <summary>
    /// Calculates and assigns <see cref="CompetitionResult.Points"/> and
    /// <see cref="CompetitionResult.Ranking"/> on each result in-place.
    /// </summary>
    void CalculateAndAssign(IReadOnlyList<CompetitionResult> results, int minPoints);
}
