namespace FishClubAlginet.Contracts.Dtos.Common;

public record PaginatedResult<T>(
    IList<T> Items,
    int TotalCount);
