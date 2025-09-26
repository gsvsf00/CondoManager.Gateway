using System.ComponentModel.DataAnnotations;
using CondoManager.Api.Attributes;

namespace CondoManager.Api.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [EmailValidation]
        public string? Email { get; set; } = string.Empty;

        [PhoneValidation]
        public string? Phone { get; set; } = string.Empty;

        [PasswordValidation]
        public string Password { get; set; } = string.Empty;

        public bool IsTrustee { get; set; }
    }
}
