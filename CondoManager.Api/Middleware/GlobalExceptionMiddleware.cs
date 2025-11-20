using CondoManager.Api.Models;
using CondoManager.Api.Services.Interfaces;
using System.Text.Json;

namespace CondoManager.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IServiceProvider serviceProvider)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                using var scope = _serviceProvider.CreateScope();
                var loggingService = scope.ServiceProvider.GetService<ILoggingService>();

                loggingService?.LogError("An unhandled exception occurred", ex, new
                {
                    RequestPath = context.Request.Path,
                    RequestMethod = context.Request.Method,
                    QueryString = context.Request.QueryString.ToString()
                });

                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var requestPath = context.Request.Path.Value ?? "";
            ErrorResponse response;

            switch (exception)
            {
                case ArgumentException:
                case InvalidOperationException:
                    response = ErrorResponse.BadRequest(exception.Message, requestPath);
                    break;
                case UnauthorizedAccessException:
                    response = ErrorResponse.Unauthorized(exception.Message, requestPath);
                    break;
                case KeyNotFoundException:
                    response = ErrorResponse.NotFound(exception.Message, requestPath);
                    break;
                default:
                    response = ErrorResponse.InternalServerError("An internal server error occurred", requestPath);
                    break;
            }

            context.Response.StatusCode = response.StatusCode;

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}