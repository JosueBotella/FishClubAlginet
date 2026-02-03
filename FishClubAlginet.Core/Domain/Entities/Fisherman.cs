namespace FishClubAlginet.Core.Domain.Entities;

public class Fisherman : BaseEntity<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }

    // Identification
    public TypeNationalIdentifier DocumentType { get; set; } = TypeNationalIdentifier.Dni;
    public string DocumentNumber { get; set; } = string.Empty;

    // Licenses
    public string? FederationLicense { get; set; } = string.Empty; // Core ID for the Federation
    public string? RegionalLicense { get; set; } // GVA License (Optional)

    // Contact & Location
    public Address Address { get; set; } = new Address();
    public string? UserId { get; set; } 

    
    public bool IsMinor => DateOfBirth > DateTime.UtcNow.AddYears(-18);
}
public static class  FisherManConstraints
{
    public const int FistNameMaxLength = 50;
    public const int LastNameMaxLength = 50;
    public const int DocumentTypeMaxLength = 10;
    public const int DocumentNumberMaxLength = 20;
    public const int DocumentNumberMinLength = 10;
    public const int FederationLicenseMaxLength = 20;
    public const int MinimumAge = 16;
}
