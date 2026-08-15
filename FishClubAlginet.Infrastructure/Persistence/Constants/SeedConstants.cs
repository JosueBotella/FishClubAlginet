namespace FishClubAlginet.Infrastructure.Persistence.Constants;

[SuppressMessage("SonarQube", "S2068:Hardcoded credentials in test seeds", Justification = "Default seed credentials for local development and integration tests")]
public static class SeedConstants
{
    public const string DefaultAdminEmail = "jbotella@gmail.com";
    public const string DefaultFishermanEmail = "pescador@fishclubalginet.com";
    public const string DefaultPassword = "a5848b";

    // Backward compatibility
    public const string DefaultUserName = DefaultAdminEmail;
}
