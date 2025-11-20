using CondoManager.Api.DTOs.Chat;
using CondoManager.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CondoManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [HttpPost("send")]
        [AllowAnonymous] // Temporarily allow anonymous access for testing
        public async Task<ActionResult<MessageResponse>> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _chatService.SendMessageAsync(request);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation while sending message: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, "An error occurred while sending the message");
            }
        }

        [HttpGet("messages")]
        public async Task<ActionResult<IEnumerable<MessageResponse>>> GetMessages([FromQuery] int? apartmentId = null)
        {
            try
            {
                var messages = await _chatService.GetMessagesAsync(apartmentId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages");
                return StatusCode(500, "An error occurred while retrieving messages");
            }
        }

        [HttpGet("user/messages")]
        public async Task<ActionResult<IEnumerable<MessageResponse>>> GetUserMessages()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("Invalid user token");
                }

                var messages = await _chatService.GetUserMessagesAsync(userId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user messages for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, "An error occurred while retrieving your messages");
            }
        }

        [HttpGet("user/conversations")]
        public async Task<ActionResult<IEnumerable<ConversationResponse>>> GetUserConversations()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("Invalid user token");
                }

                var conversations = await _chatService.GetUserConversationsAsync(userId);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user conversations for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, "An error occurred while retrieving your conversations");
            }
        }

        [HttpGet("conversation/{conversationId}/messages")]
        public async Task<ActionResult<IEnumerable<MessageResponse>>> GetConversationMessages(int conversationId, [FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("Invalid user token");
                }

                // Validate that user is part of the conversation
                var isUserInConversation = await _chatService.IsUserInConversationAsync(userId, conversationId);
                if (!isUserInConversation)
                {
                    return Forbid("You don't have access to this conversation");
                }

                var messages = await _chatService.GetConversationMessagesAsync(conversationId, skip, take);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation messages for conversation {ConversationId}", conversationId);
                return StatusCode(500, "An error occurred while retrieving conversation messages");
            }
        }
    }
}