namespace FishClubAlginet.Tests.Validators;

public class RegisterUserValidatorsTests 
{
    private readonly IdentityRegisterUserValidator _validator = new();

    [Fact]
    public void Email_Empty_ShouldHaveValidationError()
    {
        var dto = new RegisterUserDto
        {
            Email = string.Empty,
            Password = "secret1",
            ConfirmPassword = "secret1"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_InvalidFormat_ShouldHaveValidationError()
    {
        var dto = new RegisterUserDto
        {
            Email = "not-an-email",
            Password = "secret1",
            ConfirmPassword = "secret1"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_Valid_ShouldNotHaveValidationError()
    {
        var dto = new RegisterUserDto
        {
            Email = "user@example.com",
            Password = "secret1",
            ConfirmPassword = "secret1"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Password_Empty_ShouldHaveValidationError()
    {
        var dto = new RegisterUserDto
        {
            Email = "user@example.com",
            Password = string.Empty,
            ConfirmPassword = string.Empty
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_TooShort_ShouldHaveValidationError()
    {
        var dto = new RegisterUserDto
        {
            Email = "user@example.com",
            Password = "12345",
            ConfirmPassword = "12345"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_MinimumLength_ShouldNotHaveValidationError()
    {
        var dto = new RegisterUserDto
        {
            Email = "user@example.com",
            Password = "123456",
            ConfirmPassword = "123456"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ConfirmPassword_NotMatching_ShouldHaveValidationError()
    {
        var dto = new RegisterUserDto
        {
            Email = "user@example.com",
            Password = "123456",
            ConfirmPassword = "abcdef"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void ConfirmPassword_Matching_ShouldNotHaveValidationError()
    {
        var dto = new RegisterUserDto
        {
            Email = "user@example.com",
            Password = "123456",
            ConfirmPassword = "123456"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.ConfirmPassword);
    }
}
