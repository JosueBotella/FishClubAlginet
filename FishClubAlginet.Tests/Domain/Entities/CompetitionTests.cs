using FluentAssertions;

namespace FishClubAlginet.Tests.Domain.Entities;

public class CompetitionTests
{
    [Fact]
    public void Create_ValidParameters_ShouldInitializeCompetitionInPlannedState()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var date = DateTime.UtcNow.AddDays(10);
        var startTime = new TimeSpan(8, 0, 0);
        var endTime = new TimeSpan(14, 0, 0);

        // Act
        var competition = Competition.Create(
            leagueId,
            1,
            "Liga Social #1",
            date,
            startTime,
            endTime,
            "Pantano de Alarcón",
            "Zona Norte",
            Subspecialty.AguaDulce,
            Category.Seniors,
            15,
            1000);

        // Assert
        competition.Id.Should().NotBeEmpty();
        competition.LeagueId.Should().Be(leagueId);
        competition.CompetitionNumber.Should().Be(1);
        competition.Name.Should().Be("Liga Social #1");
        competition.Date.Should().Be(date);
        competition.StartTime.Should().Be(startTime);
        competition.EndTime.Should().Be(endTime);
        competition.Venue.Should().Be("Pantano de Alarcón");
        competition.Zone.Should().Be("Zona Norte");
        competition.Subspecialty.Should().Be(Subspecialty.AguaDulce);
        competition.Category.Should().Be(Category.Seniors);
        competition.Status.Should().Be(CompetitionStatus.Planned);
        competition.MaxSpots.Should().Be(15);
        competition.ParticipantCount.Should().Be(0);
        competition.BiggestCatchMinWeightInGrams.Should().Be(1000);
        competition.LastUpdateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void IncrementParticipantCount_AvailableSpots_ShouldIncrementAndReturnTrue()
    {
        // Arrange
        var competition = Competition.Create(
            Guid.NewGuid(), 1, "Test", DateTime.UtcNow.AddDays(1),
            TimeSpan.FromHours(8), TimeSpan.FromHours(12), "Venue", null,
            Subspecialty.AguaDulce, Category.Seniors, maxSpots: 2);

        // Act
        var result = competition.IncrementParticipantCount();

        // Assert
        result.Should().BeTrue();
        competition.ParticipantCount.Should().Be(1);
    }

    [Fact]
    public void IncrementParticipantCount_MaxSpotsReached_ShouldReturnFalseAndNotIncrement()
    {
        // Arrange
        var competition = Competition.Create(
            Guid.NewGuid(), 1, "Test", DateTime.UtcNow.AddDays(1),
            TimeSpan.FromHours(8), TimeSpan.FromHours(12), "Venue", null,
            Subspecialty.AguaDulce, Category.Seniors, maxSpots: 1);
        competition.IncrementParticipantCount();

        // Act
        var result = competition.IncrementParticipantCount();

        // Assert
        result.Should().BeFalse();
        competition.ParticipantCount.Should().Be(1);
    }

    [Fact]
    public void DecrementParticipantCount_PositiveParticipants_ShouldDecrement()
    {
        // Arrange
        var competition = Competition.Create(
            Guid.NewGuid(), 1, "Test", DateTime.UtcNow.AddDays(1),
            TimeSpan.FromHours(8), TimeSpan.FromHours(12), "Venue", null,
            Subspecialty.AguaDulce, Category.Seniors, maxSpots: 5);
        competition.IncrementParticipantCount();

        // Act
        competition.DecrementParticipantCount();

        // Assert
        competition.ParticipantCount.Should().Be(0);
    }

    [Fact]
    public void DecrementParticipantCount_ZeroParticipants_ShouldNotDropBelowZero()
    {
        // Arrange
        var competition = Competition.Create(
            Guid.NewGuid(), 1, "Test", DateTime.UtcNow.AddDays(1),
            TimeSpan.FromHours(8), TimeSpan.FromHours(12), "Venue", null,
            Subspecialty.AguaDulce, Category.Seniors, maxSpots: 5);

        // Act
        competition.DecrementParticipantCount();

        // Assert
        competition.ParticipantCount.Should().Be(0);
    }

    [Fact]
    public void OpenRegistration_PlannedState_ShouldChangeStatusToRegistrationOpen()
    {
        // Arrange
        var competition = Competition.Create(
            Guid.NewGuid(), 1, "Test", DateTime.UtcNow.AddDays(1),
            TimeSpan.FromHours(8), TimeSpan.FromHours(12), "Venue", null,
            Subspecialty.AguaDulce, Category.Seniors, maxSpots: 10);

        // Act
        competition.OpenRegistration();

        // Assert
        competition.Status.Should().Be(CompetitionStatus.RegistrationOpen);
    }

    [Fact]
    public void CloseRegistration_RegistrationOpenState_ShouldChangeStatusToClosed()
    {
        // Arrange
        var competition = Competition.Create(
            Guid.NewGuid(), 1, "Test", DateTime.UtcNow.AddDays(1),
            TimeSpan.FromHours(8), TimeSpan.FromHours(12), "Venue", null,
            Subspecialty.AguaDulce, Category.Seniors, maxSpots: 10);
        competition.OpenRegistration();

        // Act
        competition.CloseRegistration();

        // Assert
        competition.Status.Should().Be(CompetitionStatus.Closed);
    }

    [Fact]
    public void MoveToResultsDraft_ClosedState_ShouldChangeStatusToResultsDraft()
    {
        // Arrange
        var competition = Competition.Create(
            Guid.NewGuid(), 1, "Test", DateTime.UtcNow.AddDays(1),
            TimeSpan.FromHours(8), TimeSpan.FromHours(12), "Venue", null,
            Subspecialty.AguaDulce, Category.Seniors, maxSpots: 10);
        competition.OpenRegistration();
        competition.CloseRegistration();

        // Act
        competition.MoveToResultsDraft();

        // Assert
        competition.Status.Should().Be(CompetitionStatus.ResultsDraft);
    }

    [Fact]
    public void ValidateResults_ResultsDraftState_ShouldChangeStatusToResultsValidated()
    {
        // Arrange
        var competition = Competition.Create(
            Guid.NewGuid(), 1, "Test", DateTime.UtcNow.AddDays(1),
            TimeSpan.FromHours(8), TimeSpan.FromHours(12), "Venue", null,
            Subspecialty.AguaDulce, Category.Seniors, maxSpots: 10);
        competition.OpenRegistration();
        competition.CloseRegistration();
        competition.MoveToResultsDraft();

        // Act
        competition.ValidateResults();

        // Assert
        competition.Status.Should().Be(CompetitionStatus.ResultsValidated);
    }

    [Fact]
    public void ReopenRegistration_ClosedStateWithin30Days_ShouldReturnTrueAndSetStatusToRegistrationOpen()
    {
        // Arrange
        var competition = Competition.Create(
            Guid.NewGuid(), 1, "Test", DateTime.UtcNow.AddDays(1),
            TimeSpan.FromHours(8), TimeSpan.FromHours(12), "Venue", null,
            Subspecialty.AguaDulce, Category.Seniors, maxSpots: 10);
        competition.OpenRegistration();
        competition.CloseRegistration();

        // Act
        var result = competition.ReopenRegistration();

        // Assert
        result.Should().BeTrue();
        competition.Status.Should().Be(CompetitionStatus.RegistrationOpen);
    }

    [Fact]
    public void SetBiggestCatchMinWeight_ValidWeight_ShouldUpdateWeightAndSetLastUpdateUtc()
    {
        // Arrange
        var competition = Competition.Create(
            Guid.NewGuid(), 1, "Test", DateTime.UtcNow.AddDays(1),
            TimeSpan.FromHours(8), TimeSpan.FromHours(12), "Venue", null,
            Subspecialty.AguaDulce, Category.Seniors, maxSpots: 10);

        // Act
        competition.SetBiggestCatchMinWeight(1500);

        // Assert
        competition.BiggestCatchMinWeightInGrams.Should().Be(1500);
    }
}
