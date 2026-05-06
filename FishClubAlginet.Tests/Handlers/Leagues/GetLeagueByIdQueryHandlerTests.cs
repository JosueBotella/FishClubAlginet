using FluentAssertions;
using FishClubAlginet.Application.Features.Leagues;

namespace FishClubAlginet.Tests.Handlers.Leagues;

public class GetLeagueByIdQueryHandlerTests
{
    private readonly Mock<IGenericRepository<League, Guid>> _mockRepository;
    private readonly GetLeagueByIdQueryHandler _handler;

    public GetLeagueByIdQueryHandlerTests()
    {
        _mockRepository = new Mock<IGenericRepository<League, Guid>>();
        _handler = new GetLeagueByIdQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_LeagueExists_ShouldReturnDto()
    {
        // Arrange
        var league = League.Create("Liga 2026", 2026, 5, 2);
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League> { league }.AsQueryable());

        // Act
        var result = await _handler.Handle(new GetLeagueByIdQuery(league.Id), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Id.Should().Be(league.Id);
        result.Value.Name.Should().Be("Liga 2026");
        result.Value.Year.Should().Be(2026);
        result.Value.MinPoints.Should().Be(5);
        result.Value.WorstResultsToDiscard.Should().Be(2);
    }

    [Fact]
    public async Task Handle_LeagueNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League>().AsQueryable());

        // Act
        var result = await _handler.Handle(new GetLeagueByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.NotFound");
    }

    [Fact]
    public async Task Handle_LeagueSoftDeleted_ShouldReturnNotFound()
    {
        // Arrange
        var league = League.Create("Liga borrada", 2020);
        league.IsDeleted = true;
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League> { league }.AsQueryable());

        // Act
        var result = await _handler.Handle(new GetLeagueByIdQuery(league.Id), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.NotFound");
    }
}
