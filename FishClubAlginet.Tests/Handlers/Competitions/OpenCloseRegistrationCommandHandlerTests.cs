using FluentAssertions;
using FishClubAlginet.Application.Features.Competitions;

namespace FishClubAlginet.Tests.Handlers.Competitions;

public class OpenRegistrationCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Competition, Guid>> _mockRepo;
    private readonly Mock<IUnitOfWork>                           _mockUnitOfWork;
    private readonly OpenRegistrationCommandHandler              _handler;

    public OpenRegistrationCommandHandlerTests()
    {
        _mockRepo       = new Mock<IGenericRepository<Competition, Guid>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler        = new OpenRegistrationCommandHandler(_mockRepo.Object, _mockUnitOfWork.Object);
    }

    private static Competition BuildCompetition(CompetitionStatus status) =>
        new Competition
        {
            Id               = Guid.NewGuid(),
            LeagueId         = Guid.NewGuid(),
            CompetitionNumber = 1,
            Status           = status,
            LastUpdateUtc    = DateTime.UtcNow
        };

    [Fact]
    public async Task Handle_PlannedCompetition_ShouldTransitionToRegistrationOpen()
    {
        // Arrange
        var competition = BuildCompetition(CompetitionStatus.Planned);
        _mockRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        // Act
        var result = await _handler.Handle(new OpenRegistrationCommand(competition.Id), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        competition.Status.Should().Be(CompetitionStatus.RegistrationOpen);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CompetitionNotFound_ShouldReturnCompetitionNotFoundError()
    {
        _mockRepo.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Competition?)null);

        var result = await _handler.Handle(new OpenRegistrationCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.NotFound");
    }

    [Theory]
    [InlineData(CompetitionStatus.RegistrationOpen)]
    [InlineData(CompetitionStatus.Closed)]
    [InlineData(CompetitionStatus.ResultsDraft)]
    [InlineData(CompetitionStatus.ResultsValidated)]
    public async Task Handle_AlreadyPastPlanned_ShouldReturnInvalidStatusTransitionError(CompetitionStatus status)
    {
        var competition = BuildCompetition(status);
        _mockRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);

        var result = await _handler.Handle(new OpenRegistrationCommand(competition.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.InvalidStatusTransition");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class CloseRegistrationCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Competition, Guid>> _mockRepo;
    private readonly Mock<IUnitOfWork>                           _mockUnitOfWork;
    private readonly CloseRegistrationCommandHandler             _handler;

    public CloseRegistrationCommandHandlerTests()
    {
        _mockRepo       = new Mock<IGenericRepository<Competition, Guid>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler        = new CloseRegistrationCommandHandler(_mockRepo.Object, _mockUnitOfWork.Object);
    }

    private static Competition BuildCompetition(CompetitionStatus status) =>
        new Competition
        {
            Id               = Guid.NewGuid(),
            LeagueId         = Guid.NewGuid(),
            CompetitionNumber = 1,
            Status           = status,
            LastUpdateUtc    = DateTime.UtcNow
        };

    [Fact]
    public async Task Handle_OpenCompetition_ShouldTransitionToClosed()
    {
        // Arrange
        var competition = BuildCompetition(CompetitionStatus.RegistrationOpen);
        _mockRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ErrorOr<int>)1);

        // Act
        var result = await _handler.Handle(new CloseRegistrationCommand(competition.Id), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        competition.Status.Should().Be(CompetitionStatus.Closed);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CompetitionNotFound_ShouldReturnCompetitionNotFoundError()
    {
        _mockRepo.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Competition?)null);

        var result = await _handler.Handle(new CloseRegistrationCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.NotFound");
    }

    [Theory]
    [InlineData(CompetitionStatus.Planned)]
    [InlineData(CompetitionStatus.Closed)]
    [InlineData(CompetitionStatus.ResultsDraft)]
    [InlineData(CompetitionStatus.ResultsValidated)]
    public async Task Handle_NotRegistrationOpen_ShouldReturnInvalidStatusTransitionError(CompetitionStatus status)
    {
        var competition = BuildCompetition(status);
        _mockRepo.Setup(r => r.GetById(competition.Id)).ReturnsAsync(competition);

        var result = await _handler.Handle(new CloseRegistrationCommand(competition.Id), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Competition.InvalidStatusTransition");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
