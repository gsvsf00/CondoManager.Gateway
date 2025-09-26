namespace CondoManager.Api.DTOs.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? Phone { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
    }
}
