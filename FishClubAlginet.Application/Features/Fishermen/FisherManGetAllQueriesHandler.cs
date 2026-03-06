namespace FishClubAlginet.Application.Features.Fishermen;

public record FisherManGetAllQueryResponse(
    int Id,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    TypeNationalIdentifier DocumentType,
    string DocumentNumber,
    string? FederationLicense,
    string AddressCity,
    string AddressProvince
);

public record FisherManGetAllQuery : IRequest<ErrorOr<List<FisherManGetAllQueryResponse>>>;

public class FisherManGetAllQueryHandler : IRequestHandler<FisherManGetAllQuery, ErrorOr<List<FisherManGetAllQueryResponse>>>
{
    private readonly IGenericRepository<Fisherman, int> _genericRepository;

    public FisherManGetAllQueryHandler(IGenericRepository<Fisherman, int> genericRepository)
    {
        _genericRepository = genericRepository;
    }

    public Task<ErrorOr<List<FisherManGetAllQueryResponse>>> Handle(FisherManGetAllQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var fishermen = _genericRepository.GetAll()
                .Select(f => new FisherManGetAllQueryResponse(
                    Id: f.Id,
                    FirstName: f.FirstName,
                    LastName: f.LastName,
                    DateOfBirth: f.DateOfBirth,
                    DocumentType: f.DocumentType,
                    DocumentNumber: f.DocumentNumber,
                    FederationLicense: f.FederationLicense,
                    AddressCity: f.Address.City,
                    AddressProvince: f.Address.Province
                ))
                .ToList();

            return Task.FromResult<ErrorOr<List<FisherManGetAllQueryResponse>>>(fishermen);
        }
        catch
        {
            var error = Error.Failure(
                code: ValidatorsConstants.UnexpectedErrorCode,
                description: ValidatorsConstants.UnexpectedErrorMessage);
            return Task.FromResult<ErrorOr<List<FisherManGetAllQueryResponse>>>(error);
        }
    }
}
