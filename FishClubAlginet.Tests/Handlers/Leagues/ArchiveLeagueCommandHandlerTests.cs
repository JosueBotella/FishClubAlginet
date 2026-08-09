using FluentAssertions;
using FishClubAlginet.Application.Features.Leagues;
using FishClubAlginet.Contracts.Dtos.Responses.Competition;
using MediatR;

namespace FishClubAlginet.Tests.Handlers.Leagues;

public class ArchiveLeagueCommandHandlerTests
{
    private readonly Mock<IGenericRepository<League, Guid>> _mockRepository;
    private readonly Mock<IGenericRepository<LeagueSeasonSnapshot, Guid>> _mockSnapshotRepository;
    private readonly Mock<ISender> _mockSender;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<ArchiveLeagueCommandHandler>> _mockLogger;
    private readonly ArchiveLeagueCommandHandler _handler;

    public ArchiveLeagueCommandHandlerTests()
    {
        _mockRepository = new Mock<IGenericRepository<League, Guid>>();
        _mockSnapshotRepository = new Mock<IGenericRepository<LeagueSeasonSnapshot, Guid>>();
        _mockSender = new Mock<ISender>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<ArchiveLeagueCommandHandler>>();

        _handler = new ArchiveLeagueCommandHandler(
            _mockRepository.Object,
            _mockSnapshotRepository.Object,
            _mockSender.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ArchiveActiveLeague_ShouldArchiveAndCreateSnapshot()
    {
        // Arrange
        var league = League.Create("Liga 2024", 2024);
        league.Activate();
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League> { league }.AsQueryable());
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        var matrixDto = new LeagueStandingsMatrixDto(
            league.Id,
            league.Name,
            league.Year,
            league.WorstResultsToDiscard,
            new List<CompetitionHeaderDto>(),
            new List<FishermanMatrixRowDto>(),
            new List<FishermanMatrixRowDto>());

        _mockSender.Setup(s => s.Send(It.IsAny<GetLeagueStandingsMatrixQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(matrixDto);

        var command = new ArchiveLeagueCommand(league.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.IsArchived.Should().BeTrue();
        result.Value.IsActive.Should().BeFalse();
        league.IsArchived.Should().BeTrue();
        league.IsActive.Should().BeFalse();

        _mockSnapshotRepository.Verify(s => s.AddAsync(It.Is<LeagueSeasonSnapshot>(
            snap => snap.LeagueId == league.Id && snap.Year == league.Year)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_LeagueNotFound_ShouldReturnNotFoundAndNotPersist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League>().AsQueryable());

        var command = new ArchiveLeagueCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.NotFound");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_LeagueAlreadyArchived_ShouldReturnAlreadyArchived()
    {
        // Arrange
        var league = League.Create("Liga 2023", 2023);
        league.Archive();
        _mockRepository.Setup(r => r.GetAll())
            .Returns(new List<League> { league }.AsQueryable());

        var command = new ArchiveLeagueCommand(league.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.AlreadyArchived");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
