using CondoManager.Api.DTOs.Chat;

namespace CondoManager.Api.Interfaces
{
    public interface IChatService
    {
        Task<MessageResponse> SendMessageAsync(SendMessageRequest request);
        Task<IEnumerable<MessageResponse>> GetMessagesAsync(Guid? apartmentId);
        Task<IEnumerable<MessageResponse>> GetUserMessagesAsync(Guid userId);
        Task<IEnumerable<ConversationResponse>> GetUserConversationsAsync(Guid userId);
        Task<IEnumerable<MessageResponse>> GetConversationMessagesAsync(Guid conversationId, int skip = 0, int take = 50);
        Task<bool> IsUserInConversationAsync(Guid userId, Guid conversationId);
    }
}