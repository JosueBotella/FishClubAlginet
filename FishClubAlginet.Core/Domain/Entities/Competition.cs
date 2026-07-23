namespace FishClubAlginet.Core.Domain.Entities;

public class Competition : BaseEntity<Guid>
{
    public Guid LeagueId { get; private set; }
    public League League { get; private set; } = null!;

    public int CompetitionNumber { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public string Venue { get; private set; } = string.Empty;
    public string? Zone { get; private set; }
    public Subspecialty Subspecialty { get; private set; }
    public Category Category { get; private set; }
    public CompetitionStatus Status { get; private set; } = CompetitionStatus.Planned;
    public int MaxSpots { get; private set; }
    public int ParticipantCount { get; private set; }

    /// <summary>
    /// Optional minimum weight (in grams) for a catch to qualify as "pieza mayor".
    /// When null, any catch is considered valid. Configurable per zone/competition.
    /// </summary>
    public int? BiggestCatchMinWeightInGrams { get; private set; }

    /// <summary>
    /// Factory method to create a new Competition in Planned status.
    /// </summary>
    public static Competition Create(
        Guid leagueId,
        int competitionNumber,
        string? name,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        string venue,
        string? zone,
        Subspecialty subspecialty,
        Category category,
        int maxSpots,
        int? biggestCatchMinWeightInGrams = null,
        Guid? id = null,
        CompetitionStatus? status = null,
        DateTime? lastUpdateUtc = null)
    {
        return new Competition
        {
            Id = id ?? Guid.NewGuid(),
            LeagueId = leagueId,
            CompetitionNumber = competitionNumber,
            Name = name ?? string.Empty,
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            Venue = venue,
            Zone = zone,
            Subspecialty = subspecialty,
            Category = category,
            Status = status ?? CompetitionStatus.Planned,
            MaxSpots = maxSpots,
            ParticipantCount = 0,
            BiggestCatchMinWeightInGrams = biggestCatchMinWeightInGrams,
            LastUpdateUtc = lastUpdateUtc ?? DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates competition details.
    /// </summary>
    public void Update(
        string? name,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        string venue,
        string? zone,
        Subspecialty subspecialty,
        Category category,
        int maxSpots,
        int? biggestCatchMinWeightInGrams = null)
    {
        Name = name ?? string.Empty;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        Venue = venue;
        Zone = zone;
        Subspecialty = subspecialty;
        Category = category;
        MaxSpots = maxSpots;
        BiggestCatchMinWeightInGrams = biggestCatchMinWeightInGrams;
        LastUpdateUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Safely increments the participant count if spots are available.
    /// Returns true if successfully incremented, false if max spots reached.
    /// </summary>
    public bool IncrementParticipantCount()
    {
        if (ParticipantCount >= MaxSpots)
            return false;

        ParticipantCount++;
        LastUpdateUtc = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Safely decrements the participant count without dropping below 0.
    /// </summary>
    public void DecrementParticipantCount()
    {
        if (ParticipantCount > 0)
        {
            ParticipantCount--;
            LastUpdateUtc = DateTime.UtcNow;
        }
    }

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

public static class CompetitionConstraints
{
    public const int NameMaxLength = 100;
    public const int VenueMaxLength = 100;
    public const int ZoneMaxLength = 50;
}
