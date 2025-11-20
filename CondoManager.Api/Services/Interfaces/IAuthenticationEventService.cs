using CondoManager.Api.Events;

namespace CondoManager.Api.Services.Interfaces
{
    public interface IAuthenticationEventService
    {
        Task PublishUserAuthenticatedAsync(UserAuthenticatedEvent authEvent);
        Task PublishUserLoggedOutAsync(UserLoggedOutEvent logoutEvent);
        Task PublishUserTokenChangedAsync(UserTokenChangedEvent tokenChangedEvent);
    }
}
