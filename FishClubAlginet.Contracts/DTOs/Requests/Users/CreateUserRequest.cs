namespace FishClubAlginet.Contracts.Dtos.Requests.Users;

public record CreateUserRequest(string Email, string Password, string Role);
