namespace FishClubAlginet.Application.Constants;

public class ValidatorsConstants
{

    public const string IdentityNationValidationNumberError = "El número de documento no es correcto, la letra no coincide.";



    //Logic for validating the control letter of DNI/NIE

    public const string ControlLetters = "TRWAGMYFPDXBNJZSQVHLCKE";

    #region Generics 
    public const string RequiredField = "This field is required.";
    public const string InvalidEmailFormat = "Invalid email format.";
    public const string PasswordsDoNotMatch = "Passwords do not match.";
    public const string MinimumLength = "The field must be at least {MinLength} characters long.";
    public const string MaximumLength = "The field must be at most {MaxLength} characters long.";
    public const string EmptyField = "The field cannot be empty.";
    #endregion
}
