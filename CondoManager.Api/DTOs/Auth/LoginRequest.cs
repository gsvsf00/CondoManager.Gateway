using System.ComponentModel.DataAnnotations;
using CondoManager.Api.Attributes;

namespace CondoManager.Api.DTOs.Auth
{
    public class LoginRequest
    {
        [EmailValidation]
        public string? Email { get; set; } = string.Empty;

        [PhoneValidation]
        public string? Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}
