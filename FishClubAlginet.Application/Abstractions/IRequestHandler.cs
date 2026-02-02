namespace FishClubAlginet.Application.Abstractions;

public interface IRequestHandler<in TRequest, TResponse>
{
    Task<ErrorOr<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
