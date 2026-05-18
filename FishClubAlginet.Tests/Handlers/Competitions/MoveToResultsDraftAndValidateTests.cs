using FluentAssertions;
using FishClubAlginet.Application.Features.Competitions;
using FishClubAlginet.Core.Domain.Services;

namespace FishClubAlginet.Tests.Handlers.Competitions;

public class MoveToResultsDraftCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Competition, Guid>> _mockRepo;
    private readonly Mock<IGenericRepository<CompetitionResult, Guid>> _mockResultRepo;
    private readonly Mock<IGenericRepository<League, Guid>> _mockLeagueRepo;
    private readonly Mock<IPointsCalculator> _mockCalculator;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly MoveToResultsDraftCommandHandler _handler;

    public MoveToResultsDraftCommandHandlerTests()
    {
        _mockRepo        = new Mock<IGenericRepository<Competition, Guid>>();
        _mockResultRepo  = new Mock<IGenericRepository<CompetitionResult, Guid>>();
        _mockLeagueRepo  = new Mock<IGenericRepository<League, Guid>>();
        _mockCalculator  = new Mock<IPointsCalculator>();
        _mockUnitOfWork  = new Mock<IUnitOfWork>();

        // Default: empty results list
        _mockResultRepo.Setup(r => r.GetAll())
            .Returns(new List<CompetitionResult>().AsQueryable());

        _handler = new MoveToResultsDraftCommandHandler(
            _mockRepo.Object,
            _mockResultRepo.Object,
            _mockLeagueRepo.Object,
            _mockCalculator.Object,
            _mockUnitOfWork.Object);
    }

    private static Competition Build(CompetitionStatus status) =>
        new Competition
        {
            Id = Guid.NewGuid(), LeagueId = Guid.NewGuid(),
            CompetitionNumber = 1, Status = status, LastUpdateUtc = DateTime.UtcNow
        };

    [Fact]
    public async Task Handle_ClosedCompetition_ShouldTransitionToResultsDraft()
    {
        var competition = Build(CompetitionStatus.Closed);
        _mockRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);
        _mockLeagueRepo.Setup(r => r.GetById(competition.LeagueId)).ReturnsAsync((League?)null);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync((ErrorOr<int>)1);

        var result = await _handler.Handle(new MoveToResultsDraftCommand(competition.Id), CancellationToken.None);

        result.IsError.Should().BeFalse();
        competition.Status.Should().Be(CompetitionStatus.ResultsDraft);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ClosedCompetition_ShouldInvokePointsCalculator()
    {
        var competition = Build(CompetitionStatus.Closed);
        _mockRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);
        _mockLeagueRepo.Setup(r => r.GetById(competition.LeagueId)).ReturnsAsync((League?)null);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync((ErrorOr<int>)1);

        await _handler.Handle(new MoveToResultsDraftCommand(competition.Id), CancellationToken.None);

        _mockCalculator.Verify(
            c => c.CalculateAndAssign(It.IsAny<IReadOnlyList<CompetitionResult>>(), It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ClosedCompetition_UsesLeagueMinPoints()
    {
        var competition = Build(CompetitionStatus.Closed);
        var league = new League { Id = competition.LeagueId, MinPoints = 7 };
        _mockRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);
        _mockLeagueRepo.Setup(r => r.GetById(competition.LeagueId)).ReturnsAsync(league);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync((ErrorOr<int>)1);

        await _handler.Handle(new MoveToResultsDraftCommand(competition.Id), CancellationToken.None);

        _mockCalculator.Verify(
            c => c.CalculateAndAssign(It.IsAny<IReadOnlyList<CompetitionResult>>(), 7),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LeagueNotFound_FallsBackToDefaultMinPoints()
    {
        var competition = Build(CompetitionStatus.Closed);
        _mockRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);
        _mockLeagueRepo.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((League?)null);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync((ErrorOr<int>)1);

        await _handler.Handle(new MoveToResultsDraftCommand(competition.Id), CancellationToken.None);

        // Falls back to 5 (the default)
        _mockCalculator.Verify(
            c => c.CalculateAndAssign(It.IsAny<IReadOnlyList<CompetitionResult>>(), 5),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnNotFoundError()
    {
        _mockRepo.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Competition?)null);
        var result = await _handler.Handle(new MoveToResultsDraftCommand(Guid.NewGuid()), CancellationToken.None);
        result.FirstError.Code.Should().Be("Competition.NotFound");
    }

    [Theory]
    [InlineData(CompetitionStatus.Planned)]
    [InlineData(CompetitionStatus.RegistrationOpen)]
    [InlineData(CompetitionStatus.ResultsDraft)]
    [InlineData(CompetitionStatus.ResultsValidated)]
    public async Task Handle_WrongStatus_ShouldReturnNotInClosedError(CompetitionStatus status)
    {
        var competition = Build(status);
        _mockRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);

        var result = await _handler.Handle(new MoveToResultsDraftCommand(competition.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.NotInClosed");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class ValidateResultsCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Competition, Guid>> _mockRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly ValidateResultsCommandHandler _handler;

    public ValidateResultsCommandHandlerTests()
    {
        _mockRepo = new Mock<IGenericRepository<Competition, Guid>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new ValidateResultsCommandHandler(_mockRepo.Object, _mockUnitOfWork.Object);
    }

    private static Competition Build(CompetitionStatus status) =>
        new Competition { Id = Guid.NewGuid(), LeagueId = Guid.NewGuid(), CompetitionNumber = 1, Status = status, LastUpdateUtc = DateTime.UtcNow };

    [Fact]
    public async Task Handle_ResultsDraftCompetition_ShouldTransitionToResultsValidated()
    {
        var competition = Build(CompetitionStatus.ResultsDraft);
        _mockRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync((ErrorOr<int>)1);

        var result = await _handler.Handle(new ValidateResultsCommand(competition.Id), CancellationToken.None);

        result.IsError.Should().BeFalse();
        competition.Status.Should().Be(CompetitionStatus.ResultsValidated);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AlreadyValidated_ShouldReturnAlreadyValidatedError()
    {
        var competition = Build(CompetitionStatus.ResultsValidated);
        _mockRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);

        var result = await _handler.Handle(new ValidateResultsCommand(competition.Id), CancellationToken.None);

        result.FirstError.Code.Should().Be("Competition.AlreadyValidated");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(CompetitionStatus.Planned)]
    [InlineData(CompetitionStatus.RegistrationOpen)]
    [InlineData(CompetitionStatus.Closed)]
    public async Task Handle_WrongStatus_ShouldReturnNotInResultsDraftError(CompetitionStatus status)
    {
        var competition = Build(status);
        _mockRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);

        var result = await _handler.Handle(new ValidateResultsCommand(competition.Id), CancellationToken.None);

        result.FirstError.Code.Should().Be("Competition.NotInResultsDraft");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
