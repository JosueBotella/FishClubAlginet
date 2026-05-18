namespace FishClubAlginet.Application.Features.Competitions;

public record OpenRegistrationCommand(Guid CompetitionId) : IRequest<ErrorOr<Success>>;

public sealed class OpenRegistrationCommandHandler
    : IRequestHandler<OpenRegistrationCommand, ErrorOr<Success>>
{
    private readonly IGenericRepository<Competition, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public OpenRegistrationCommandHandler(
        IGenericRepository<Competition, Guid> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        OpenRegistrationCommand request,
        CancellationToken cancellationToken)
    {
        var competition = await _repository.GetById(request.CompetitionId);
        if (competition is null || competition.IsDeleted)
            return Errors.Competition.NotFound;

        if (competition.Status != CompetitionStatus.Planned)
            return Errors.Competition.InvalidStatusTransition;

        competition.OpenRegistration();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}

public class OpenRegistrationCommandValidator : AbstractValidator<OpenRegistrationCommand>
{
    public OpenRegistrationCommandValidator()
    {
        RuleFor(x => x.CompetitionId)
            .NotEmpty()
            .WithErrorCode("Competition.CompetitionId.Required")
            .WithMessage("CompetitionId is required.");
    }
}
