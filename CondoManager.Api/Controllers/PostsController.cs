using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CondoManager.Api.DTOs.Posts;
using CondoManager.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondoManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _service;

        public PostsController(IPostService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostResponseDTO>>> GetPosts()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return BadRequest(new { Message = "Invalid search data", Errors = errors });
            }

            try
            {
                var posts = await _service.GetPostsAsync();
                return Ok(posts);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = "No posts found", Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PostResponseDTO>> GetPost(int id)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return BadRequest(new { Message = "Invalid search data", Errors = errors });
            }

            try
            {
                var post = await _service.GetPostAsync(id);
                
                if (post is null) 
                    return NotFound(new { Message = "No post found" });
                
                return Ok(post);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = "Invalid operation", Error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpPost("add")]
        [Authorize(Roles = "Admin,Trustee")]
        public async Task<ActionResult<PostResponseDTO>> AddPost([FromBody] PostRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return BadRequest(new { Message = "Invalid search data", Errors = errors });
            }
            
            try
            {
                var created = await _service.CreateAsync(request);
                return CreatedAtAction(nameof(GetPosts), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = "Invalid operation", Error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }
    }
}
