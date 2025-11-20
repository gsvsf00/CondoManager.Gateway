using CondoManager.Api.Attributes;
using CondoManager.Api.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CondoManager.Api.DTOs.Auth
{
    [ModelBinder(BinderType = typeof(LoginRequestBinder))]
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
