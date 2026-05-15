using FluentAssertions;
using FishClubAlginet.Application.Features.Competitions;

namespace FishClubAlginet.Tests.Handlers.Competitions;

public class CreateCompetitionCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Competition, Guid>> _mockCompetitionRepo;
    private readonly Mock<IGenericRepository<League, Guid>>      _mockLeagueRepo;
    private readonly Mock<IUnitOfWork>                           _mockUnitOfWork;
    private readonly Mock<ILogger<CreateCompetitionCommandHandler>> _mockLogger;
    private readonly CreateCompetitionCommandHandler             _handler;

    public CreateCompetitionCommandHandlerTests()
    {
        _mockCompetitionRepo = new Mock<IGenericRepository<Competition, Guid>>();
        _mockLeagueRepo      = new Mock<IGenericRepository<League, Guid>>();
        _mockUnitOfWork      = new Mock<IUnitOfWork>();
        _mockLogger          = new Mock<ILogger<CreateCompetitionCommandHandler>>();

        _handler = new CreateCompetitionCommandHandler(
            _mockCompetitionRepo.Object,
            _mockLeagueRepo.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    // ── fixtures ──────────────────────────────────────────────────────────────

    private static League BuildActiveLeague(Guid? id = null)
    {
        var league = League.Create("Liga 2026", 2026);
        league.Id  = id ?? Guid.NewGuid();
        league.IsActive   = true;
        league.IsArchived = false;
        return league;
    }

    private static CreateCompetitionCommand BuildValidCommand(Guid leagueId) =>
        new CreateCompetitionCommand(
            LeagueId:          leagueId,
            CompetitionNumber: 1,
            Name:              "Concurso 1",
            Date:              DateTime.UtcNow.AddDays(10),
            StartTime:         new TimeSpan(8, 0, 0),
            EndTime:           new TimeSpan(14, 0, 0),
            Venue:             "Acequia de l'Horta",
            Zone:              "Zona A",
            Subspecialty:      Subspecialty.AguaDulce,
            Category:          Category.Seniors,
            MaxSpots:          30);

    // ── happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateCompetitionAndReturnId()
    {
        // Arrange
        var league = BuildActiveLeague();

        _mockLeagueRepo.Setup(r => r.GetById(league.Id))
            .ReturnsAsync(league);
        _mockCompetitionRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition>().AsQueryable());
        _mockCompetitionRepo.Setup(r => r.AddAsync(It.IsAny<Competition>()))
            .ReturnsAsync((Competition c) => c);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        var command = BuildValidCommand(league.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeEmpty();
        _mockCompetitionRepo.Verify(r => r.AddAsync(It.Is<Competition>(c =>
            c.LeagueId          == league.Id
            && c.CompetitionNumber == 1
            && c.Status            == CompetitionStatus.Planned
            && c.ParticipantCount  == 0)),
            Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── league guard ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_LeagueNotFound_ShouldReturnLeagueNotFoundError()
    {
        _mockLeagueRepo.Setup(r => r.GetById(It.IsAny<Guid>()))
            .ReturnsAsync((League?)null);

        var result = await _handler.Handle(BuildValidCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.NotFound");
        _mockCompetitionRepo.Verify(r => r.AddAsync(It.IsAny<Competition>()), Times.Never);
    }

    [Fact]
    public async Task Handle_LeagueSoftDeleted_ShouldReturnLeagueNotFoundError()
    {
        var league = BuildActiveLeague();
        league.IsDeleted = true;

        _mockLeagueRepo.Setup(r => r.GetById(league.Id)).ReturnsAsync(league);

        var result = await _handler.Handle(BuildValidCommand(league.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.NotFound");
    }

    [Fact]
    public async Task Handle_ArchivedLeague_ShouldReturnCannotModifyArchivedError()
    {
        var league = BuildActiveLeague();
        league.IsArchived = true;
        league.IsActive   = false;

        _mockLeagueRepo.Setup(r => r.GetById(league.Id)).ReturnsAsync(league);

        var result = await _handler.Handle(BuildValidCommand(league.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.CannotModifyArchived");
    }

    [Fact]
    public async Task Handle_InactiveLeague_ShouldReturnLeagueNotActiveError()
    {
        var league = BuildActiveLeague();
        league.IsActive   = false;
        league.IsArchived = false;

        _mockLeagueRepo.Setup(r => r.GetById(league.Id)).ReturnsAsync(league);

        var result = await _handler.Handle(BuildValidCommand(league.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("League.NotActive");
    }

    // ── duplicate number ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DuplicateCompetitionNumber_ShouldReturnDuplicateNumberError()
    {
        var league = BuildActiveLeague();

        var existing = new Competition
        {
            Id                = Guid.NewGuid(),
            LeagueId          = league.Id,
            CompetitionNumber = 1,
            LastUpdateUtc     = DateTime.UtcNow
        };

        _mockLeagueRepo.Setup(r => r.GetById(league.Id)).ReturnsAsync(league);
        _mockCompetitionRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition> { existing }.AsQueryable());

        var result = await _handler.Handle(BuildValidCommand(league.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.DuplicateNumber");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateNumberOnDeletedCompetition_ShouldAllowCreation()
    {
        var league = BuildActiveLeague();

        var deleted = new Competition
        {
            Id                = Guid.NewGuid(),
            LeagueId          = league.Id,
            CompetitionNumber = 1,
            IsDeleted         = true,
            LastUpdateUtc     = DateTime.UtcNow
        };

        _mockLeagueRepo.Setup(r => r.GetById(league.Id)).ReturnsAsync(league);
        _mockCompetitionRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition> { deleted }.AsQueryable());
        _mockCompetitionRepo.Setup(r => r.AddAsync(It.IsAny<Competition>()))
            .ReturnsAsync((Competition c) => c);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        var result = await _handler.Handle(BuildValidCommand(league.Id), CancellationToken.None);

        result.IsError.Should().BeFalse();
    }

    // ── persistence failures ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_UniqueConstraintOnSave_ShouldMapToDuplicateNumber()
    {
        var league = BuildActiveLeague();

        _mockLeagueRepo.Setup(r => r.GetById(league.Id)).ReturnsAsync(league);
        _mockCompetitionRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition>().AsQueryable());
        _mockCompetitionRepo.Setup(r => r.AddAsync(It.IsAny<Competition>()))
            .ReturnsAsync((Competition c) => c);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Conflict(code: "Database.UniqueConstraintViolation", description: "..."));

        var result = await _handler.Handle(BuildValidCommand(league.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.DuplicateNumber");
    }

    [Fact]
    public async Task Handle_GenericPersistenceFailure_ShouldReturnSaveFailedError()
    {
        var league = BuildActiveLeague();

        _mockLeagueRepo.Setup(r => r.GetById(league.Id)).ReturnsAsync(league);
        _mockCompetitionRepo.Setup(r => r.GetAll())
            .Returns(new List<Competition>().AsQueryable());
        _mockCompetitionRepo.Setup(r => r.AddAsync(It.IsAny<Competition>()))
            .ReturnsAsync((Competition c) => c);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure(code: "Database.SaveFailure", description: "..."));

        var result = await _handler.Handle(BuildValidCommand(league.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("COMPETITION_SAVE_FAILED");
    }
}
