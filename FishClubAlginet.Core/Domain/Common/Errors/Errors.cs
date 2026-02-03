namespace FishClubAlginet.Core.Domain.Common.Errors;

public static partial class Errors
{
    // Agrupamos por entidad para mantener el orden
    public static class Fisherman
    {
        public static Error NotFound => Error.NotFound(
            code: "Fisherman.NotFound",
            description: "El pescador solicitado no existe en la base de datos.");

        public static Error DuplicateLicense => Error.Conflict(
            code: "Fisherman.DuplicateLicense",
            description: "Ya existe un pescador con esa licencia federativa.");

        public static Error InvalidDocument => Error.Validation(
            code: "Fisherman.InvalidDocument",
            description: "El número de documento no es válido para el tipo seleccionado.");
    }

    public static class Auth
    {
        public static Error InvalidCredentials => Error.Validation(
            code: "Auth.InvalidCredentials",
            description: "Las credenciales proporcionadas son incorrectas.");
    }
}
