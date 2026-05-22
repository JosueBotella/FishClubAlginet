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
    public void CalculateAndAssign_NormalRanking_AssignsFixedScalePoints()
    {
        // minPoints = 5, positions 1-4
        // Rank 1 → 5 + 20 = 25 pts
        // Rank 2 → 5 + 19 = 24 pts
        // Rank 3 → 5 + 18 = 23 pts
        // Rank 4 → 5 + 17 = 22 pts
        var a = MakeAttendee(5000);
        var b = MakeAttendee(3000);
        var c = MakeAttendee(1500);
        var d = MakeAttendee(500);

        _sut.CalculateAndAssign([a, b, c, d], minPoints: 5);

        Assert.Equal(25m, a.Points);
        Assert.Equal(24m, b.Points);
        Assert.Equal(23m, c.Points);
        Assert.Equal(22m, d.Points);

        Assert.Equal(1, a.Ranking);
        Assert.Equal(2, b.Ranking);
        Assert.Equal(3, c.Ranking);
        Assert.Equal(4, d.Ranking);
    }

    [Fact]
    public void CalculateAndAssign_PositionsBeyond20_GetZeroRankingBonus()
    {
        // 21 attendees; position 21 gets 5 + 0 = 5 pts
        var attendees = Enumerable.Range(1, 21)
            .Select(i => MakeAttendee(10000 - i * 100))
            .ToList();

        _sut.CalculateAndAssign(attendees, minPoints: 5);

        Assert.Equal(5m + 20m, attendees[0].Points);  // rank 1
        Assert.Equal(5m + 1m,  attendees[19].Points); // rank 20
        Assert.Equal(5m,       attendees[20].Points); // rank 21 → no ranking bonus
    }

    // ── Ties ─────────────────────────────────────────────────────────────────

    [Fact]
    public void CalculateAndAssign_TwoWayTie_EachGetsTieBonusHalf()
    {
        // Positions 2 and 3 tie:
        // Rank 2 → 5 + 19 + 0.5 = 24.5
        // Rank 3 → 5 + 18 + 0.5 = 23.5
        var a = MakeAttendee(5000);
        var b = MakeAttendee(3000);
        var c = MakeAttendee(3000);
        var d = MakeAttendee(500);

        _sut.CalculateAndAssign([a, b, c, d], minPoints: 5);

        Assert.Equal(25m,   a.Points); // rank 1, no tie
        Assert.Equal(24.5m, b.Points); // rank 2, tied
        Assert.Equal(23.5m, c.Points); // rank 3, tied
        Assert.Equal(22m,   d.Points); // rank 4, no tie

        Assert.Equal(1, a.Ranking);
        Assert.Equal(2, b.Ranking); // both share rank 2
        Assert.Equal(2, c.Ranking);
        Assert.Equal(4, d.Ranking);
    }

    [Fact]
    public void CalculateAndAssign_ThreeWayTie_EachKeepsPositionBonusPlusOneThird()
    {
        // Positions 1, 2, 3 tie. Each keeps its position's ranking bonus + 1/3 shared:
        // Sorted position 1 → 5 + 20 + 1/3
        // Sorted position 2 → 5 + 19 + 1/3
        // Sorted position 3 → 5 + 18 + 1/3
        // All assigned Ranking = 1
        var a = MakeAttendee(5000);
        var b = MakeAttendee(5000);
        var c = MakeAttendee(5000);

        var all = new List<CompetitionResult> { a, b, c };
        _sut.CalculateAndAssign(all, minPoints: 5);

        var bonus = 1m / 3m;
        var posPoints = all.Select(r => r.Points).OrderByDescending(p => p).ToList();
        Assert.Equal(5m + 20m + bonus, posPoints[0]);
        Assert.Equal(5m + 19m + bonus, posPoints[1]);
        Assert.Equal(5m + 18m + bonus, posPoints[2]);
        Assert.All(all, r => Assert.Equal(1, r.Ranking));
    }

    [Fact]
    public void CalculateAndAssign_TieStraddlingPosition20_AppliesTieBonus()
    {
        // Positions 20 and 21 tie (group starts at 20 which is within top 20):
        // Rank 20 → 5 + 1 + 0.5 = 6.5
        // Rank 21 → 5 + 0 + 0.5 = 5.5
        var attendees = Enumerable.Range(1, 19)
            .Select(i => MakeAttendee(10000 - i * 100))
            .ToList();
        attendees.Add(MakeAttendee(100)); // position 20
        attendees.Add(MakeAttendee(100)); // position 21 (tied with 20)

        _sut.CalculateAndAssign(attendees, minPoints: 5);

        Assert.Equal(6.5m, attendees[19].Points); // rank 20, tied
        Assert.Equal(5.5m, attendees[20].Points); // rank 21, tied
        Assert.Equal(20, attendees[19].Ranking);
        Assert.Equal(20, attendees[20].Ranking);
    }

    [Fact]
    public void CalculateAndAssign_TieBeyondPosition20_NoTieBonus()
    {
        // Two people tied at positions 21 and 22: both get only minPoints
        var attendees = Enumerable.Range(1, 20)
            .Select(i => MakeAttendee(10000 - i * 100))
            .ToList();
        attendees.Add(MakeAttendee(50)); // position 21
        attendees.Add(MakeAttendee(50)); // position 22 (tied with 21, but beyond top 20)

        _sut.CalculateAndAssign(attendees, minPoints: 5);

        Assert.Equal(5m, attendees[20].Points);
        Assert.Equal(5m, attendees[21].Points);
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
    public void CalculateAndAssign_NonAttendeesDoNotAffectRanking()
    {
        // N = 2 attendees; rank 1 → 5+20=25, rank 2 → 5+19=24
        var a = MakeAttendee(5000);
        var b = MakeAttendee(2000);
        var absent = MakeNonAttendee();

        _sut.CalculateAndAssign([a, b, absent], minPoints: 5);

        Assert.Equal(25m, a.Points);
        Assert.Equal(24m, b.Points);
        Assert.Equal(0m,  absent.Points);
    }

    // ── Edge cases ────────────────────────────────────────────────────────────

    [Fact]
    public void CalculateAndAssign_EmptyList_DoesNotThrow()
    {
        var ex = Record.Exception(() => _sut.CalculateAndAssign([], minPoints: 5));
        Assert.Null(ex);
    }

    [Fact]
    public void CalculateAndAssign_SingleAttendee_GetsMinPlusTwenty()
    {
        var a = MakeAttendee(3000);

        _sut.CalculateAndAssign([a], minPoints: 5);

        Assert.Equal(25m, a.Points);
        Assert.Equal(1,   a.Ranking);
    }

    [Fact]
    public void CalculateAndAssign_AttendeeWithZeroWeight_GetsPointsBasedOnRank()
    {
        var a = MakeAttendee(5000);
        var b = MakeAttendee(2000);
        var c = MakeAttendee(0); // attended but no catch → rank 3

        _sut.CalculateAndAssign([a, b, c], minPoints: 5);

        Assert.Equal(25m, a.Points); // 5 + 20
        Assert.Equal(24m, b.Points); // 5 + 19
        Assert.Equal(23m, c.Points); // 5 + 18 (rank 3)
        Assert.Equal(3, c.Ranking);
    }
}
