using System.Text.Json;

namespace CondoManager.Api.Services
{
    public interface ILoggingService
    {
        void LogInfo(string message, object? data = null);
        void LogWarning(string message, object? data = null);
        void LogError(string message, Exception? exception = null, object? data = null);
        void LogDebug(string message, object? data = null);
    }

    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger;
        }

        public void LogInfo(string message, object? data = null)
        {
            var logMessage = FormatMessage(message, data);
            _logger.LogInformation(logMessage);
        }

        public void LogWarning(string message, object? data = null)
        {
            var logMessage = FormatMessage(message, data);
            _logger.LogWarning(logMessage);
        }

        public void LogError(string message, Exception? exception = null, object? data = null)
        {
            var logMessage = FormatMessage(message, data);
            if (exception != null)
            {
                _logger.LogError(exception, logMessage);
            }
            else
            {
                _logger.LogError(logMessage);
            }
        }

        public void LogDebug(string message, object? data = null)
        {
            var logMessage = FormatMessage(message, data);
            _logger.LogDebug(logMessage);
        }

        private string FormatMessage(string message, object? data)
        {
            if (data == null)
            {
                return message;
            }

            try
            {
                var serializedData = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });
                return $"{message} | Data: {serializedData}";
            }
            catch (Exception ex)
            {
                return $"{message} | Data serialization failed: {ex.Message}";
            }
        }
    }
}