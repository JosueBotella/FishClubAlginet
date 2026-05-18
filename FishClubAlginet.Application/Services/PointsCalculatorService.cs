using FishClubAlginet.Core.Domain.Entities;
using FishClubAlginet.Core.Domain.Services;

namespace FishClubAlginet.Application.Services;

/// <summary>
/// Calculates competition points and rankings.
///
/// Algorithm:
///   1. Sort attendees by WeightInGrams descending.
///   2. Points for position p (1-indexed) = N + 1 - p, where N = total attendees.
///   3. Ties share the average of the points for their tied positions.
///   4. Apply a minimum floor: every attendee gets at least <c>minPoints</c>.
///   5. Non-attendees receive 0 points and ranking 0.
///
/// Example (N = 5, minPoints = 5):
///   Rank 1 → 5 pts | Rank 2 → 4 pts | Rank 3+4 (tie) → (3+2)/2 = 2.5 → floor → 5 pts
///   Rank 5 → 1 pt → floor → 5 pts
/// </summary>
public sealed class PointsCalculatorService : IPointsCalculator
{
    public void CalculateAndAssign(IReadOnlyList<CompetitionResult> results, int minPoints)
    {
        var attendees = results
            .Where(r => r.DidAttend)
            .OrderByDescending(r => r.WeightInGrams)
            .ToList();

        int n = attendees.Count;
        int i = 0;

        while (i < n)
        {
            // Extend to include all participants with the same weight (tie group)
            int j = i;
            while (j + 1 < n && attendees[j + 1].WeightInGrams == attendees[i].WeightInGrams)
                j++;

            // Sum points for positions i+1 … j+1 (1-indexed)
            // Points for position p = n + 1 - p  →  for 0-indexed k: n + 1 - (k + 1) = n - k
            decimal totalPoints = 0;
            for (int k = i; k <= j; k++)
                totalPoints += n - k;

            int tieCount = j - i + 1;
            decimal avgPoints = totalPoints / tieCount;
            decimal finalPoints = Math.Max(avgPoints, minPoints);
            int rank = i + 1; // rank of the tie group = first position (1-indexed)

            for (int k = i; k <= j; k++)
                attendees[k].SetCalculatedPoints(finalPoints, rank);

            i = j + 1;
        }

        // Non-attendees: 0 points, rank 0
        foreach (var nonAttendee in results.Where(r => !r.DidAttend))
            nonAttendee.SetCalculatedPoints(0m, 0);
    }
}
