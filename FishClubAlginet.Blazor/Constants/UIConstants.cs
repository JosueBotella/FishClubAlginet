namespace FishClubAlginet.Blazor.Constants;

public class UIConstants
{
    public const string TokenKey = "authToken";

    public static class BlazorSettings
    {
        public const string ApiSettingsSectionError = "La URL de la API no está configurada en appsettings.Development.json";
    }

    public static class Formats
    {
        public const string ShortDate = "dd/MM/yyyy";
    }

    public static class Notifications
    {
        public const string ErrorTitle = "Error";
        public const string ConnectionErrorTitle = "Error de Conexión";
    }

    public static class DocumentTypeNames
    {
        public const string Dni = "DNI";
        public const string Nie = "NIE";
        public const string Passport = "Pasaporte";
    }

    public static class FishermanForm
    {
        public const string PageTitle = "Nuevo Pescador";
        public const string PersonalDataSection = "Datos Personales";
        public const string AddressSection = "Dirección";

        public const string FirstNameLabel = "Nombre *";
        public const string FirstNamePlaceholder = "Nombre";
        public const string LastNameLabel = "Apellidos *";
        public const string LastNamePlaceholder = "Apellidos";
        public const string DateOfBirthLabel = "Fecha de Nacimiento *";
        public const string DocumentTypeLabel = "Tipo Documento *";
        public const string DocumentNumberLabel = "Nº Documento *";
        public const string DocumentNumberPlaceholder = "Número de documento";
        public const string FederationLicenseLabel = "Licencia Federativa";
        public const string FederationLicensePlaceholder = "Opcional";

        public const string StreetLabel = "Calle";
        public const string StreetPlaceholder = "Calle";
        public const string CityLabel = "Ciudad";
        public const string CityPlaceholder = "Ciudad";
        public const string ZipCodeLabel = "Código Postal";
        public const string ZipCodePlaceholder = "Código Postal";
        public const string ProvinceLabel = "Provincia";
        public const string ProvincePlaceholder = "Provincia";

        public const string SaveButtonText = "Guardar Pescador";
        public const string CancelButtonText = "Cancelar";

        public const string CreatedTitle = "Pescador creado";
        public const string CreatedMessage = "El pescador se ha registrado correctamente.";
        public const string CreateErrorPrefix = "No se pudo crear el pescador: ";
    }
}
