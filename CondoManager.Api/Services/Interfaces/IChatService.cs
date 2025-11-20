using CondoManager.Api.DTOs.Chat;

namespace CondoManager.Api.Services.Interfaces
{
    public interface IChatService
    {
        Task<MessageResponse> SendMessageAsync(SendMessageRequest request);
        Task<IEnumerable<MessageResponse>> GetMessagesAsync(int? apartmentId);
        Task<IEnumerable<MessageResponse>> GetUserMessagesAsync(int userId);
        Task<IEnumerable<ConversationResponse>> GetUserConversationsAsync(int userId);
        Task<IEnumerable<MessageResponse>> GetConversationMessagesAsync(int conversationId, int skip = 0, int take = 50);
        Task<bool> IsUserInConversationAsync(int userId, int conversationId);
    }
}