using CondoManager.Entity.Enums;

namespace CondoManager.Api.DTOs.Chat
{
    public class ConversationResponse
    {
        public Guid Id { get; set; }
        public ConversationType Type { get; set; }
        public string? Name { get; set; }
        public Guid? ApartmentId { get; set; }
        public string? ApartmentNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public bool IsActive { get; set; }
        public int UnreadCount { get; set; }
        public MessageResponse? LastMessage { get; set; }
        public List<ConversationParticipantResponse> Participants { get; set; } = new();
    }

    public class ConversationParticipantResponse
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}