namespace FishClubAlginet.Blazor.Constants;

public class EndpointsConstants
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
    }

    public static class Fishermen
    {
        public const string Add = "api/fishermen/Add";
        public const string GetAll = "api/fishermen/GetAll";

        public static string GetAllPaged(int skip, int take, string? search = null)
        {
            var url = $"{GetAll}?skip={skip}&take={take}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";
            return url;
        }
    }

    public static class Users
    {
        public const string GetAll = "api/users";
        public const string Create = "api/users";
        public static string Block(string userId) => $"api/users/{userId}/block";
        public static string Unblock(string userId) => $"api/users/{userId}/unblock";
        public static string AssignRole(string userId) => $"api/users/{userId}/assign-role";
        public static string RemoveRole(string userId) => $"api/users/{userId}/remove-role";

        public static string GetAllPaged(int skip, int take, string? search = null)
        {
            var url = $"{GetAll}?skip={skip}&take={take}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";
            return url;
        }
    }

    public static class Users
    {
        public const string GetAll = "api/users";
        public const string Create = "api/users";
        public static string Block(string userId) => $"api/users/{userId}/block";
        public static string Unblock(string userId) => $"api/users/{userId}/unblock";
    }
}
