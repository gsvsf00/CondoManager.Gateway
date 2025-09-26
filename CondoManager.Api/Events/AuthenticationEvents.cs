namespace CondoManager.Api.Events
{
    public class UserAuthenticatedEvent
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime AuthenticatedAt { get; set; }
        public string AuthToken { get; set; } = string.Empty;
        public string JwtToken { get; set; } = string.Empty;
    }

    public class UserLoggedOutEvent
    {
        public string UserId { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public DateTime LoggedOutAt { get; set; }
    }

    public class UserTokenChangedEvent
    {
        public string UserId { get; set; } = string.Empty;
        public string OldAuthToken { get; set; } = string.Empty;
        public string NewAuthToken { get; set; } = string.Empty;
        public string JwtToken { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }

    public class TokenValidationRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
    }

    public class TokenValidationResponse
    {
        public bool IsValid { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string CurrentToken { get; set; } = string.Empty;
    }
}