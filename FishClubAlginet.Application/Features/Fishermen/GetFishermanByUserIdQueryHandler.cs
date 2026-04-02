using FishClubAlginet.Contracts.Dtos.Responses.Fishermen;

namespace FishClubAlginet.Application.Features.Fishermen;

public record GetFishermanByUserIdQuery(string UserId) : IRequest<ErrorOr<FishermanProfileDto>>;

public class GetFishermanByUserIdQueryHandler : IRequestHandler<GetFishermanByUserIdQuery, ErrorOr<FishermanProfileDto>>
{
    private readonly IGenericRepository<Fisherman, int> _repository;
    private readonly ILogger<GetFishermanByUserIdQueryHandler> _logger;

    public GetFishermanByUserIdQueryHandler(
        IGenericRepository<Fisherman, int> repository,
        ILogger<GetFishermanByUserIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<ErrorOr<FishermanProfileDto>> Handle(GetFishermanByUserIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var fisherman = _repository.GetAll()
                .FirstOrDefault(f => f.UserId == request.UserId && !f.IsDeleted);

            if (fisherman is null)
            {
                return Task.FromResult<ErrorOr<FishermanProfileDto>>(
                    Error.NotFound("Fisherman.NotFound", ErrorMessages.Fisherman_NotFound));
            }

            var dto = new FishermanProfileDto(
                Id: fisherman.Id,
                FirstName: fisherman.FirstName,
                LastName: fisherman.LastName,
                DateOfBirth: fisherman.DateOfBirth,
                DocumentType: fisherman.DocumentType.ToString(),
                DocumentNumber: fisherman.DocumentNumber,
                FederationLicense: fisherman.FederationLicense,
                RegionalLicense: fisherman.RegionalLicense,
                Street: fisherman.Address.Street,
                Number: fisherman.Address.Number,
                FloorDoor: fisherman.Address.FloorDoor,
                ZipCode: fisherman.Address.ZipCode,
                City: fisherman.Address.City,
                Province: fisherman.Address.Province);

            return Task.FromResult<ErrorOr<FishermanProfileDto>>(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fisherman profile for user {UserId}", request.UserId);
            return Task.FromResult<ErrorOr<FishermanProfileDto>>(
                Error.Failure(ValidatorsConstants.UnexpectedErrorCode, ValidatorsConstants.UnexpectedErrorMessage));
        }
    }
}
