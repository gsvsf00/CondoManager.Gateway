using System.ComponentModel.DataAnnotations;

namespace CondoManager.Api.DTOs.Chat
{
    public class SendMessageRequest
    {
        [Required]
        public Guid SenderId { get; set; }
        
        public Guid? ApartmentId { get; set; }
        
        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
        
        public bool IsAnnouncement { get; set; } = false;
    }
}