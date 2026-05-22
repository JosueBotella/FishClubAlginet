namespace FishClubAlginet.Core.Domain.Services;

/// <summary>
/// Domain service that calculates competition points and rankings for a set of results.
///
/// Algorithm:
///   1. Sort attendees by WeightInGrams descending.
///   2. Every attendee receives <c>minPoints</c> just for attending.
///   3. Ranking bonus (fixed scale): position 1 → +20, position 2 → +19, … position 20 → +1, beyond → +0.
///   4. Ties within the top 20: each tied participant receives an additional +1/tieCount bonus.
///   5. Non-attendees receive 0 points and ranking 0.
/// </summary>
public interface IPointsCalculator
{
    /// <summary>
    /// Calculates and assigns <see cref="CompetitionResult.Points"/> and
    /// <see cref="CompetitionResult.Ranking"/> on each result in-place.
    /// </summary>
    void CalculateAndAssign(IReadOnlyList<CompetitionResult> results, int minPoints);
}
