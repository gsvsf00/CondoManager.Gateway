using CondoManager.Api.DTOs.Auth;
using CondoManager.Api.Interfaces;
using CondoManager.Api.Models;
using CondoManager.Api.Events;
using Microsoft.AspNetCore.Mvc;

namespace CondoManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorResponse = ErrorResponse.ValidationError("Validation failed", Request.Path, errors);
                return BadRequest(errorResponse);
            }

            try
            {
                var response = await _authService.RegisterAsync(request);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                var errorResponse = ErrorResponse.BadRequest(ex.Message, Request.Path);
                return BadRequest(errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = ErrorResponse.InternalServerError("An error occurred during registration", Request.Path);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorResponse = ErrorResponse.ValidationError("Validation failed", Request.Path, errors);
                return BadRequest(errorResponse);
            }

            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                var errorResponse = ErrorResponse.Unauthorized(ex.Message, Request.Path);
                return Unauthorized(errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = ErrorResponse.InternalServerError("An error occurred during login", Request.Path);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromBody] TokenValidationRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorResponse = ErrorResponse.ValidationError("Validation failed", Request.Path, errors);
                return BadRequest(errorResponse);
            }

            try
            {
                var response = await _authService.ValidateAuthTokenAsync(request.UserId, request.AuthToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ErrorResponse.InternalServerError("An error occurred during token validation", Request.Path);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorResponse = ErrorResponse.ValidationError("Validation failed", Request.Path, errors);
                return BadRequest(errorResponse);
            }

            try
            {
                await _authService.LogoutAsync(request.UserId, request.AuthToken);
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                var errorResponse = ErrorResponse.InternalServerError("An error occurred during logout", Request.Path);
                return StatusCode(500, errorResponse);
            }
        }
    }
}
