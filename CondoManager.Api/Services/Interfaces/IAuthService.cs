using CondoManager.Api.DTOs.Auth;
using CondoManager.Api.Events;
using CondoManager.Entity.Models;
using System.Security.Claims;

namespace CondoManager.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> RegisterAsync(RegisterRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<User?> ValidateTokenAsync(string token);
        Task<ClaimsPrincipal?> GetPrincipalFromTokenAsync(string token);
        Task<bool> ValidateUserAsync(int userId);
        Task<TokenValidationResponse> ValidateAuthTokenAsync(string userId, string authToken);
        Task LogoutAsync(string userId, string authToken);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}