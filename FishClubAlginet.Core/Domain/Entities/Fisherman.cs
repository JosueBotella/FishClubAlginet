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
    public string? FederationNumber { get; set; } // Unique federative identifier, e.g. "V-552"
    public string? RegionalLicense { get; set; } // GVA License (Optional)

    // Contact & Location
    public Address Address { get; set; } = new Address();
    public string? UserId { get; set; } 


    public bool IsMinor => DateOfBirth > DateTime.UtcNow.AddYears(-18);

    /// <summary>
    /// Factory method to create a new Fisherman
    /// </summary>
    public static Fisherman Create(
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        TypeNationalIdentifier documentType,
        string documentNumber,
        string? federationLicense,
        Address address)
    {
        var fisherman = new Fisherman
        {
            Id = 0, // Will be set by the database
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            DocumentType = documentType,
            DocumentNumber = documentNumber,
            FederationLicense = federationLicense ?? string.Empty,
            Address = address,
            LastUpdateUtc = DateTime.UtcNow
        };

        return fisherman;
    }

    /// <summary>
    /// Updates the fisherman's mutable fields.
    /// The caller (command handler) is responsible for raising FishermanUpdatedDomainEvent
    /// before saving, following the same pattern as Fisherman.Create().
    /// </summary>
    public void Update(
        string firstName,
        string lastName,
        string? federationLicense,
        Address address)
    {
        FirstName = firstName;
        LastName = lastName;
        FederationLicense = federationLicense;
        Address = address;
        LastUpdateUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft-deletes the fisherman.
    /// The caller (command handler) is responsible for raising FishermanDeletedDomainEvent
    /// before saving, following the same pattern as Fisherman.Create().
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
        DeletedTimeUtc = DateTime.UtcNow;
    }
}
public static class  FisherManConstraints
{
    public const int FistNameMaxLength = 50;
    public const int LastNameMaxLength = 50;
    public const int DocumentTypeMaxLength = 10;
    public const int DocumentNumberMaxLength = 20;
    public const int DocumentNumberMinLength = 10;
    public const int FederationLicenseMaxLength = 20;
    public const int FederationNumberMaxLength = 20;
    public const int MinimumAge = 16;
}
