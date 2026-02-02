namespace FishClubAlginet.Application.Abstractions;


public interface IRequest<out TResponse>
{
}


public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<ErrorOr<TResponse>>
{
    Task<ErrorOr<TResponse>> Handle(TRequest request, CancellationToken cancellationToken);
}
