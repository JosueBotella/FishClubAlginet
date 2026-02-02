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
    public string FederationLicense { get; set; } = string.Empty; // Core ID for the Federation
    public string? RegionalLicense { get; set; } // GVA License (Optional)

    // Contact & Location
    public Address Address { get; set; } = new Address();
    public string? UserId { get; set; } 

    
    public bool IsMinor => DateOfBirth > DateTime.UtcNow.AddYears(-18);
}
