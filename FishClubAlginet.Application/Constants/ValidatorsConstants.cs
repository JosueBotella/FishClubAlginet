namespace FishClubAlginet.Application.Constants;

public static class ValidatorsConstants
{
    // Logic for validating the control letter of DNI/NIE
    public const string ControlLetters = "TRWAGMYFPDXBNJZSQVHLCKE";

    #region Generic

    public const string NotFoundErrorCode = "NotFound";
    public const string NotFoundErrorMessage = "The requested resource was not found.";

    public const string UnexpectedErrorCode = "System.Unexpected";
    public const string UnexpectedErrorMessage = "An unexpected error occurred.";

    #endregion

    #region Fisherman

    public static class FisherManValidationConstants
    {
        // Regex
        public const string DocumentNumberRegex = @"^[A-Za-z0-9\-\s]+$";

        // Error codes — stable, language-neutral identifiers used by the frontend for i18n
        public const string FirstNameRequiredErrorCode = "Fisherman.FirstName.Required";
        public const string FirstNameNotWhitespaceErrorCode = "Fisherman.FirstName.NotWhitespace";
        public const string FirstNameMaxLengthErrorCode = "Fisherman.FirstName.MaxLength";

        public const string LastNameRequiredErrorCode = "Fisherman.LastName.Required";
        public const string LastNameNotWhitespaceErrorCode = "Fisherman.LastName.NotWhitespace";
        public const string LastNameMaxLengthErrorCode = "Fisherman.LastName.MaxLength";

        public const string BirthDateInPastErrorCode = "Fisherman.BirthDate.InPast";
        public const string MinimumAgeMessageErrorCode = "Fisherman.BirthDate.MinimumAge";

        public const string InvalidDocumentTypeErrorCode = "Fisherman.DocumentType.Invalid";

        public const string DocumentNumberRequiredErrorCode = "Fisherman.DocumentNumber.Required";
        public const string DocumentNumberNotWhitespaceErrorCode = "Fisherman.DocumentNumber.NotWhitespace";
        public const string DocumentNumberMinLengthErrorCode = "Fisherman.DocumentNumber.MinLength";
        public const string DocumentNumberMaxLengthErrorCode = "Fisherman.DocumentNumber.MaxLength";
        public const string DocumentNumberInvalidFormatErrorCode = "Fisherman.DocumentNumber.InvalidFormat";
        public const string DocumentNumberInvalidControlLetterErrorCode = "Fisherman.DocumentNumber.InvalidControlLetter";

        public const string FederationLicenseMaxLengthErrorCode = "Fisherman.FederationLicense.MaxLength";

        // English fallback descriptions — consumed from ErrorMessages.resx (i18n-ready)
        public static string FirstNameRequiredErrorMessage => ErrorMessages.Fisherman_FirstName_Required;
        public static string FirstNameNotWhitespaceErrorMessage => ErrorMessages.Fisherman_FirstName_NotWhitespace;
        public static string FirstNameMaxLengthErrorMessage => ErrorMessages.Fisherman_FirstName_MaxLength;

        public static string LastNameRequiredErrorMessage => ErrorMessages.Fisherman_LastName_Required;
        public static string LastNameNotWhitespaceErrorMessage => ErrorMessages.Fisherman_LastName_NotWhitespace;
        public static string LastNameMaxLengthErrorMessage => ErrorMessages.Fisherman_LastName_MaxLength;

        public static string BirthDateInPastErrorMessage => ErrorMessages.Fisherman_BirthDate_InPast;
        public static string MinimumAgeMessageErrorMessage => ErrorMessages.Fisherman_MinimumAge;

        public static string InvalidDocumentTypeErrorMessage => ErrorMessages.Fisherman_DocumentType_Invalid;

        public static string DocumentNumberRequiredErrorMessage => ErrorMessages.Fisherman_DocumentNumber_Required;
        public static string DocumentNumberNotWhitespaceErrorMessage => ErrorMessages.Fisherman_DocumentNumber_NotWhitespace;
        public static string DocumentNumberMinLengthErrorMessage => ErrorMessages.Fisherman_DocumentNumber_MinLength;
        public static string DocumentNumberMaxLengthErrorMessage => ErrorMessages.Fisherman_DocumentNumber_MaxLength;
        public static string DocumentNumberInvalidFormatErrorMessage => ErrorMessages.Fisherman_DocumentNumber_InvalidFormat;
        public static string DocumentNumberInvalidControlLetterErrorMessage => ErrorMessages.Fisherman_DocumentNumber_InvalidControlLetter;

        public static string FederationLicenseMaxLengthErrorMessage => ErrorMessages.Fisherman_FederationLicense_MaxLength;
    }

    #endregion
}
