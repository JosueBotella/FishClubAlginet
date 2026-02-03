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


    #region Fisherman
    public static class FisherManValidationConstants
    {
      

        // Regex
        public const string DocumentNumberRegex = @"^[A-Za-z0-9\-\s]+$";

        // Mensajes
        public const string FirstNameRequired = "El nombre es obligatorio.";
        public const string FirstNameNotWhitespace = "El nombre no puede ser vacío o solo espacios.";
        public const string FirstNameMaxLength = "El nombre no puede tener más de {0} caracteres.";

        public const string LastNameRequired = "El apellido es obligatorio.";
        public const string LastNameNotWhitespace = "El apellido no puede ser vacío o solo espacios.";
        public const string LastNameMaxLength = "El apellido no puede tener más de {0} caracteres.";

        public const string BirthDateInPast = "La fecha de nacimiento debe ser anterior a la fecha actual.";
        public const string MinimumAgeMessage = "El pescador debe tener al menos {0} años.";

        public const string InvalidDocumentType = "El tipo de documento no es válido.";

        public const string DocumentNumberRequired = "El número de documento es obligatorio.";
        public const string DocumentNumberNotWhitespace = "El número de documento no puede ser vacío o solo espacios.";
        public const string DocumentNumberMinLength = "El número de documento debe tener al menos {0} caracteres.";
        public const string DocumentNumberMaxLength = "El número de documento no puede tener más de {0} caracteres.";
        public const string DocumentNumberInvalidFormat = "El número de documento solo puede contener letras, números, guiones y espacios.";

        public const string FederationLicenseMaxLengthMessage = "La licencia de la federación no puede tener más de {0} caracteres.";
    }

    #endregion
}
