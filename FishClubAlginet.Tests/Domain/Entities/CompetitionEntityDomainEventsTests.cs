namespace FishClubAlginet.Tests.Domain.Entities;

public class CompetitionEntityDomainEventsTests
{
    private static Competition BuildCompetition(
        Guid? leagueId = null,
        int competitionNumber = 1,
        string name = "Concurso Albufera",
        int maxSpots = 20,
        int? minWeight = 1000)
    {
        return Competition.Create(
            leagueId: leagueId ?? Guid.NewGuid(),
            competitionNumber: competitionNumber,
            name: name,
            date: DateTime.UtcNow.AddDays(7),
            startTime: new TimeSpan(8, 0, 0),
            endTime: new TimeSpan(14, 0, 0),
            venue: "Albufera",
            zone: "Zona Norte",
            subspecialty: Subspecialty.AguaDulce,
            category: Category.Seniors,
            maxSpots: maxSpots,
            biggestCatchMinWeightInGrams: minWeight);
    }

    [Fact]
    public void Create_ShouldInitializePlannedStatusAndZeroParticipants()
    {
        // Arrange & Act
        var competition = BuildCompetition();

        // Assert
        competition.Status.Should().Be(CompetitionStatus.Planned);
        competition.ParticipantCount.Should().Be(0);
        competition.Name.Should().Be("Concurso Albufera");
        competition.MaxSpots.Should().Be(20);
        competition.BiggestCatchMinWeightInGrams.Should().Be(1000);
        competition.LastUpdateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void OpenRegistration_ShouldTransitionStatusToRegistrationOpen()
    {
        // Arrange
        var competition = BuildCompetition();

        // Act
        competition.OpenRegistration();

        // Assert
        competition.Status.Should().Be(CompetitionStatus.RegistrationOpen);
        competition.LastUpdateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CloseRegistration_ShouldTransitionStatusToClosed()
    {
        // Arrange
        var competition = BuildCompetition();
        competition.OpenRegistration();

        // Act
        competition.CloseRegistration();

        // Assert
        competition.Status.Should().Be(CompetitionStatus.Closed);
        competition.LastUpdateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ReopenRegistration_WithinWindow_ShouldReturnTrueAndTransitionStatus()
    {
        // Arrange
        var competition = BuildCompetition();
        competition.OpenRegistration();
        competition.CloseRegistration();

        // Act
        var reopened = competition.ReopenRegistration();

        // Assert
        reopened.Should().BeTrue();
        competition.Status.Should().Be(CompetitionStatus.RegistrationOpen);
    }

    [Fact]
    public void MoveToResultsDraft_ShouldTransitionStatusToResultsDraft()
    {
        // Arrange
        var competition = BuildCompetition();
        competition.OpenRegistration();
        competition.CloseRegistration();

        // Act
        competition.MoveToResultsDraft();

        // Assert
        competition.Status.Should().Be(CompetitionStatus.ResultsDraft);
    }

    [Fact]
    public void ValidateResults_ShouldTransitionStatusToResultsValidated()
    {
        // Arrange
        var competition = BuildCompetition();
        competition.OpenRegistration();
        competition.CloseRegistration();
        competition.MoveToResultsDraft();

        // Act
        competition.ValidateResults();

        // Assert
        competition.Status.Should().Be(CompetitionStatus.ResultsValidated);
    }

    [Fact]
    public void IncrementParticipantCount_WhenUnderMaxSpots_ShouldIncrementAndReturnTrue()
    {
        // Arrange
        var competition = BuildCompetition(maxSpots: 2);

        // Act
        var first = competition.IncrementParticipantCount();
        var second = competition.IncrementParticipantCount();
        var third = competition.IncrementParticipantCount();

        // Assert
        first.Should().BeTrue();
        second.Should().BeTrue();
        third.Should().BeFalse();
        competition.ParticipantCount.Should().Be(2);
    }

    [Fact]
    public void DecrementParticipantCount_ShouldNotDropBelowZero()
    {
        // Arrange
        var competition = BuildCompetition();
        competition.IncrementParticipantCount();

        // Act
        competition.DecrementParticipantCount();
        competition.DecrementParticipantCount();

        // Assert
        competition.ParticipantCount.Should().Be(0);
    }

    [Fact]
    public void SetBiggestCatchMinWeight_ShouldUpdateWeightThreshold()
    {
        // Arrange
        var competition = BuildCompetition(minWeight: null);

        // Act
        competition.SetBiggestCatchMinWeight(1500);

        // Assert
        competition.BiggestCatchMinWeightInGrams.Should().Be(1500);
    }

    [Fact]
    public void RaiseDomainEvent_ShouldAddEventToDomainEventsCollection()
    {
        // Arrange
        var competition = BuildCompetition();
        var domainEvent = new CompetitionCreatedDomainEvent
        {
            CompetitionId = competition.Id,
            LeagueId = competition.LeagueId,
            CompetitionNumber = competition.CompetitionNumber,
            Name = competition.Name,
            Date = competition.Date,
            MaxSpots = competition.MaxSpots
        };

        // Act
        competition.RaiseDomainEvent(domainEvent);

        // Assert
        competition.GetDomainEvents().Should().ContainSingle().Which.Should().Be(domainEvent);

        // Act Clear
        competition.ClearDomainEvents();
        competition.GetDomainEvents().Should().BeEmpty();
    }
}
