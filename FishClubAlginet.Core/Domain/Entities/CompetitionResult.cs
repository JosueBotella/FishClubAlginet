namespace FishClubAlginet.Core.Domain.Entities;

public class CompetitionResult : BaseEntity<Guid>
{
    public Guid CompetitionId { get; set; }
    public Competition Competition { get; set; } = null!;

    public int FishermanId { get; set; }
    public Fisherman Fisherman { get; set; } = null!;

    public int? AssignedSpotNumber { get; set; }
    public DateTime RegistrationDate { get; set; }
    public bool IsValidated { get; set; }

    // Results — populated by admin after the competition
    public bool DidAttend { get; set; }
    public int WeightInGrams { get; set; }
    public int? BiggestCatchWeight { get; set; }
    public decimal Points { get; set; }
    public int Ranking { get; set; }

    /// <summary>
    /// Registers a fisherman for a competition (no spot, no results yet).
    /// </summary>
    public static CompetitionResult Register(Guid competitionId, int fishermanId) => new CompetitionResult
    {
        Id = Guid.NewGuid(),
        CompetitionId = competitionId,
        FishermanId = fishermanId,
        RegistrationDate = DateTime.UtcNow,
        IsValidated = false,
        LastUpdateUtc = DateTime.UtcNow
    };

    /// <summary>
    /// Assigns a fishing spot after admin draw.
    /// </summary>
    public void AssignSpot(int spotNumber)
    {
        AssignedSpotNumber = spotNumber;
        LastUpdateUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Records the raw result after the competition (weight, attendance).
    /// Points and ranking are calculated separately by <see cref="Core.Domain.Services.IPointsCalculator"/>
    /// when the competition moves to ResultsDraft.
    /// </summary>
    public void RecordResult(bool didAttend, int weightInGrams, int? biggestCatchWeight)
    {
        DidAttend = didAttend;
        WeightInGrams = didAttend ? weightInGrams : 0;
        BiggestCatchWeight = didAttend ? biggestCatchWeight : null;
        Points = 0;   // recalculated by IPointsCalculator on MoveToResultsDraft
        Ranking = 0;
        LastUpdateUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Assigns the calculated points and ranking produced by <see cref="Core.Domain.Services.IPointsCalculator"/>.
    /// </summary>
    public void SetCalculatedPoints(decimal points, int ranking)
    {
        Points = points;
        Ranking = ranking;
        LastUpdateUtc = DateTime.UtcNow;
    }
}
