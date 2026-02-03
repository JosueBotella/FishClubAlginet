namespace FishClubAlginet.Contracts.Dtos.Requests.Fisherman;

public class CreateFishermanDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public TypeNationalIdentifier DocumentType { get; set; } = TypeNationalIdentifier.Dni;
    public string DocumentNumber { get; set; } = string.Empty;

    public string FederationLicense { get; set; } = string.Empty;
}
