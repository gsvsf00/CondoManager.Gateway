namespace CondoManager.Api.DTOs.Auth
{
    public class LoginResponse
    {
        public string Access { get; set; } = string.Empty;
        public string Refresh { get; set; } = string.Empty;
        public UserInfo? User { get; set; }
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Name { get; set; }
        public string Roles { get; set; }
    }
}
