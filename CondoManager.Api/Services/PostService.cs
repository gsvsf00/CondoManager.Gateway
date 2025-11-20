using CondoManager.Api.DTOs.Posts;
using CondoManager.Api.Infrastructure;
using CondoManager.Api.Services.Interfaces;
using CondoManager.Entity.DTOs;
using CondoManager.Entity.Models;
using Microsoft.EntityFrameworkCore;

namespace CondoManager.Api.Services
{
    public class PostService : IPostService
    {
        private readonly CondoContext _db;

        public PostService(CondoContext db)
        {
            _db = db;
        }

        public async Task<PostResponseDTO> CreateAsync(PostRequestDTO request)  
        {
            var entity = new Post { Title = request.Title, Body = request.Body };

            _db.Posts.Add(entity);
            await _db.SaveChangesAsync();

            return new PostResponseDTO { Id = entity.Id, Title = entity.Title, Body = entity.Body };
        }

        public async Task<IEnumerable<PostResponseDTO>> GetPostsAsync()
        {
            var list = await _db.Posts.Where(p => p.DeletedAt == null).OrderByDescending(p => p.CreatedAt).ToListAsync();

            return list.Select(p => new PostResponseDTO { Id = p.Id, Title = p.Title, Body = p.Body, CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt });
        }

        public async Task<PostResponseDTO?> GetPostAsync(int id)
        {
            var post = await _db.Posts.FindAsync(id);
            return post is null ? null : new PostResponseDTO { Id = post.Id, Title = post.Title, Body = post.Body, CreatedAt = post.CreatedAt, UpdatedAt = post.UpdatedAt };
        }
    }
}
