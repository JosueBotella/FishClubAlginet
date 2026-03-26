namespace FishClubAlginet.Application.Validators;

public static class SpanishIdValidator
{
    /// <summary>
    /// FluentValidation extension to validate Spanish identification documents (DNI/NIE/Passport).
    /// </summary>
    public static IRuleBuilderOptions<T, string> MustBeValidIdentification<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        Func<T, TypeNationalIdentifier> documentTypeSelector)
    {
        return ruleBuilder
            .Must((rootObject, documentNumber) =>
            {
                var type = documentTypeSelector(rootObject);
                return IsDocumentValid(type, documentNumber);
            })
            .WithErrorCode(ValidatorsConstants.FisherManValidationConstants.DocumentNumberInvalidControlLetterErrorCode)
            .WithMessage(ValidatorsConstants.FisherManValidationConstants.DocumentNumberInvalidControlLetterErrorMessage);
    }

    private static bool IsDocumentValid(TypeNationalIdentifier type, string documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
            return false;

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
        // Format: X/Y/Z + 7 digits + 1 letter (e.g., X1234567Z)
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
        return Regex.IsMatch(passport, @"^[A-Z0-9]{5,20}$");
    }

    private static bool CheckControlLetter(string numberString, char providedLetter)
    {
        if (!int.TryParse(numberString, out int number))
            return false;

        var calculatedIndex = number % 23;
        return ValidatorsConstants.ControlLetters[calculatedIndex] == providedLetter;
    }
}
