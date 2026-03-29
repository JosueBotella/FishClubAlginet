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
    }

    public static class Users
    {
        public const string GetAll = "api/users";
        public const string Create = "api/users";
        public static string Block(string userId) => $"api/users/{userId}/block";
        public static string Unblock(string userId) => $"api/users/{userId}/unblock";
    }
}
