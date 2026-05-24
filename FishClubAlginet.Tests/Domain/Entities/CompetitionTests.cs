using FluentAssertions;

namespace FishClubAlginet.Tests.Domain.Entities;

public class CompetitionTests
{
    [Fact]
    public void OpenRegistration_PlannedState_ShouldChangeStatusToRegistrationOpen()
    {
        // Arrange
        var competition = new Competition
        {
            Id = Guid.NewGuid(),
            Status = CompetitionStatus.Planned,
            LastUpdateUtc = DateTime.UtcNow.AddMinutes(-5)
        };
        var initialUpdate = competition.LastUpdateUtc;

        // Act
        competition.OpenRegistration();

        // Assert
        competition.Status.Should().Be(CompetitionStatus.RegistrationOpen);
        competition.LastUpdateUtc.Should().BeAfter(initialUpdate);
    }

    [Fact]
    public void CloseRegistration_RegistrationOpenState_ShouldChangeStatusToClosed()
    {
        // Arrange
        var competition = new Competition
        {
            Id = Guid.NewGuid(),
            Status = CompetitionStatus.RegistrationOpen
        };

        // Act
        competition.CloseRegistration();

        // Assert
        competition.Status.Should().Be(CompetitionStatus.Closed);
        competition.LastUpdateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MoveToResultsDraft_ClosedState_ShouldChangeStatusToResultsDraft()
    {
        // Arrange
        var competition = new Competition
        {
            Id = Guid.NewGuid(),
            Status = CompetitionStatus.Closed
        };

        // Act
        competition.MoveToResultsDraft();

        // Assert
        competition.Status.Should().Be(CompetitionStatus.ResultsDraft);
    }

    [Fact]
    public void ValidateResults_ResultsDraftState_ShouldChangeStatusToResultsValidated()
    {
        // Arrange
        var competition = new Competition
        {
            Id = Guid.NewGuid(),
            Status = CompetitionStatus.ResultsDraft
        };

        // Act
        competition.ValidateResults();

        // Assert
        competition.Status.Should().Be(CompetitionStatus.ResultsValidated);
    }

    [Fact]
    public void ReopenRegistration_ClosedStateWithin30Days_ShouldReturnTrueAndSetStatusToRegistrationOpen()
    {
        // Arrange
        var competition = new Competition
        {
            Id = Guid.NewGuid(),
            Status = CompetitionStatus.Closed,
            LastUpdateUtc = DateTime.UtcNow.AddDays(-15) // within 30 days
        };

        // Act
        var result = competition.ReopenRegistration();

        // Assert
        result.Should().BeTrue();
        competition.Status.Should().Be(CompetitionStatus.RegistrationOpen);
        competition.LastUpdateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ReopenRegistration_ClosedStateOlderThan30Days_ShouldReturnFalseAndLeaveStatusAsClosed()
    {
        // Arrange
        var competition = new Competition
        {
            Id = Guid.NewGuid(),
            Status = CompetitionStatus.Closed,
            LastUpdateUtc = DateTime.UtcNow.AddDays(-31) // older than 30 days
        };

        // Act
        var result = competition.ReopenRegistration();

        // Assert
        result.Should().BeFalse();
        competition.Status.Should().Be(CompetitionStatus.Closed);
        competition.LastUpdateUtc.Should().BeCloseTo(DateTime.UtcNow.AddDays(-31), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void SetBiggestCatchMinWeight_ValidWeight_ShouldUpdateWeightAndSetLastUpdateUtc()
    {
        // Arrange
        var competition = new Competition
        {
            Id = Guid.NewGuid(),
            BiggestCatchMinWeightInGrams = null,
            LastUpdateUtc = DateTime.UtcNow.AddMinutes(-10)
        };
        var initialUpdate = competition.LastUpdateUtc;

        // Act
        competition.SetBiggestCatchMinWeight(1500);

        // Assert
        competition.BiggestCatchMinWeightInGrams.Should().Be(1500);
        competition.LastUpdateUtc.Should().BeAfter(initialUpdate);
    }
}
