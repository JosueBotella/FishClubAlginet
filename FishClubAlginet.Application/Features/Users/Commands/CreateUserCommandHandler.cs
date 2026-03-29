namespace FishClubAlginet.Application.Features.Users.Commands;

public record CreateUserCommand(string Email, string Password, string Role) : IRequest<ErrorOr<string>>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ErrorOr<string>>
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(IUserManagementService userManagementService, ILogger<CreateUserCommandHandler> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    public async Task<ErrorOr<string>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (request.Role != ApplicationConstants.Roles.Admin && request.Role != ApplicationConstants.Roles.Fisherman)
        {
            return Error.Validation("Roles.InvalidRole", ErrorMessages.User_InvalidRole);
        }

        var result = await _userManagementService.CreateUserWithRoleAsync(request.Email, request.Password, request.Role);

        if (result.IsError)
        {
            _logger.LogError("Error creating user {Email} with role {Role}: {Errors}",
                request.Email, request.Role,
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        else
        {
            _logger.LogInformation("User {Email} created with role {Role}", request.Email, request.Role);
        }

        return result;
    }

    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithErrorCode("Auth.Email.Required").WithMessage(ErrorMessages.Auth_Email_Required)
                .EmailAddress().WithErrorCode("Auth.Email.InvalidFormat").WithMessage(ErrorMessages.Auth_Email_InvalidFormat);

            RuleFor(x => x.Password)
                .NotEmpty().WithErrorCode("Auth.Password.Required").WithMessage(ErrorMessages.Auth_Password_Required)
                .MinimumLength(6).WithErrorCode("Auth.Password.MinLength").WithMessage(ErrorMessages.Auth_Password_MinLength);

            RuleFor(x => x.Role)
                .NotEmpty().WithErrorCode("Roles.Required").WithMessage(ErrorMessages.User_InvalidRole)
                .Must(r => r == ApplicationConstants.Roles.Admin || r == ApplicationConstants.Roles.Fisherman)
                .WithErrorCode("Roles.InvalidRole").WithMessage(ErrorMessages.User_InvalidRole);
        }
    }
}
