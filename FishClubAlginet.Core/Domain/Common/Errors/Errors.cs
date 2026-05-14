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

    public static class League
    {
        public static Error NotFound => Error.NotFound(
            code: "League.NotFound",
            description: "The requested league does not exist.");

        public static Error DuplicateYear => Error.Conflict(
            code: "League.DuplicateYear",
            description: "A league for this year already exists.");

        public static Error AlreadyActive => Error.Conflict(
            code: "League.AlreadyActive",
            description: "The league is already active.");

        public static Error AlreadyArchived => Error.Conflict(
            code: "League.AlreadyArchived",
            description: "The league is already archived.");

        public static Error CannotModifyArchived => Error.Validation(
            code: "League.CannotModifyArchived",
            description: "Cannot modify an archived league.");
    }

    public static class Auth
    {
        public static Error InvalidCredentials => Error.Validation(
            code: "Auth.InvalidCredentials",
            description: "The provided credentials are invalid.");
    }

    public static class Competition
    {
        public static Error NotFound => Error.NotFound(
            code: "Competition.NotFound",
            description: "The requested competition does not exist.");

        public static Error DuplicateNumber => Error.Conflict(
            code: "Competition.DuplicateNumber",
            description: "A competition with this number already exists in the league.");

        public static Error RegistrationNotOpen => Error.Validation(
            code: "Competition.RegistrationNotOpen",
            description: "This competition is not open for registration.");

        public static Error AlreadyRegistered => Error.Conflict(
            code: "Competition.AlreadyRegistered",
            description: "This fisherman is already registered for this competition.");

        public static Error InvalidStatusTransition => Error.Validation(
            code: "Competition.InvalidStatusTransition",
            description: "This status transition is not allowed.");
    }
}
