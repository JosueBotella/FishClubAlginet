namespace FishClubAlginet.Core.Domain.Entities;

public class League : BaseEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public bool IsActive { get; set; }
    public bool IsArchived { get; set; }
    public int MinPoints { get; set; } = 5;
    public int WorstResultsToDiscard { get; set; } = 0;

    public ICollection<Competition> Competitions { get; set; } = new List<Competition>();

    public static League Create(string name, int year, int minPoints = 5, int worstResultsToDiscard = 0)
    {
        return new League
        {
            Id = Guid.NewGuid(),
            Name = name,
            Year = year,
            IsActive = false,
            IsArchived = false,
            MinPoints = minPoints,
            WorstResultsToDiscard = worstResultsToDiscard,
            LastUpdateUtc = DateTime.UtcNow
        };
    }

    public void Update(string name, int minPoints, int worstResultsToDiscard)
    {
        Name = name;
        MinPoints = minPoints;
        WorstResultsToDiscard = worstResultsToDiscard;
        LastUpdateUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        IsArchived = false;
        LastUpdateUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastUpdateUtc = DateTime.UtcNow;
    }

    public void Archive()
    {
        IsArchived = true;
        IsActive = false;
        LastUpdateUtc = DateTime.UtcNow;
    }
}

public static class LeagueConstraints
{
    public const int NameMaxLength = 100;
    public const int MinYear = 2000;
}
