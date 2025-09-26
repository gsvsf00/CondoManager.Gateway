namespace CondoManager.Api.DTOs.Chat
{
    public class MessageResponse
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid? ApartmentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsAnnouncement { get; set; }
    }
}