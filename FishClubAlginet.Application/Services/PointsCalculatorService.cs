using FishClubAlginet.Core.Domain.Entities;
using FishClubAlginet.Core.Domain.Services;

namespace FishClubAlginet.Application.Services;

/// <summary>
/// Calculates competition points and rankings.
///
/// Algorithm:
///   1. Sort attendees by WeightInGrams descending.
///   2. Every attendee receives <c>minPoints</c> just for attending.
///   3. Ranking bonus (fixed scale): position 1 → +20, position 2 → +19, … position 20 → +1, beyond → +0.
///   4. Ties within the top 20: each tied participant receives +1/tieCount bonus.
///   5. Non-attendees receive 0 points and ranking 0.
///
/// Example (minPoints = 5, attendees = 25):
///   Rank 1 → 5 + 20 = 25 pts
///   Rank 2 (tied with 3) → 5 + 19 + 0.5 = 24.5 pts
///   Rank 3 (tied with 2) → 5 + 18 + 0.5 = 23.5 pts
///   Rank 20 → 5 + 1 = 6 pts
///   Rank 21+ → 5 + 0 = 5 pts
/// </summary>
public sealed class PointsCalculatorService : IPointsCalculator
{
    private const int FirstPlaceBonus = 20;
    private const int MaxRankedPositions = 20;

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
            int j = i;
            while (j + 1 < n && attendees[j + 1].WeightInGrams == attendees[i].WeightInGrams)
                j++;

            int tieCount = j - i + 1;
            int groupStartRank = i + 1; // 1-indexed rank assigned to all in this group

            // 1 bonus point shared among tied participants, only when inside the top 20
            decimal tieBonus = (tieCount > 1 && groupStartRank <= MaxRankedPositions)
                ? 1m / tieCount
                : 0m;

            for (int k = i; k <= j; k++)
            {
                int position = k + 1; // 1-indexed position for ranking-points calculation
                decimal rankingBonus = Math.Max(0, FirstPlaceBonus + 1 - position);
                decimal points = minPoints + rankingBonus + tieBonus;
                attendees[k].SetCalculatedPoints(points, groupStartRank);
            }

            i = j + 1;
        }

        foreach (var nonAttendee in results.Where(r => !r.DidAttend))
            nonAttendee.SetCalculatedPoints(0m, 0);
    }
}
