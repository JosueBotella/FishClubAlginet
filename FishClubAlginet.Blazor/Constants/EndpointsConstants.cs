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
}
