using System.Text.Json.Serialization;

namespace CondoManager.Api.Models
{
    public class ErrorResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("details")]
        public object? Details { get; set; }

        public ErrorResponse()
        {
        }

        public ErrorResponse(string error, string message, int statusCode, string path = "")
        {
            Error = error;
            Message = message;
            StatusCode = statusCode;
            Path = path;
        }

        public static ErrorResponse BadRequest(string message, string path = "", object? details = null)
        {
            return new ErrorResponse("Bad Request", message, 400, path) { Details = details };
        }

        public static ErrorResponse Unauthorized(string message = "Unauthorized access", string path = "")
        {
            return new ErrorResponse("Unauthorized", message, 401, path);
        }

        public static ErrorResponse Forbidden(string message = "Access forbidden", string path = "")
        {
            return new ErrorResponse("Forbidden", message, 403, path);
        }

        public static ErrorResponse NotFound(string message = "Resource not found", string path = "")
        {
            return new ErrorResponse("Not Found", message, 404, path);
        }

        public static ErrorResponse Conflict(string message, string path = "")
        {
            return new ErrorResponse("Conflict", message, 409, path);
        }

        public static ErrorResponse ValidationError(string message, string path = "", object? details = null)
        {
            return new ErrorResponse("Validation Error", message, 422, path) { Details = details };
        }

        public static ErrorResponse InternalServerError(string message = "An internal server error occurred", string path = "")
        {
            return new ErrorResponse("Internal Server Error", message, 500, path);
        }
    }
}