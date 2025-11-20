using CondoManager.Entity.Events;

namespace CondoManager.Api.Services.Interfaces
{
    public interface IMessageEventHandler
    {
        Task HandleMessageReceivedAsync(MessageReceivedEvent messageEvent);
        Task HandleMessageReadAsync(MessageReadEvent readEvent);
        Task HandleMessageDeliveredAsync(MessageDeliveredEvent deliveredEvent);
    }
}
