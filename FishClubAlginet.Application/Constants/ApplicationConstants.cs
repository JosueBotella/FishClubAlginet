namespace FishClubAlginet.Application.Constants;

public static class ApplicationConstants
{
    public static class ConfigurationProgram
    {
        public const string AddCors_MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        public const string AddCors_WithOrigins = "http://localhost:5173";
    }

    public static class Database
    {
        public const string ConnectionName = "LocalConnectionString";
        public const string MigrationErrorMessage = "An error occurred during database migration.";
    }

    public static class Authentication
    {
        public const string JwtSection = "JwtSettings";
    }

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Fisherman = "Fisherman";
    }

    public static class Logging
    {
        public const string DbInitialized = "Database initialized successfully.";
    }
}
