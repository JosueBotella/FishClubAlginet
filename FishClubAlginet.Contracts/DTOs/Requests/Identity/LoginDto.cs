namespace FishClubAlginet.Contracts.Dtos.Requests.Identity;

public class LoginDto
{
    private string _userName = string.Empty;

    public string? Email
    {
        get => _userName;
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
                _userName = value;
        }
    }

    public string UserName
    {
        get => _userName;
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
                _userName = value;
        }
    }

    public string Password { get; set; } = string.Empty;
}
