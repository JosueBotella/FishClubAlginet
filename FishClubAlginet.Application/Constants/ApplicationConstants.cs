namespace FishClubAlginet.Application.Constants;

public static class ApplicationConstants
{
    public static class BlazorSettings
    {
        public const string ApiSettingsSectionError = "La URL de la API no está configurada en appsettings.Development.json";
    }

    public static class Database
    {
        public const string ConnectionName = "TodoListContext";
        public const string MigrationErrorMessage = "Error durante la migración de base de datos.";
    }

    public static class Authentication
    {
        public const string JwtSection = "JwtSettings";
        public const string LoginFailed = "Intento de inicio de sesión fallido.";
        public const string RegisterSuccess = "Usuario creado con éxito.";
        public const string RegisterError = "Ocurrió un error al registrar el usuario.";
        public const string Unauthorized = "No tienes permiso para acceder a este recurso.";
    }

    public static class Logging
    {
        public const string DbInitialized = "Base de datos inicializada correctamente.";
        public const string ProcessStarted = "Iniciando proceso de {0}...";
    }


    public static class Endpoints
    {
        public static class Account
        {
            public const string Login = "api/account/login";
            public const string Register = "api/account/register";
        }

        public static class TodoList
        {
            public const string GetAll = "api/todolist";
            public const string Create = "api/todolist";
            // etc...
        }
    }
}
