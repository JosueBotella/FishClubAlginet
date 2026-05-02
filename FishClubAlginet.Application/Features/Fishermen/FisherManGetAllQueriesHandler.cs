using FishClubAlginet.Contracts.Dtos.Common;

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
    string AddressProvince,
    bool IsDeleted
);

public record FisherManGetAllQuery(int Skip, int Take, string? Search, bool ShowDeleted = false) : IRequest<ErrorOr<PaginatedResult<FisherManGetAllQueryResponse>>>;

public class FisherManGetAllQueryHandler : IRequestHandler<FisherManGetAllQuery, ErrorOr<PaginatedResult<FisherManGetAllQueryResponse>>>
{
    private readonly IGenericRepository<Fisherman, int> _genericRepository;

    public FisherManGetAllQueryHandler(IGenericRepository<Fisherman, int> genericRepository)
    {
        _genericRepository = genericRepository;
    }

    public Task<ErrorOr<PaginatedResult<FisherManGetAllQueryResponse>>> Handle(FisherManGetAllQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _genericRepository.GetAll()
                .Where(f => f.IsDeleted == request.ShowDeleted);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(f =>
                    f.FirstName.ToLower().Contains(search) ||
                    f.LastName.ToLower().Contains(search) ||
                    f.DocumentNumber.ToLower().Contains(search) ||
                    f.FederationLicense != null && f.FederationLicense.ToLower().Contains(search));
            }

            var totalCount = query.Count();

            var fishermen = query
                .OrderBy(f => f.LastName)
                .ThenBy(f => f.FirstName)
                .Skip(request.Skip)
                .Take(request.Take)
                .Select(f => new FisherManGetAllQueryResponse(
                    Id: f.Id,
                    FirstName: f.FirstName,
                    LastName: f.LastName,
                    DateOfBirth: f.DateOfBirth,
                    DocumentType: f.DocumentType,
                    DocumentNumber: f.DocumentNumber,
                    FederationLicense: f.FederationLicense,
                    AddressCity: f.Address.City,
                    AddressProvince: f.Address.Province,
                    IsDeleted: f.IsDeleted
                ))
                .ToList();

            return Task.FromResult<ErrorOr<PaginatedResult<FisherManGetAllQueryResponse>>>(
                new PaginatedResult<FisherManGetAllQueryResponse>(fishermen, totalCount));
        }
        catch
        {
            var error = Error.Failure(
                code: ValidatorsConstants.UnexpectedErrorCode,
                description: ValidatorsConstants.UnexpectedErrorMessage);
            return Task.FromResult<ErrorOr<PaginatedResult<FisherManGetAllQueryResponse>>>(error);
        }
    }
}
