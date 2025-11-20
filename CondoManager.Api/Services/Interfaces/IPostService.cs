using CondoManager.Api.DTOs.Posts;
using CondoManager.Entity.DTOs;

namespace CondoManager.Api.Services.Interfaces
{
    public interface IPostService
    {
        Task<PostResponseDTO> CreateAsync(PostRequestDTO request);
        Task<IEnumerable<PostResponseDTO>> GetPostsAsync();
        Task<PostResponseDTO?> GetPostAsync(int id);
    }
}
