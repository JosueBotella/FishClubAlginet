namespace FishClubAlginet.Application.Features.Competitions;

public record ReopenRegistrationCommand(Guid CompetitionId) : IRequest<ErrorOr<Success>>;

public sealed class ReopenRegistrationCommandHandler
    : IRequestHandler<ReopenRegistrationCommand, ErrorOr<Success>>
{
    private readonly IGenericRepository<Competition, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ReopenRegistrationCommandHandler(
        IGenericRepository<Competition, Guid> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        ReopenRegistrationCommand request,
        CancellationToken cancellationToken)
    {
        var competition = await _repository.GetById(request.CompetitionId);
        if (competition is null || competition.IsDeleted)
            return Errors.Competition.NotFound;

        if (competition.Status != CompetitionStatus.Closed)
            return Errors.Competition.InvalidStatusTransition;

        // Business rule: only allowed within 30 days of closing
        var reopened = competition.ReopenRegistration();
        if (!reopened)
            return Errors.Competition.ReopenWindowExpired;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}

public class ReopenRegistrationCommandValidator : AbstractValidator<ReopenRegistrationCommand>
{
    public ReopenRegistrationCommandValidator()
    {
        RuleFor(x => x.CompetitionId)
            .NotEmpty()
            .WithErrorCode("Competition.CompetitionId.Required")
            .WithMessage("CompetitionId is required.");
    }
}
