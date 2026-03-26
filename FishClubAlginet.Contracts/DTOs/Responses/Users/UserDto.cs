namespace FishClubAlginet.Contracts.Dtos.Responses.Users;

public record UserDto(
    string Id,
    string Email,
    bool IsLockedOut,
    IList<string> Roles);
