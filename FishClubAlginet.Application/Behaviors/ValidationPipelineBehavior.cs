namespace FishClubAlginet.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that executes all registered FluentValidation validators
/// for a request before reaching the handler.
/// Only activates when the response type implements IErrorOr (i.e., ErrorOr&lt;T&gt;).
/// Validators are registered automatically by AddValidatorsFromAssembly in DependencyInjection.
/// </summary>
public sealed class ValidationPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // No validators registered for this request → skip validation entirely
        if (!_validators.Any())
            return await next(cancellationToken);

        var errors = _validators
            .Select(v => v.Validate(request))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => Error.Validation(
                code: failure.ErrorCode,
                description: failure.ErrorMessage))
            .ToList();

        if (errors.Count > 0)
            return CreateValidationResult(errors);

        return await next(cancellationToken);
    }

    /// <summary>
    /// Creates a TResponse (always ErrorOr&lt;T&gt;) from a list of validation errors.
    /// Uses dynamic dispatch to invoke ErrorOr's implicit operator from List&lt;Error&gt;.
    /// </summary>
    private static TResponse CreateValidationResult(List<Error> errors) =>
            (TResponse)(dynamic)errors;
}
