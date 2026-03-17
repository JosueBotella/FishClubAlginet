

namespace FishClubAlginet.Tests.Data;

public class FisherManFixture
{
    public static readonly string FirstName = "John";
    public static readonly string LastName = "Doe";
    public static readonly DateTime DateOfBirth = new(1990, 1, 1);
    public static readonly TypeNationalIdentifier DocumentType = TypeNationalIdentifier.Dni;
    public static readonly string DocumentNumber = "73582791H";
    public static readonly string FederationLicense = "FL123456";
    public static readonly string AddressStreet = "123 Main St";
    public static readonly string AddressCity = "Alginet";
    public static readonly string AddressZipCode = "46230";
    public static readonly string AddressProvince = "Valencia";

    public static FisherManCommand GetFisherManCommand() => new FisherManCommand(
            FirstName,
            LastName,
            DateOfBirth,
            DocumentType,
            DocumentNumber,
            FederationLicense,
            AddressStreet,
            AddressCity,
            AddressZipCode,
            AddressProvince
        );
}
