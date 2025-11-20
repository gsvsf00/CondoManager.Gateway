namespace CondoManager.Api.DTOs.Chat
{
    public class MessageResponse
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int? ApartmentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsAnnouncement { get; set; }
    }
}