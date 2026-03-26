namespace FishClubAlginet.Core.Domain.Common.Errors;

public static partial class Errors
{
    public static class FishermanErrors
    {
        public static Error NotFound => Error.NotFound(
            code: "Fisherman.NotFound",
            description: "The requested fisherman does not exist.");

        public static Error DuplicateLicense => Error.Conflict(
            code: "Fisherman.DuplicateLicense",
            description: "A fisherman with this federation license already exists.");

        public static Error InvalidDocument => Error.Validation(
            code: "Fisherman.InvalidDocument",
            description: "The document number is not valid for the selected document type.");
    }

    public static class Auth
    {
        public static Error InvalidCredentials => Error.Validation(
            code: "Auth.InvalidCredentials",
            description: "The provided credentials are invalid.");
    }
}
