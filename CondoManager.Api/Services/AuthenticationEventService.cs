using CondoManager.Api.Events;
using CondoManager.Api.Infrastructure;

namespace CondoManager.Api.Services
{
    public interface IAuthenticationEventService
    {
        Task PublishUserAuthenticatedAsync(UserAuthenticatedEvent authEvent);
        Task PublishUserLoggedOutAsync(UserLoggedOutEvent logoutEvent);
        Task PublishUserTokenChangedAsync(UserTokenChangedEvent tokenChangedEvent);
    }

    public class AuthenticationEventService : IAuthenticationEventService
    {
        private readonly RabbitMqPublisher _publisher;
        private readonly ILogger<AuthenticationEventService> _logger;

        public AuthenticationEventService(RabbitMqPublisher publisher, ILogger<AuthenticationEventService> logger)
        {
            _publisher = publisher;
            _logger = logger;
        }

        public async Task PublishUserAuthenticatedAsync(UserAuthenticatedEvent authEvent)
        {
            try
            {
                await _publisher.PublishAsync(authEvent, "auth.user.authenticated");
                _logger.LogInformation("Published user authenticated event for user {UserId}", authEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish user authenticated event for user {UserId}", authEvent.UserId);
                throw;
            }
        }

        public async Task PublishUserLoggedOutAsync(UserLoggedOutEvent logoutEvent)
        {
            try
            {
                await _publisher.PublishAsync(logoutEvent, "auth.user.loggedout");
                _logger.LogInformation("Published user logged out event for user {UserId}", logoutEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish user logged out event for user {UserId}", logoutEvent.UserId);
                throw;
            }
        }

        public async Task PublishUserTokenChangedAsync(UserTokenChangedEvent tokenChangedEvent)
        {
            try
            {
                await _publisher.PublishAsync(tokenChangedEvent, "auth.user.refreshed");
                _logger.LogInformation("Published user token changed event for user {UserId}", tokenChangedEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish user token changed event for user {UserId}", tokenChangedEvent.UserId);
                throw;
            }
        }
    }
}