using FluentAssertions;
using FishClubAlginet.Application.Features.Leagues;

namespace FishClubAlginet.Tests.Handlers.Leagues;

public class GetAllLeaguesQueryHandlerTests
{
    private readonly Mock<IGenericRepository<League, Guid>> _mockRepository;
    private readonly GetAllLeaguesQueryHandler _handler;

    public GetAllLeaguesQueryHandlerTests()
    {
        _mockRepository = new Mock<IGenericRepository<League, Guid>>();
        _handler = new GetAllLeaguesQueryHandler(_mockRepository.Object);
    }

    private static List<League> Sample()
    {
        return new List<League>
        {
            League.Create("Liga 2024", 2024),
            League.Create("Liga 2025", 2025),
            League.Create("Liga 2026", 2026),
        };
    }

    [Fact]
    public async Task Handle_WithoutFilter_ShouldReturnAllOrderedByYearDescending()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAll())
            .Returns(Sample().AsQueryable());

        // Act
        var result = await _handler.Handle(new GetAllLeaguesQuery(0, 10), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TotalCount.Should().Be(3);
        result.Value.Items.Should().HaveCount(3);
        result.Value.Items.Select(l => l.Year).Should().ContainInOrder(2026, 2025, 2024);
    }

    [Fact]
    public async Task Handle_WithYearFilter_ShouldReturnOnlyMatching()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAll())
            .Returns(Sample().AsQueryable());

        // Act
        var result = await _handler.Handle(new GetAllLeaguesQuery(0, 10, 2025), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items.Single().Year.Should().Be(2025);
    }

    [Fact]
    public async Task Handle_ShouldExcludeSoftDeletedLeagues()
    {
        // Arrange
        var leagues = Sample();
        leagues[0].IsDeleted = true; // Liga 2024 borrada
        _mockRepository.Setup(r => r.GetAll())
            .Returns(leagues.AsQueryable());

        // Act
        var result = await _handler.Handle(new GetAllLeaguesQuery(0, 10), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TotalCount.Should().Be(2);
        result.Value.Items.Should().NotContain(l => l.Year == 2024);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldRespectSkipAndTake()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAll())
            .Returns(Sample().AsQueryable());

        // Act: skip 1 take 1 → la 2ª por año desc (2025)
        var result = await _handler.Handle(new GetAllLeaguesQuery(1, 1), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TotalCount.Should().Be(3);
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.Single().Year.Should().Be(2025);
    }
}
