namespace FishClubAlginet.Blazor.Auth;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    private static readonly AuthenticationState _anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public JwtAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>(UIConstants.TokenKey);

        if (string.IsNullOrEmpty(token))
        {
            return _anonymous;
        }

        if (IsTokenExpired(token))
        {
            await LogoutAsync();
            return _anonymous;
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return new AuthenticationState(
            new ClaimsPrincipal(GetClaimsFromToken(token)));
    }

    public async Task LoginAsync(string token)
    {
        await _localStorage.SetItemAsync(UIConstants.TokenKey, token);

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var authState = new AuthenticationState(
            new ClaimsPrincipal(GetClaimsFromToken(token)));
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(UIConstants.TokenKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
    }

    private static bool IsTokenExpired(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.ValidTo < DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }

    private static ClaimsIdentity GetClaimsFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        return new ClaimsIdentity(jwtToken.Claims, "jwt");
    }
}
