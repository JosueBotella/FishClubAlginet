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

    /* TODO: Implement Update method with domain event
    /// <summary>
    /// Updates the fisherman's information and raises an update domain event
    /// </summary>
    public void Update(
        string firstName,
        string lastName,
        Address address)
    {
        FirstName = firstName;
        LastName = lastName;
        Address = address;
        LastUpdateUtc = DateTime.UtcNow;

        this.RaiseDomainEvent(new FishermanUpdatedDomainEvent 
        { 
            Id = this.Id,
            FirstName = this.FirstName,
            LastName = this.LastName
        });
    }
    */

    /* TODO: Implement Delete method with domain event
    /// <summary>
    /// Soft deletes the fisherman and raises a delete domain event
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
        DeletedTimeUtc = DateTime.UtcNow;

        this.RaiseDomainEvent(new FishermanDeletedDomainEvent 
        { 
            Id = this.Id
        });
    }
    */
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
