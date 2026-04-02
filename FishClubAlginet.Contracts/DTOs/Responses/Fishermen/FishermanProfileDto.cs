namespace FishClubAlginet.Contracts.Dtos.Responses.Fishermen;

public record FishermanProfileDto(
    int Id,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string DocumentType,
    string DocumentNumber,
    string? FederationLicense,
    string? RegionalLicense,
    string Street,
    string Number,
    string FloorDoor,
    string ZipCode,
    string City,
    string Province);
