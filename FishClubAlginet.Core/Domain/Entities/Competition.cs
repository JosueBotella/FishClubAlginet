namespace FishClubAlginet.Core.Domain.Entities;

public class Competition : BaseEntity<Guid>
{
    public Guid LeagueId { get; set; }
    public League League { get; set; } = null!;

    public int CompetitionNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public Subspecialty Subspecialty { get; set; }
    public Category Category { get; set; }
    public CompetitionStatus Status { get; set; } = CompetitionStatus.Planned;
    public int MaxSpots { get; set; }
    public int ParticipantCount { get; set; }

    /// <summary>
    /// Optional minimum weight (in grams) for a catch to qualify as "pieza mayor".
    /// When null, any catch is considered valid. Configurable per zone/competition.
    /// </summary>
    public int? BiggestCatchMinWeightInGrams { get; set; }

    /// <summary>Opens registration (Planned → RegistrationOpen).</summary>
    public void OpenRegistration()
    {
        Status = CompetitionStatus.RegistrationOpen;
        LastUpdateUtc = DateTime.UtcNow;
    }

    /// <summary>Closes registration (RegistrationOpen → Closed).</summary>
    public void CloseRegistration()
    {
        Status = CompetitionStatus.Closed;
        LastUpdateUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Reopens registration (Closed → RegistrationOpen).
    /// Only allowed within 30 days of closing (LastUpdateUtc).
    /// Returns false if the window has expired.
    /// </summary>
    public bool ReopenRegistration()
    {
        var daysSinceClosed = (DateTime.UtcNow - LastUpdateUtc).TotalDays;
        if (daysSinceClosed > 30)
            return false;

        Status = CompetitionStatus.RegistrationOpen;
        LastUpdateUtc = DateTime.UtcNow;
        return true;
    }

    /// <summary>Moves to results draft (Closed → ResultsDraft).</summary>
    public void MoveToResultsDraft()
    {
        Status = CompetitionStatus.ResultsDraft;
        LastUpdateUtc = DateTime.UtcNow;
    }

    /// <summary>Validates results (ResultsDraft → ResultsValidated).</summary>
    public void ValidateResults()
    {
        Status = CompetitionStatus.ResultsValidated;
        LastUpdateUtc = DateTime.UtcNow;
    }

    /// <summary>Updates the optional minimum weight threshold for "pieza mayor".</summary>
    public void SetBiggestCatchMinWeight(int? minWeightInGrams)
    {
        BiggestCatchMinWeightInGrams = minWeightInGrams;
        LastUpdateUtc = DateTime.UtcNow;
    }
}
