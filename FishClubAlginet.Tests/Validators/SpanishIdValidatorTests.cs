namespace FishClubAlginet.Tests.Validators;

public class CreateFishermanValidatorTests
{
    private readonly CreateFishermanValidator _validator;

    public CreateFishermanValidatorTests()
    {
        _validator = new CreateFishermanValidator();
    }

    [Fact]
    public void Validate_WithValidDni_ShouldNotHaveError()
    {
        // Arrange
        var model = new CreateFishermanDto
        {
            FirstName = "John",
            LastName = "Doe",
            FederationLicense = "VAL-2026",
            DocumentType = TypeNationalIdentifier.Dni,
            DocumentNumber = "12345678Z", // Valid: 12345678 % 23 = 14 -> Z
            AddressStreet = "123 Main St",
            AddressCity = "Valencia",
            AddressZipCode = "46001",
            AddressProvince = "Valencia",
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DocumentNumber);
    }

    [Fact]
    public void Validate_WithInvalidDniLetter_ShouldHaveError()
    {
        // Arrange
        var model = new CreateFishermanDto
        {
            DocumentType = TypeNationalIdentifier.Dni,
            DocumentNumber = "12345678A" ,// Invalid letter
            AddressStreet = "123 Main St",
            AddressCity = "Valencia",
            AddressZipCode = "46001",
            AddressProvince = "Valencia",
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DocumentNumber)
              .WithErrorCode(ValidatorsConstants.FisherManValidationConstants.DocumentNumberInvalidControlLetterErrorCode);
    }

    [Theory]
    [InlineData("X1234567L")]
    [InlineData("Z8883297N")]
    public void Validate_WithValidNie_ShouldPass(string validNie)
    {
        var model = new CreateFishermanDto
        {
            DocumentType = TypeNationalIdentifier.Nie,
            DocumentNumber = validNie,
            AddressStreet = "123 Main St",
            AddressCity = "Valencia",
            AddressZipCode = "46001",
            AddressProvince = "Valencia",
        };

        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.DocumentNumber);
    }
}
