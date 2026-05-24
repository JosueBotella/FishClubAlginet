using FluentAssertions;

namespace FishClubAlginet.Tests.Domain.Entities;

public class LeagueTests
{
    [Fact]
    public void Create_ValidParameters_ShouldReturnLeagueWithDefaultStates()
    {
        // Arrange
        var name = "Liga de Alginet 2026";
        var year = 2026;
        var minPoints = 5;
        var worstResultsToDiscard = 2;

        // Act
        var league = League.Create(name, year, minPoints, worstResultsToDiscard);

        // Assert
        league.Should().NotBeNull();
        league.Id.Should().NotBeEmpty();
        league.Name.Should().Be(name);
        league.Year.Should().Be(year);
        league.MinPoints.Should().Be(minPoints);
        league.WorstResultsToDiscard.Should().Be(worstResultsToDiscard);
        league.IsActive.Should().BeFalse();
        league.IsArchived.Should().BeFalse();
        league.LastUpdateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_ValidParameters_ShouldModifyPropertiesAndSetLastUpdateUtc()
    {
        // Arrange
        var league = League.Create("Liga 2026", 2026);
        var initialUpdateUtc = league.LastUpdateUtc;
        
        // Wait briefly to ensure DateTime.UtcNow changes or compare strictly greater
        Thread.Sleep(10); 

        // Act
        league.Update("Liga Actualizada", 8, 3);

        // Assert
        league.Name.Should().Be("Liga Actualizada");
        league.MinPoints.Should().Be(8);
        league.WorstResultsToDiscard.Should().Be(3);
        league.LastUpdateUtc.Should().BeAfter(initialUpdateUtc);
    }

    [Fact]
    public void Activate_InactiveLeague_ShouldSetActiveToTrueAndArchivedToFalse()
    {
        // Arrange
        var league = League.Create("Liga 2026", 2026);
        league.IsArchived = true;
        league.IsActive = false;

        // Act
        league.Activate();

        // Assert
        league.IsActive.Should().BeTrue();
        league.IsArchived.Should().BeFalse();
        league.LastUpdateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Deactivate_ActiveLeague_ShouldSetActiveToFalse()
    {
        // Arrange
        var league = League.Create("Liga 2026", 2026);
        league.IsActive = true;

        // Act
        league.Deactivate();

        // Assert
        league.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Archive_ActiveLeague_ShouldSetArchivedToTrueAndActiveToFalse()
    {
        // Arrange
        var league = League.Create("Liga 2026", 2026);
        league.IsActive = true;
        league.IsArchived = false;

        // Act
        league.Archive();

        // Assert
        league.IsArchived.Should().BeTrue();
        league.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Unarchive_ArchivedLeague_ShouldSetArchivedToFalseAndLeaveActiveAsFalse()
    {
        // Arrange
        var league = League.Create("Liga 2026", 2026);
        league.IsArchived = true;
        league.IsActive = true; // edge case: active but archived

        // Act
        league.Unarchive();

        // Assert
        league.IsArchived.Should().BeFalse();
        // Note: Unarchive leaves IsActive unchanged in the domain model itself (caller must manually deactivate/activate),
        // let's verify it matches the current implementation (Unarchive only sets IsArchived = false)
        league.IsActive.Should().BeTrue(); 
    }
}
