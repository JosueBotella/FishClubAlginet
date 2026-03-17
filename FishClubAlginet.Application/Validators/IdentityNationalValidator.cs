namespace FishClubAlginet.Application.Validators;


public static class SpanishIdValidator
{
    /// <summary>
    /// Extension method for FluentValidation to validate Spanish IDs (DNI/NIE) or Passports.
    /// </summary>
    public static IRuleBuilderOptions<T, string> MustBeValidIdentification<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        Func<T, TypeNationalIdentifier> documentTypeSelector)
    {
        var ruleBuilderValidation = ruleBuilder.Must((rootObject, documentNumber) =>
        {
            var type = documentTypeSelector(rootObject);
            return IsDocumentValid(type, documentNumber);
        })
        .WithMessage(ValidatorsConstants.IdentityNationValidationNumberErrorMessage);

        return ruleBuilderValidation;
    }

    private static bool IsDocumentValid(TypeNationalIdentifier type, string documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
            return false;

        // Normalize: Upper case and trim spaces
        var sanitizedDoc = documentNumber.ToUpper().Trim();

        return type switch
        {
            TypeNationalIdentifier.Dni => ValidateDni(sanitizedDoc),
            TypeNationalIdentifier.Nie => ValidateNie(sanitizedDoc),
            TypeNationalIdentifier.Passport => ValidatePassport(sanitizedDoc),
            _ => false
        };
    }

    private static bool ValidateDni(string dni)
    {        
        if (!Regex.IsMatch(dni, @"^\d{8}[A-Z]$"))
            return false;

        return CheckControlLetter(dni[..8], dni[^1]);
    }

    private static bool ValidateNie(string nie)
    {
        // Format: X/Y/Z + 7 digits + 1 Letter (e.g., X1234567Z)
        if (!Regex.IsMatch(nie, @"^[XYZ]\d{7}[A-Z]$"))
            return false;

        // Replace prefix: X->0, Y->1, Z->2
        var prefix = nie[0] switch
        {
            'X' => "0",
            'Y' => "1",
            'Z' => "2",
            _ => "0"
        };

        var numberPart = prefix + nie.Substring(1, 7);
        return CheckControlLetter(numberPart, nie[^1]);
    }

    private static bool ValidatePassport(string passport)
    {
        // Passports don't have a standard algorithm, checking basic format (Alphanumeric, 5-20 chars)
        var regexPassport = Regex.IsMatch(passport, @"^[A-Z0-9]{5,20}$");
        return regexPassport;
    }

    private static bool CheckControlLetter(string numberString, char providedLetter)
    {        

        if (!int.TryParse(numberString, out int number))
            return false;

        var calculatedIndex = number % 23;
        return ValidatorsConstants.ControlLetters[calculatedIndex] == providedLetter;
    }
}
