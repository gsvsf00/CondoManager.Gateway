using CondoManager.Api.DTOs.Auth;
using CondoManager.Api.Infrastructure;
using CondoManager.Api.Events;
using CondoManager.Entity.Models;
using CondoManager.Entity.Enums;
using CondoManager.Entity.Events;
using CondoManager.Repository.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CondoManager.Api.Services.Interfaces;

namespace CondoManager.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtTokenGenerator _jwtGenerator;
        private readonly RabbitMqPublisher _eventPublisher;
        private readonly IAuthenticationEventService _authEventService;

        public AuthService(IUnitOfWork unitOfWork, JwtTokenGenerator jwtGenerator, RabbitMqPublisher eventPublisher, IAuthenticationEventService authEventService)
        {
            _unitOfWork = unitOfWork;
            _jwtGenerator = jwtGenerator;
            _eventPublisher = eventPublisher;
            _authEventService = authEventService;
        }

        public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
        {
            if(string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.Phone))
            {
                throw new InvalidOperationException("Insert a Email or a Phone Number.");
            }

            if (!string.IsNullOrWhiteSpace(request.Email) && await _unitOfWork.Users.EmailExistsAsync(request.Email))
            {
                throw new InvalidOperationException("Email already in use");
            }
            
            if (!string.IsNullOrWhiteSpace(request.Phone) && await _unitOfWork.Users.PhoneExistsAsync(request.Phone))
            {
                throw new InvalidOperationException("Phone already in use");
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email,
                Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone,
                PasswordHash = HashPassword(request.Password),
                Roles = request.IsTrustee
                    ? UserRole.Resident | UserRole.Trustee
                    : UserRole.Resident,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Publish user registered event
            var userRegisteredEvent = new UserRegisteredEvent
            {
                UserId = user.Id,
                Email = user.Email,
                Phone = user.Phone,
                Name = user.FullName.Split(' ').FirstOrDefault() ?? ""
            };
            await _eventPublisher.PublishAsync(userRegisteredEvent, "user.registered");

            var token = _jwtGenerator.GenerateToken(user.Id, user.Email, user.Roles);
            
            // Generate a persistent auth token for the new user
            var authToken = GenerateAuthToken();
            
            // Update user with the auth token
            user.AuthToken = authToken;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return new LoginResponse
            {
                Access = token,
                Refresh = authToken,
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    Phone = user.Phone,
                    Name = user.FullName
                }
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            User? user = null;
            
            // Try to find user by email first if provided
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            }
            
            // If not found by email and phone is provided, try phone
            if (user == null && !string.IsNullOrWhiteSpace(request.Phone))
            {
                user = await _unitOfWork.Users.GetByPhoneAsync(request.Phone);
            }
            
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid login or password");
            }

            var token = _jwtGenerator.GenerateToken(user.Id, user.Email, user.Roles);
            
            // Generate a persistent auth token
            var authToken = GenerateAuthToken();
            
            // Update user with new auth token and last login
            user.AuthToken = authToken;
            user.LastLoginAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Publish authentication event
            var authEvent = new UserAuthenticatedEvent
            {
                UserId = user.Id.ToString(),
                UserName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = user.Roles.ToString(),
                AuthenticatedAt = DateTime.UtcNow,
                AuthToken = authToken,
                JwtToken = token
            };

            await _authEventService.PublishUserAuthenticatedAsync(authEvent);

            return new LoginResponse
            {
                Access = token,
                Refresh = authToken,
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    Phone = user.Phone,
                    Name = user.FullName,
                    Roles = user.Roles.ToString()
                }
            };
        }

        public async Task<User?> ValidateTokenAsync(string token)
        {
            var principal = await GetPrincipalFromTokenAsync(token);
            if (principal == null) return null;

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return null;

            return await _unitOfWork.Users.GetByIdAsync(userId);
        }

        public Task<ClaimsPrincipal?> GetPrincipalFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = _jwtGenerator.GetValidationParameters();
                
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return Task.FromResult<ClaimsPrincipal?>(principal);
            }
            catch
            {
                return Task.FromResult<ClaimsPrincipal?>(null);
            }
        }

        public async Task<bool> ValidateUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            return user != null;
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "CondoManagerSalt"));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }

        private string GenerateAuthToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public async Task<TokenValidationResponse> ValidateAuthTokenAsync(string userId, string authToken)
        {
            if (!int.TryParse(userId, out var userInt))
            {
                return new TokenValidationResponse { IsValid = false };
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userInt);
            if (user == null || user.AuthToken != authToken)
            {
                return new TokenValidationResponse { IsValid = false };
            }

            return new TokenValidationResponse
            {
                IsValid = true,
                UserId = user.Id.ToString(),
                UserName = user.FullName,
                Role = user.Roles.ToString(),
                CurrentToken = user.AuthToken ?? string.Empty
            };
        }

        public async Task LogoutAsync(string userId, string authToken)
        {
            if (!int.TryParse(userId, out var userInt))
                return;

            var user = await _unitOfWork.Users.GetByIdAsync(userInt);
            if (user != null && user.AuthToken == authToken)
            {
                user.AuthToken = null;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Publish logout event
                var logoutEvent = new UserLoggedOutEvent
                {
                    UserId = userId,
                    AuthToken = authToken,
                    LoggedOutAt = DateTime.UtcNow
                };

                await _authEventService.PublishUserLoggedOutAsync(logoutEvent);
            }
        }
    }
}