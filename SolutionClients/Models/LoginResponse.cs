namespace SolutionClients.Models
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = "";
        public string Message { get; set; } = "";
        public UserInfo? User { get; set; }
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string Name { get; set; } = "";
    }
}