namespace FishClubAlginet.Blazor.Constants;

public class UIConstants
{
    public const string TokenKey = "authToken";

    public static class BlazorSettings
    {
        public const string ApiSettingsSectionError = "The API URL is not configured in appsettings.Development.json";
    }

    public static class Formats
    {
        public const string ShortDate = "dd/MM/yyyy";
    }

    public static class Notifications
    {
        public const string ErrorTitle = "Error";
        public const string ConnectionErrorTitle = "Connection Error";
    }

    public static class DocumentTypeNames
    {
        public const string Dni = "DNI";
        public const string Nie = "NIE";
        public const string Passport = "Passport";
    }

    public static class FishermanForm
    {
        public const string PageTitle = "New Fisherman";
        public const string PersonalDataSection = "Personal Data";
        public const string AddressSection = "Address";

        public const string FirstNameLabel = "First Name *";
        public const string FirstNamePlaceholder = "First name";
        public const string LastNameLabel = "Last Name *";
        public const string LastNamePlaceholder = "Last name";
        public const string DateOfBirthLabel = "Date of Birth *";
        public const string DocumentTypeLabel = "Document Type *";
        public const string DocumentNumberLabel = "Document No. *";
        public const string DocumentNumberPlaceholder = "Document number";
        public const string FederationLicenseLabel = "Federation License";
        public const string FederationLicensePlaceholder = "Optional";

        public const string StreetLabel = "Street";
        public const string StreetPlaceholder = "Street";
        public const string CityLabel = "City";
        public const string CityPlaceholder = "City";
        public const string ZipCodeLabel = "Zip Code";
        public const string ZipCodePlaceholder = "Zip code";
        public const string ProvinceLabel = "Province";
        public const string ProvincePlaceholder = "Province";

        public const string SaveButtonText = "Save Fisherman";
        public const string CancelButtonText = "Cancel";

        public const string CreatedTitle = "Fisherman created";
        public const string CreatedMessage = "The fisherman has been registered successfully.";
        public const string CreateErrorPrefix = "Could not create the fisherman: ";
    }

    public static class UserManagement
    {
        public const string PageTitle = "User Management";
        public const string NewUserButton = "New User";
        public const string CreateDialogTitle = "Create User";

        public const string EmailLabel = "Email *";
        public const string EmailPlaceholder = "user@example.com";
        public const string PasswordLabel = "Password *";
        public const string PasswordPlaceholder = "Password";
        public const string RoleLabel = "Role *";

        public const string SaveButtonText = "Create";
        public const string CancelButtonText = "Cancel";
        public const string BlockButtonText = "Block";
        public const string UnblockButtonText = "Unblock";

        public const string ColEmail = "Email";
        public const string ColRoles = "Roles";
        public const string ColStatus = "Status";
        public const string ColActions = "Actions";

        public const string StatusActive = "Active";
        public const string StatusBlocked = "Blocked";

        public const string CreatedTitle = "User created";
        public const string CreatedMessage = "The user has been created successfully.";
        public const string CreateErrorPrefix = "Could not create the user: ";

        public const string BlockedTitle = "User blocked";
        public const string BlockedMessage = "The user has been blocked.";
        public const string BlockErrorPrefix = "Could not block the user: ";

        public const string UnblockedTitle = "User unblocked";
        public const string UnblockedMessage = "The user has been unblocked.";
        public const string UnblockErrorPrefix = "Could not unblock the user: ";
    }
}
