namespace FishClubAlginet.Application.Constants;

public class ValidatorsConstants
{

    public const string IdentityNationValidationNumberErrorMessage = "El número de documento no es correcto, la letra no coincide.";



    //Logic for validating the control letter of DNI/NIE

    public const string ControlLetters = "TRWAGMYFPDXBNJZSQVHLCKE";

    #region Generics 
    public const string RequiredField = "This field is required.";
    public const string InvalidEmailFormat = "Invalid email format.";
    public const string PasswordsDoNotMatch = "Passwords do not match.";
    public const string MinimumLength = "The field must be at least {MinLength} characters long.";
    public const string MaximumLength = "The field must be at most {MaxLength} characters long.";
    public const string EmptyField = "The field cannot be empty.";


    public const string NotFoundErrorCode = "NotFoundErrorCode";
    public const string NotFoundErrorMessage = "Not found anything";
    #endregion


    #region Fisherman
    public static class FisherManValidationConstants
    {
      

        // Regex
        public const string DocumentNumberRegex = @"^[A-Za-z0-9\-\s]+$";

        // Mensajes
        public const string FirstNameRequiredErrorCode = "FirstNameRequiredErrorCode";
        public const string FirstNameRequiredErrorMessage = "El nombre es obligatorio.";
        public const string FirstNameNotWhitespaceErrorCode = "FirstNameNotWhitespaceErrorCode";
        public const string FirstNameNotWhitespaceErrorMessage = "El nombre no puede ser vacío o solo espacios.";
        public const string FirstNameMaxLengthErrorCode = "FirstNameMaxLengthErrorCode";
        public const string FirstNameMaxLengthErrorMessage = "El nombre no puede tener más de {0} caracteres.";

        public const string LastNameRequiredErrorCode = "LastNameRequiredErrorCode";
        public const string LastNameRequiredErrorMessage = "El apellido es obligatorio.";
        public const string LastNameNotWhitespaceErrorCode = "LastNameNotWhitespaceErrorCode";
        public const string LastNameNotWhitespaceErrorMessage = "El apellido no puede ser vacío o solo espacios.";
        public const string LastNameMaxLengthErrorCode = "LastNameMaxLengthErrorCode";
        public const string LastNameMaxLengthErrorMessage = "El apellido no puede tener más de {0} caracteres.";

        public const string BirthDateInPastErrorCode = "BirthDateInPastErrorCode";
        public const string BirthDateInPastErrorMessage = "La fecha de nacimiento debe ser anterior a la fecha actual.";
        public const string MinimumAgeMessageErrorCode = "MinimumAgeMessageErrorCode";
        public const string MinimumAgeMessageErrorMessage = "El pescador debe tener al menos {0} años.";

        public const string InvalidDocumentTypeErrorCode = "InvalidDocumentTypeErrorCode";
        public const string InvalidDocumentTypeErrorMessage = "El tipo de documento no es válido.";

        public const string DocumentNumberRequiredErrorCode = "DocumentNumberRequiredErroCode";
        public const string DocumentNumberRequiredErrorMessage = "El número de documento es obligatorio.";
        public const string DocumentNumberNotWhitespaceErroCode = "DocumentNumberNotWhitespaceErroCode";
        public const string DocumentNumberNotWhitespaceErrorMessage = "El número de documento no puede ser vacío o solo espacios.";
        public const string DocumentNumberMinLengthErrorCode = "DocumentNumberMinLengthErroCode";
        public const string DocumentNumberMinLengthErrorMessage = "El número de documento debe tener al menos {0} caracteres.";
        public const string DocumentNumberMaxLengthErrorCode = "DocumentNumberMaxLengthErroCode";
        public const string DocumentNumberMaxLengthErrorMessage = "El número de documento no puede tener más de {0} caracteres.";
        public const string DocumentNumberInvalidFormatErrorCode = "DocumentNumberInvalidFormatErroCode";
        public const string DocumentNumberInvalidFormatErrorMessage = "El número de documento solo puede contener letras, números, guiones y espacios.";

        public const string FederationLicenseMaxLengthErrorCode = "FederationLicenseMaxLengthErrorCode";
        public const string FederationLicenseMaxLengthErrorMessage = "La licencia de la federación no puede tener más de {0} caracteres.";
    }

    #endregion

    #region ExceptionsMessages
        public const string UnexpectedErrorCode = "UnexpectedErrorCode";
        public const string UnexpectedErrorMessage = "An unexpected error occurred. ";
    #endregion
}
