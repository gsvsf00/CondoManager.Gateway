using System.ComponentModel.DataAnnotations;

namespace CondoManager.Api.DTOs.Posts
{
    public class PostRequestDTO
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Body { get; set; } = string.Empty;
    }
}
