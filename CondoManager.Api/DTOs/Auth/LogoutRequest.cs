using System.ComponentModel.DataAnnotations;

namespace CondoManager.Api.DTOs.Auth
{
    public class LogoutRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string AuthToken { get; set; } = string.Empty;
    }
}