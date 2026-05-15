using FluentAssertions;
using FishClubAlginet.Application.Features.Competitions;

namespace FishClubAlginet.Tests.Handlers.Competitions;

public class RegisterFishermanCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Competition, Guid>>     _mockCompetitionRepo;
    private readonly Mock<IGenericRepository<Fisherman, int>>        _mockFishermanRepo;
    private readonly Mock<IGenericRepository<CompetitionResult, Guid>> _mockResultRepo;
    private readonly Mock<IUnitOfWork>                               _mockUnitOfWork;
    private readonly Mock<ILogger<RegisterFishermanCommandHandler>>  _mockLogger;
    private readonly RegisterFishermanCommandHandler                 _handler;

    public RegisterFishermanCommandHandlerTests()
    {
        _mockCompetitionRepo = new Mock<IGenericRepository<Competition, Guid>>();
        _mockFishermanRepo   = new Mock<IGenericRepository<Fisherman, int>>();
        _mockResultRepo      = new Mock<IGenericRepository<CompetitionResult, Guid>>();
        _mockUnitOfWork      = new Mock<IUnitOfWork>();
        _mockLogger          = new Mock<ILogger<RegisterFishermanCommandHandler>>();

        _handler = new RegisterFishermanCommandHandler(
            _mockCompetitionRepo.Object,
            _mockFishermanRepo.Object,
            _mockResultRepo.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    // ── fixtures ──────────────────────────────────────────────────────────────

    private static Competition BuildOpenCompetition(Guid? id = null, int maxSpots = 30, int current = 0) =>
        new Competition
        {
            Id               = id ?? Guid.NewGuid(),
            LeagueId         = Guid.NewGuid(),
            CompetitionNumber = 1,
            Status           = CompetitionStatus.RegistrationOpen,
            MaxSpots         = maxSpots,
            ParticipantCount = current,
            LastUpdateUtc    = DateTime.UtcNow
        };

    private static Fisherman BuildFisherman(int id = 1) =>
        Fisherman.Create(
            firstName:         "Pepe",
            lastName:          "García",
            dateOfBirth:       DateTime.UtcNow.AddYears(-30),
            documentType:      TypeNationalIdentifier.Dni,
            documentNumber:    "12345678Z",
            federationLicense: "FED-001",
            address:           new Address());

    // ── happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidCommand_ShouldRegisterFishermanAndReturnResultId()
    {
        // Arrange
        var competition = BuildOpenCompetition();
        var fisherman   = BuildFisherman();
        fisherman.Id    = 1;
        var command     = new RegisterFishermanCommand(competition.Id, fisherman.Id);

        _mockCompetitionRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);
        _mockFishermanRepo.Setup(r => r.GetById(fisherman.Id)).ReturnsAsync(fisherman);
        _mockResultRepo.Setup(r => r.GetAll()).Returns(new List<CompetitionResult>().AsQueryable());
        _mockResultRepo.Setup(r => r.AddAsync(It.IsAny<CompetitionResult>()))
            .ReturnsAsync((CompetitionResult cr) => cr);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeEmpty();
        _mockResultRepo.Verify(r => r.AddAsync(It.Is<CompetitionResult>(cr =>
            cr.CompetitionId == competition.Id && cr.FishermanId == fisherman.Id)), Times.Once);
        competition.ParticipantCount.Should().Be(1);
    }

    // ── competition guard ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CompetitionNotFound_ShouldReturnCompetitionNotFoundError()
    {
        _mockCompetitionRepo.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Competition?)null);

        var result = await _handler.Handle(new RegisterFishermanCommand(Guid.NewGuid(), 1), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.NotFound");
    }

    [Fact]
    public async Task Handle_CompetitionNotOpen_ShouldReturnRegistrationNotOpenError()
    {
        var competition = BuildOpenCompetition();
        competition.Status = CompetitionStatus.Planned; // not open

        _mockCompetitionRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);

        var result = await _handler.Handle(new RegisterFishermanCommand(competition.Id, 1), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.RegistrationNotOpen");
    }

    [Fact]
    public async Task Handle_MaxSpotsReached_ShouldReturnMaxSpotsReachedError()
    {
        // maxSpots == current participants → full
        var competition = BuildOpenCompetition(maxSpots: 2, current: 2);

        _mockCompetitionRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);

        var result = await _handler.Handle(new RegisterFishermanCommand(competition.Id, 1), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.MaxSpotsReached");
    }

    // ── fisherman guard ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_FishermanNotFound_ShouldReturnFishermanNotFoundError()
    {
        var competition = BuildOpenCompetition();

        _mockCompetitionRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);
        _mockFishermanRepo.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync((Fisherman?)null);

        var result = await _handler.Handle(new RegisterFishermanCommand(competition.Id, 99), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Fisherman.NotFound");
    }

    // ── duplicate registration ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_AlreadyRegistered_ShouldReturnAlreadyRegisteredError()
    {
        var competition = BuildOpenCompetition();
        var fisherman   = BuildFisherman();
        fisherman.Id    = 1;

        var existingResult = CompetitionResult.Register(competition.Id, fisherman.Id);

        _mockCompetitionRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);
        _mockFishermanRepo.Setup(r => r.GetById(fisherman.Id)).ReturnsAsync(fisherman);
        _mockResultRepo.Setup(r => r.GetAll())
            .Returns(new List<CompetitionResult> { existingResult }.AsQueryable());

        var result = await _handler.Handle(new RegisterFishermanCommand(competition.Id, fisherman.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.AlreadyRegistered");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── persistence failures ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_UniqueConstraintOnSave_ShouldMapToAlreadyRegistered()
    {
        var competition = BuildOpenCompetition();
        var fisherman   = BuildFisherman();
        fisherman.Id    = 1;

        _mockCompetitionRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);
        _mockFishermanRepo.Setup(r => r.GetById(fisherman.Id)).ReturnsAsync(fisherman);
        _mockResultRepo.Setup(r => r.GetAll()).Returns(new List<CompetitionResult>().AsQueryable());
        _mockResultRepo.Setup(r => r.AddAsync(It.IsAny<CompetitionResult>()))
            .ReturnsAsync((CompetitionResult cr) => cr);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Conflict(code: "Database.UniqueConstraintViolation", description: "..."));

        var result = await _handler.Handle(new RegisterFishermanCommand(competition.Id, fisherman.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.AlreadyRegistered");
    }

    [Fact]
    public async Task Handle_GenericPersistenceFailure_ShouldReturnRegistrationSaveFailedError()
    {
        var competition = BuildOpenCompetition();
        var fisherman   = BuildFisherman();
        fisherman.Id    = 1;

        _mockCompetitionRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);
        _mockFishermanRepo.Setup(r => r.GetById(fisherman.Id)).ReturnsAsync(fisherman);
        _mockResultRepo.Setup(r => r.GetAll()).Returns(new List<CompetitionResult>().AsQueryable());
        _mockResultRepo.Setup(r => r.AddAsync(It.IsAny<CompetitionResult>()))
            .ReturnsAsync((CompetitionResult cr) => cr);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure(code: "Database.SaveFailure", description: "..."));

        var result = await _handler.Handle(new RegisterFishermanCommand(competition.Id, fisherman.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("REGISTRATION_SAVE_FAILED");
    }
}
