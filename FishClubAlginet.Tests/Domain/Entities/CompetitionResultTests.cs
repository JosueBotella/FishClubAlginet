using FluentAssertions;

namespace FishClubAlginet.Tests.Domain.Entities;

public class CompetitionResultTests
{
    [Fact]
    public void Register_ValidParameters_ShouldCreateResultWithDefaultStates()
    {
        // Arrange
        var competitionId = Guid.NewGuid();
        var fishermanId = 15;

        // Act
        var result = CompetitionResult.Register(competitionId, fishermanId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.CompetitionId.Should().Be(competitionId);
        result.FishermanId.Should().Be(fishermanId);
        result.IsValidated.Should().BeFalse();
        result.AssignedSpotNumber.Should().BeNull();
        result.RegistrationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AssignSpot_ValidSpotNumber_ShouldUpdateSpotAndSetLastUpdateUtc()
    {
        // Arrange
        var result = CompetitionResult.Register(Guid.NewGuid(), 1);

        // Act
        result.AssignSpot(8);

        // Assert
        result.AssignedSpotNumber.Should().Be(8);
        result.LastUpdateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RecordResult_DidAttendTrue_ShouldPopulateResults()
    {
        // Arrange
        var result = CompetitionResult.Register(Guid.NewGuid(), 1);

        // Act
        result.RecordResult(didAttend: true, weightInGrams: 1450, biggestCatchWeight: 890);

        // Assert
        result.DidAttend.Should().BeTrue();
        result.WeightInGrams.Should().Be(1450);
        result.BiggestCatchWeight.Should().Be(890);
        result.Points.Should().Be(0);
        result.Ranking.Should().Be(0);
    }

    [Fact]
    public void RecordResult_DidAttendFalse_ShouldResetGramsAndBiggestCatchToNull()
    {
        // Arrange
        var result = CompetitionResult.Register(Guid.NewGuid(), 1);
        result.WeightInGrams = 2000;
        result.BiggestCatchWeight = 500;

        // Act
        result.RecordResult(didAttend: false, weightInGrams: 1450, biggestCatchWeight: 890);

        // Assert
        result.DidAttend.Should().BeFalse();
        result.WeightInGrams.Should().Be(0);
        result.BiggestCatchWeight.Should().BeNull();
        result.Points.Should().Be(0);
        result.Ranking.Should().Be(0);
    }

    [Fact]
    public void SetCalculatedPoints_ValidPointsAndRanking_ShouldUpdateProperties()
    {
        // Arrange
        var result = CompetitionResult.Register(Guid.NewGuid(), 1);

        // Act
        result.SetCalculatedPoints(24.5m, 2);

        // Assert
        result.Points.Should().Be(24.5m);
        result.Ranking.Should().Be(2);
        result.LastUpdateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
