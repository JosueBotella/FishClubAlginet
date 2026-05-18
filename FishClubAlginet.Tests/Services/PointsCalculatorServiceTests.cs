using FishClubAlginet.Application.Services;

namespace FishClubAlginet.Tests.Services;

public class PointsCalculatorServiceTests
{
    private readonly PointsCalculatorService _sut = new();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static CompetitionResult MakeAttendee(int weightInGrams)
    {
        var r = CompetitionResult.Register(Guid.NewGuid(), 1);
        r.RecordResult(didAttend: true, weightInGrams, biggestCatchWeight: null);
        return r;
    }

    private static CompetitionResult MakeNonAttendee()
    {
        var r = CompetitionResult.Register(Guid.NewGuid(), 2);
        r.RecordResult(didAttend: false, 0, biggestCatchWeight: null);
        return r;
    }

    // ── Normal ranking (no ties) ──────────────────────────────────────────────

    [Fact]
    public void CalculateAndAssign_NormalRanking_AssignsDescendingPoints()
    {
        // N = 4, minPoints = 1
        // Rank 1 → 4 pts, Rank 2 → 3 pts, Rank 3 → 2 pts, Rank 4 → 1 pt
        var a = MakeAttendee(5000);
        var b = MakeAttendee(3000);
        var c = MakeAttendee(1500);
        var d = MakeAttendee(500);

        _sut.CalculateAndAssign([a, b, c, d], minPoints: 1);

        Assert.Equal(4m, a.Points);
        Assert.Equal(3m, b.Points);
        Assert.Equal(2m, c.Points);
        Assert.Equal(1m, d.Points);

        Assert.Equal(1, a.Ranking);
        Assert.Equal(2, b.Ranking);
        Assert.Equal(3, c.Ranking);
        Assert.Equal(4, d.Ranking);
    }

    // ── Ties ─────────────────────────────────────────────────────────────────

    [Fact]
    public void CalculateAndAssign_TwoWayTie_AssignsAveragePoints()
    {
        // N = 4, minPoints = 1
        // A: 5000 → rank 1 → 4 pts
        // B: 3000 → rank 2 (tied) → avg(3+2)/2 = 2.5 pts
        // C: 3000 → rank 2 (tied) → 2.5 pts
        // D:  500 → rank 4       → 1 pt
        var a = MakeAttendee(5000);
        var b = MakeAttendee(3000);
        var c = MakeAttendee(3000);
        var d = MakeAttendee(500);

        _sut.CalculateAndAssign([a, b, c, d], minPoints: 1);

        Assert.Equal(4m,   a.Points);
        Assert.Equal(2.5m, b.Points);
        Assert.Equal(2.5m, c.Points);
        Assert.Equal(1m,   d.Points);

        Assert.Equal(1, a.Ranking);
        Assert.Equal(2, b.Ranking); // both share rank 2
        Assert.Equal(2, c.Ranking);
        Assert.Equal(4, d.Ranking);
    }

    [Fact]
    public void CalculateAndAssign_AllTied_AssignsAverageOfAllPositions()
    {
        // N = 3, all 5000g, minPoints = 1
        // Avg(3+2+1)/3 = 2
        var a = MakeAttendee(5000);
        var b = MakeAttendee(5000);
        var c = MakeAttendee(5000);

        _sut.CalculateAndAssign([a, b, c], minPoints: 1);

        Assert.Equal(2m, a.Points);
        Assert.Equal(2m, b.Points);
        Assert.Equal(2m, c.Points);
        Assert.All([a, b, c], r => Assert.Equal(1, r.Ranking));
    }

    // ── MinPoints floor ───────────────────────────────────────────────────────

    [Fact]
    public void CalculateAndAssign_MinPointsHigherThanCalculated_AppliesFloor()
    {
        // N = 3, minPoints = 5
        // Calculated: A→3, B→2, C→1  →  all floored to 5
        var a = MakeAttendee(5000);
        var b = MakeAttendee(3000);
        var c = MakeAttendee(1000);

        _sut.CalculateAndAssign([a, b, c], minPoints: 5);

        Assert.Equal(5m, a.Points);
        Assert.Equal(5m, b.Points);
        Assert.Equal(5m, c.Points);
    }

    [Fact]
    public void CalculateAndAssign_AttendeeWithZeroWeight_GetsMinPoints()
    {
        // N = 3, minPoints = 5
        // A: 5000 → rank 1 → max(3,5) = 5
        // B: 2000 → rank 2 → max(2,5) = 5
        // C:    0 → rank 3 → max(1,5) = 5
        var a = MakeAttendee(5000);
        var b = MakeAttendee(2000);
        var c = MakeAttendee(0);   // attended but no catch

        _sut.CalculateAndAssign([a, b, c], minPoints: 5);

        Assert.Equal(5m, a.Points);
        Assert.Equal(5m, b.Points);
        Assert.Equal(5m, c.Points);
        Assert.Equal(3, c.Ranking);
    }

    // ── Non-attendees ─────────────────────────────────────────────────────────

    [Fact]
    public void CalculateAndAssign_NonAttendees_GetZeroPointsAndZeroRank()
    {
        var a = MakeAttendee(5000);
        var b = MakeAttendee(2000);
        var absent = MakeNonAttendee();

        _sut.CalculateAndAssign([a, b, absent], minPoints: 5);

        Assert.Equal(0m, absent.Points);
        Assert.Equal(0,  absent.Ranking);
    }

    [Fact]
    public void CalculateAndAssign_NonAttendeesDoNotAffectN()
    {
        // N = 2 attendees (not 3 total).
        // A: 5000 → rank 1 → 2 pts (N=2)
        // B: 2000 → rank 2 → 1 pt
        var a = MakeAttendee(5000);
        var b = MakeAttendee(2000);
        var absent = MakeNonAttendee();

        _sut.CalculateAndAssign([a, b, absent], minPoints: 1);

        Assert.Equal(2m, a.Points);
        Assert.Equal(1m, b.Points);
        Assert.Equal(0m, absent.Points);
    }

    // ── Edge cases ────────────────────────────────────────────────────────────

    [Fact]
    public void CalculateAndAssign_EmptyList_DoesNotThrow()
    {
        var ex = Record.Exception(() => _sut.CalculateAndAssign([], minPoints: 5));
        Assert.Null(ex);
    }

    [Fact]
    public void CalculateAndAssign_SingleAttendee_GetsMinPointsFloor()
    {
        // N = 1 → points = 1 → floor to minPoints = 5
        var a = MakeAttendee(3000);

        _sut.CalculateAndAssign([a], minPoints: 5);

        Assert.Equal(5m, a.Points);
        Assert.Equal(1,  a.Ranking);
    }

    [Fact]
    public void CalculateAndAssign_SingleAttendee_NoMinPointsFloor()
    {
        // N = 1, minPoints = 1 → points = 1 (no floor needed)
        var a = MakeAttendee(3000);

        _sut.CalculateAndAssign([a], minPoints: 1);

        Assert.Equal(1m, a.Points);
    }
}
