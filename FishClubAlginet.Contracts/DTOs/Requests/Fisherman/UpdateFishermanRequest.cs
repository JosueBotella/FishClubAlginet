using System.ComponentModel.DataAnnotations;

namespace FishClubAlginet.Contracts.Dtos.Requests.Fisherman;

public class UpdateFishermanRequest
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? FederationLicense { get; set; }

    public string? AddressStreet { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressZipCode { get; set; }
    public string? AddressProvince { get; set; }
}
