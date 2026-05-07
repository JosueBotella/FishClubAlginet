namespace FishClubAlginet.Core.Domain.Entities;

public class CompetitionResult : BaseEntity<Guid>
{
    public Guid CompetitionId { get; set; }
    public Competition Competition { get; set; } = null!;

    public Guid FishermanId { get; set; }
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
    public static CompetitionResult Register(Guid competitionId, Guid fishermanId) => new CompetitionResult
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
    /// Records the result after the competition.
    /// Applies minimum points rule: if attended but no catch, fisherman receives minPoints.
    /// </summary>
    public void RecordResult(bool didAttend, int weightInGrams, int? biggestCatchWeight, int minPoints)
    {
        DidAttend = didAttend;

        if (!didAttend)
        {
            WeightInGrams = 0;
            BiggestCatchWeight = null;
            Points = 0;
        }
        else
        {
            WeightInGrams = weightInGrams;
            BiggestCatchWeight = biggestCatchWeight;
            Points = weightInGrams > 0
                ? weightInGrams
                : minPoints;
        }

        LastUpdateUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the final ranking within the competition.
    /// </summary>
    public void SetRanking(int ranking)
    {
        Ranking = ranking;
        LastUpdateUtc = DateTime.UtcNow;
    }
}
